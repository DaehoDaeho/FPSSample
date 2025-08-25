// EnemyFSMHybrid.cs
using UnityEngine;
using UnityEngine.AI;

public class EnemyFSMHybrid : MonoBehaviour
{
    // ���� ���� ������������������������������������������������������������������������������������������
    public Transform player;
    public string playerTag = "Player";
    public NavMeshAgent agent;
    public Transform firePoint;           // ���Ÿ� �߻� ��ġ(�ѱ�)
    public GameObject projectilePrefab;   // ���Ÿ� źȯ ������

    // ���� ����/��� ������������������������������������������������������������������������������
    public enum State { Idle, Chase, Attack }
    public enum AttackMode { MeleeOnly, RangedOnly, Hybrid }
    public AttackMode attackMode = AttackMode.Hybrid;

    private State currentState = State.Idle;

    // ���� �Ÿ�/ȸ�� �Ķ���� ������������������������������������������������������������
    public float detectRange = 18f;   // �� �Ÿ� �ȿ����� �ൿ
    public float meleeRange = 2.0f;  // ���� ���� ����
    public float rangedRange = 14f;   // ���Ÿ� ���� ����
    public float turnSpeed = 10f;   // �÷��̾� �ٶ󺸱� �ӵ�

    // �����׸��ý�(���� ����): ��ȯ ����
    public float meleeExitBuffer = 0.3f;  // ���� ���� ����
    public float rangedExitBuffer = 1.0f;  // ���Ÿ� ���� ����

    // ���� ���� ���� ������������������������������������������������������������������������������
    public float meleeDamage = 15f;
    public float hitRadius = 1.2f;       // �� ���� ���� ��
    public LayerMask playerMask;           // Player ���̾
    public float meleeCooldown = 1.2f;
    private float nextMeleeTime = 0f;

    // ���� ���Ÿ� ���� ��������������������������������������������������������������������������
    public float projectileSpeed = 20f;
    public float projectileDamage = 10f;
    public float rangedCooldown = 1.0f;
    private float nextRangedTime = 0f;

    // ���̺긮���� �� ���� ����Ÿ�� ���(���� ������)
    private bool usingMelee = false;   // true�� ����, false�� ���Ÿ�

    void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null && agent.stoppingDistance < 1.0f)
        {
            agent.stoppingDistance = 1.5f;
        }
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null)
            {
                player = p.transform;
            }
        }
    }

    void OnDisable()
    {
        // ��Ȱ�� ���¿��� ������Ʈ/ȸ���� �ǵ帮�� ����(�ڷ�ƾ�� ����)
        StopAllCoroutines();
    }

    void Update()
    {
        if (player == null || agent == null)
        {
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        // 1) ���� ����
        if (dist > detectRange)
        {
            currentState = State.Idle;
        }

        else if (ShouldAttack(dist) == true)
        {
            currentState = State.Attack;
        }
        else
        {
            currentState = State.Chase;
        }

        // 2) ���º� ó��
        if (currentState == State.Idle)
        {
            agent.isStopped = true;
        }
        else if (currentState == State.Chase)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else // Attack
        {
            agent.isStopped = true;
            FaceTarget();

            // � ������ ���� �����ϰ� ����
            if (attackMode == AttackMode.MeleeOnly)
            {
                TryMelee();
            }
            else if (attackMode == AttackMode.RangedOnly)
            {
                TryRanged();
            }
            else // Hybrid
            {
                DecideHybrid(dist); // usingMelee ���� ����
                if (usingMelee == true)
                {
                    TryMelee();
                }

                else TryRanged();
            }
        }
    }

    // ���� ���� ��ƿ ��������������������������������������������������������������������������������
    void FaceTarget()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
        }
    }

    bool ShouldAttack(float dist)
    {
        if (attackMode == AttackMode.MeleeOnly)
        {
            return dist <= meleeRange;
        }

        if (attackMode == AttackMode.RangedOnly)
        {
            return dist <= rangedRange;
        }

        // Hybrid: ��� �ϳ��� ���� ���̸� ���� ����
        return dist <= rangedRange;
    }

    void DecideHybrid(float dist)
    {
        // �̹� ���� ���̸� ���� �� �־��� ������ ���� ����
        if (usingMelee)
        {
            if (dist > meleeRange + meleeExitBuffer)
            {
                usingMelee = false; // ���� ���� �� ���Ÿ� ������
            }

            return;
        }
        else // ���Ÿ� ���̸�, ����� ��������� ���� �������� ��ȯ
        {
            // ���Ÿ� ���� ����: rangedRange + rangedExitBuffer �� �ѱ� ������ ����
            if (dist <= meleeRange)
            {
                usingMelee = true;
            }
            // (dist�� rangedRange���� ���� Ŀ���ٰ� �پ��� ��鸲�� Chase ���¿��� �ڿ����� �ؼ�)
        }
    }

    // ���� ���� ���� ��������������������������������������������������������������������������������
    void TryMelee()
    {
        if (Time.time < nextMeleeTime)
        {
            return;
        }

        // �� ���ʿ� ���� ���� �׷��� Player�� ���Դ��� Ȯ��
        Vector3 center = transform.position + transform.forward * (meleeRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerStatus ps = hits[i].GetComponent<PlayerStatus>();
            if (ps != null)
            {
                ps.TakeDamage(meleeDamage);
                break; // �� ����
            }
        }
        nextMeleeTime = Time.time + meleeCooldown;
        // (����) �ִ�/���� Ʈ���� ����
    }

    // ���� ���Ÿ� ���� ����������������������������������������������������������������������������
    void TryRanged()
    {
        if (Time.time < nextRangedTime)
        {
            return;
        }

        if (projectilePrefab == null || firePoint == null)
        {
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Projectile�� �� �����(�ܼ� ���� �ʵ� ����)
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.speed = projectileSpeed;
            p.damage = projectileDamage;
        }

        nextRangedTime = Time.time + rangedCooldown;
        // (����) ���� �÷���/���� ����
    }

    // ���� ����� ǥ�� ����������������������������������������������������������������������������
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, rangedRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
