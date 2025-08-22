// EnemyMeleeAI.cs (SAFE)
// ��Ȱ��ȭ�Ǹ�(agent/ȸ��) ���� �ǵ帮�� �ʵ��� OnDisable���� �ڷ�ƾ�� ����.
// �̵�/ȸ��/������ Ȱ�� ���¿����� �����.

using UnityEngine;
using UnityEngine.AI;

public class EnemyMeleeAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Ranges & Damage")]
    public float detectRange = 15f;
    public float attackRange = 2.0f;
    public float attackDamage = 15f;
    public float hitRadius = 1.2f;

    [Header("Cooldown")]
    public float attackCooldown = 1.2f;
    private float nextAttackTime = 0f;

    [Header("Look & Move")]
    public float turnSpeed = 10f;

    [Header("Layer Mask")]
    public LayerMask playerMask;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.stoppingDistance < 1.0f)
            agent.stoppingDistance = 1.5f;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }
    }

    void OnDisable()
    {
        // �߿�: ��Ȱ��ȭ �ÿ��� ���� agent/ȸ���� ������ �ʴ´�.
        // (���� ���� �� �ִ� �ڷ�ƾ�� ����)
        StopAllCoroutines();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // ��Ÿ� ��: (���⼱ ������) ���߱⸸
        if (dist > detectRange)
        {
            agent.isStopped = true;
            return;
        }

        // ��Ÿ� ���� ����
        if (dist > attackRange && dist <= detectRange)
        {
            // ����
            agent.isStopped = false;
            agent.SetDestination(player.position);
            return;
        }

        // ���� �Ÿ�: ���߰� �ٶ� �� ��Ÿ���̸� �� �� ������
        // (���⼭�� ȸ��/���� ����, ��ũ��Ʈ�� ������ Update ��ü�� ���� ����)
        if (dist <= attackRange)
        {
            agent.isStopped = true;

            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
            }

            if (Time.time >= nextAttackTime)
            {
                DoMeleeHit();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void DoMeleeHit()
    {
        Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerStatus ps = hits[i].GetComponent<PlayerStatus>();
            if (ps != null)
            {
                ps.TakeDamage(attackDamage);
                break; // �� ����
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);
        Gizmos.DrawWireSphere(center, hitRadius);
    }
}
