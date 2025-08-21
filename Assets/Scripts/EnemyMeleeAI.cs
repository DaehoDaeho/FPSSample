// EnemyMeleeAI.cs
using UnityEngine;
using UnityEngine.AI;

public class EnemyMeleeAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;         // 비어 있으면 Tag로 자동 탐색
    public string playerTag = "Player";

    [Header("Ranges & Damage")]
    public float detectRange = 15f;  // 이 거리 내에 있으면 쫓아감(여유값)
    public float attackRange = 2.0f; // 이 거리 안이면 공격 가능
    public float attackDamage = 15f; // 한 번 때릴 때 데미지
    public float hitRadius = 1.2f;   // 내 앞쪽 작은 원 반지름(공격 판정)

    [Header("Cooldown")]
    public float attackCooldown = 1.2f; // 공격 후 쉬는 시간(초)
    private float nextAttackTime = 0f;  // 이 시간 이후에 다시 공격 가능

    [Header("Look & Move")]
    public float turnSpeed = 10f;  // 플레이어 바라볼 때 회전 속도

    [Header("Layer Mask")]
    public LayerMask playerMask;   // Player 레이어만 맞추도록 설정

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // 멈출 거리를 적당히 두면 서로 밀착해서 떨리는 일이 줄어듭니다.
        if (agent != null && agent.stoppingDistance < 0.1f)
            agent.stoppingDistance = 1.5f;
    }

    void Start()
    {
        // 플레이어 자동 찾기(Inspector에 안 넣었을 때)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null || agent == null) return;

        // 플레이어까지 거리
        float dist = Vector3.Distance(transform.position, player.position);

        // 1) 멀면 추적
        if (dist > attackRange && dist <= detectRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            return;
        }

        // 2) 너무 멀어 추적도 안 할 정도면 멈춤(여기서는 간단히 멈추기만)
        if (dist > detectRange)
        {
            agent.isStopped = true;
            return;
        }

        // 3) 공격 범위에 들어왔다면 멈추고 바라보기
        if (dist <= attackRange)
        {
            agent.isStopped = true;

            // 수평으로만 플레이어를 바라보게(위아래 흔들림 방지)
            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
            }

            // 4) 쿨타임이 끝났다면 한 번 때리기
            if (Time.time >= nextAttackTime)
            {
                DoMeleeHit();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    // 실제 한 번 때리는 동작(OverlapSphere로 작은 원을 그려서 맞았는지 확인)
    void DoMeleeHit()
    {
        // 내 위치에서 약간 앞쪽(허리~팔 길이 정도) 지점을 중심으로 작은 원
        Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);

        // 원 안에 들어온 "플레이어 레이어" 콜라이더들
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerStatus ps = hits[i].GetComponent<PlayerStatus>();
            if (ps != null)
            {
                ps.TakeDamage(attackDamage); // 실제로 체력 깎기
                // 한 번만 때리고 끝내려면 break 해도 됨(원 안에 플레이어 하나만 있다고 가정)
                break;
            }
        }

        // (선택) 여기서 공격 애니메이션/사운드를 재생해도 됩니다.
        // Animator가 있다면: animator.SetTrigger("Attack");
    }

    // 에디터에서 범위 확인용(장면 뷰에서 선택했을 때 보임)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);
        Gizmos.DrawWireSphere(center, hitRadius);
    }
}
