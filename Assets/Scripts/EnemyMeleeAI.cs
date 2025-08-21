// EnemyMeleeAI.cs
using UnityEngine;
using UnityEngine.AI;

public class EnemyMeleeAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;         // ��� ������ Tag�� �ڵ� Ž��
    public string playerTag = "Player";

    [Header("Ranges & Damage")]
    public float detectRange = 15f;  // �� �Ÿ� ���� ������ �Ѿư�(������)
    public float attackRange = 2.0f; // �� �Ÿ� ���̸� ���� ����
    public float attackDamage = 15f; // �� �� ���� �� ������
    public float hitRadius = 1.2f;   // �� ���� ���� �� ������(���� ����)

    [Header("Cooldown")]
    public float attackCooldown = 1.2f; // ���� �� ���� �ð�(��)
    private float nextAttackTime = 0f;  // �� �ð� ���Ŀ� �ٽ� ���� ����

    [Header("Look & Move")]
    public float turnSpeed = 10f;  // �÷��̾� �ٶ� �� ȸ�� �ӵ�

    [Header("Layer Mask")]
    public LayerMask playerMask;   // Player ���̾ ���ߵ��� ����

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // ���� �Ÿ��� ������ �θ� ���� �����ؼ� ������ ���� �پ��ϴ�.
        if (agent != null && agent.stoppingDistance < 0.1f)
            agent.stoppingDistance = 1.5f;
    }

    void Start()
    {
        // �÷��̾� �ڵ� ã��(Inspector�� �� �־��� ��)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null || agent == null) return;

        // �÷��̾���� �Ÿ�
        float dist = Vector3.Distance(transform.position, player.position);

        // 1) �ָ� ����
        if (dist > attackRange && dist <= detectRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            return;
        }

        // 2) �ʹ� �־� ������ �� �� ������ ����(���⼭�� ������ ���߱⸸)
        if (dist > detectRange)
        {
            agent.isStopped = true;
            return;
        }

        // 3) ���� ������ ���Դٸ� ���߰� �ٶ󺸱�
        if (dist <= attackRange)
        {
            agent.isStopped = true;

            // �������θ� �÷��̾ �ٶ󺸰�(���Ʒ� ��鸲 ����)
            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
            }

            // 4) ��Ÿ���� �����ٸ� �� �� ������
            if (Time.time >= nextAttackTime)
            {
                DoMeleeHit();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    // ���� �� �� ������ ����(OverlapSphere�� ���� ���� �׷��� �¾Ҵ��� Ȯ��)
    void DoMeleeHit()
    {
        // �� ��ġ���� �ణ ����(�㸮~�� ���� ����) ������ �߽����� ���� ��
        Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);

        // �� �ȿ� ���� "�÷��̾� ���̾�" �ݶ��̴���
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerStatus ps = hits[i].GetComponent<PlayerStatus>();
            if (ps != null)
            {
                ps.TakeDamage(attackDamage); // ������ ü�� ���
                // �� ���� ������ �������� break �ص� ��(�� �ȿ� �÷��̾� �ϳ��� �ִٰ� ����)
                break;
            }
        }

        // (����) ���⼭ ���� �ִϸ��̼�/���带 ����ص� �˴ϴ�.
        // Animator�� �ִٸ�: animator.SetTrigger("Attack");
    }

    // �����Ϳ��� ���� Ȯ�ο�(��� �信�� �������� �� ����)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);
        Gizmos.DrawWireSphere(center, hitRadius);
    }
}
