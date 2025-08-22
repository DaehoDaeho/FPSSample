// EnemyMeleeAI.cs (SAFE)
// 비활성화되면(agent/회전) 절대 건드리지 않도록 OnDisable에서 코루틴만 정리.
// 이동/회전/공격은 활성 상태에서만 수행됨.

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
        // 중요: 비활성화 시에는 절대 agent/회전을 만지지 않는다.
        // (남아 있을 수 있는 코루틴만 정리)
        StopAllCoroutines();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 사거리 밖: (여기선 간단히) 멈추기만
        if (dist > detectRange)
        {
            agent.isStopped = true;
            return;
        }

        // 사거리 안쪽 로직
        if (dist > attackRange && dist <= detectRange)
        {
            // 추적
            agent.isStopped = false;
            agent.SetDestination(player.position);
            return;
        }

        // 공격 거리: 멈추고 바라본 뒤 쿨타임이면 한 번 때리기
        // (여기서만 회전/정지 제어, 스크립트가 꺼지면 Update 자체가 돌지 않음)
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
                break; // 한 번만
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
