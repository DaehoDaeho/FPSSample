// EnemyFSMHybrid.cs
using UnityEngine;
using UnityEngine.AI;

public class EnemyFSMHybrid : MonoBehaviour
{
    // ── 참조 ─────────────────────────────────────────────
    public Transform player;
    public string playerTag = "Player";
    public NavMeshAgent agent;
    public Transform firePoint;           // 원거리 발사 위치(총구)
    public GameObject projectilePrefab;   // 원거리 탄환 프리팹

    // ── 상태/모드 ───────────────────────────────────────
    public enum State { Idle, Chase, Attack }
    public enum AttackMode { MeleeOnly, RangedOnly, Hybrid }
    public AttackMode attackMode = AttackMode.Hybrid;

    private State currentState = State.Idle;

    // ── 거리/회전 파라미터 ──────────────────────────────
    public float detectRange = 18f;   // 이 거리 안에서만 행동
    public float meleeRange = 2.0f;  // 근접 공격 가능
    public float rangedRange = 14f;   // 원거리 공격 가능
    public float turnSpeed = 10f;   // 플레이어 바라보기 속도

    // 히스테리시스(덜덜 방지): 전환 여유
    public float meleeExitBuffer = 0.3f;  // 근접 유지 여유
    public float rangedExitBuffer = 1.0f;  // 원거리 유지 여유

    // ── 근접 공격 ───────────────────────────────────────
    public float meleeDamage = 15f;
    public float hitRadius = 1.2f;       // 내 앞의 작은 원
    public LayerMask playerMask;           // Player 레이어만
    public float meleeCooldown = 1.2f;
    private float nextMeleeTime = 0f;

    // ── 원거리 공격 ─────────────────────────────────────
    public float projectileSpeed = 20f;
    public float projectileDamage = 10f;
    public float rangedCooldown = 1.0f;
    private float nextRangedTime = 0f;

    // 하이브리드일 때 현재 서브타입 기억(덜덜 방지용)
    private bool usingMelee = false;   // true면 근접, false면 원거리

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
        // 비활성 상태에서 에이전트/회전은 건드리지 않음(코루틴만 정리)
        StopAllCoroutines();
    }

    void Update()
    {
        if (player == null || agent == null)
        {
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        // 1) 상태 결정
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

        // 2) 상태별 처리
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

            // 어떤 공격을 할지 결정하고 실행
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
                DecideHybrid(dist); // usingMelee 값을 갱신
                if (usingMelee == true)
                {
                    TryMelee();
                }

                else TryRanged();
            }
        }
    }

    // ── 공통 유틸 ────────────────────────────────────────
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

        // Hybrid: 어느 하나라도 범위 안이면 공격 상태
        return dist <= rangedRange;
    }

    void DecideHybrid(float dist)
    {
        // 이미 근접 중이면 조금 더 멀어질 때까지 근접 유지
        if (usingMelee)
        {
            if (dist > meleeRange + meleeExitBuffer)
            {
                usingMelee = false; // 근접 해제 → 원거리 쪽으로
            }

            return;
        }
        else // 원거리 중이면, 충분히 가까워졌을 때만 근접으로 전환
        {
            // 원거리 유지 여유: rangedRange + rangedExitBuffer 를 넘기 전까진 유지
            if (dist <= meleeRange)
            {
                usingMelee = true;
            }
            // (dist가 rangedRange보다 조금 커졌다가 줄어드는 흔들림은 Chase 상태에서 자연스레 해소)
        }
    }

    // ── 근접 공격 ────────────────────────────────────────
    void TryMelee()
    {
        if (Time.time < nextMeleeTime)
        {
            return;
        }

        // 내 앞쪽에 작은 원을 그려서 Player가 들어왔는지 확인
        Vector3 center = transform.position + transform.forward * (meleeRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerStatus ps = hits[i].GetComponent<PlayerStatus>();
            if (ps != null)
            {
                ps.TakeDamage(meleeDamage);
                break; // 한 번만
            }
        }
        nextMeleeTime = Time.time + meleeCooldown;
        // (선택) 애니/사운드 트리거 가능
    }

    // ── 원거리 공격 ──────────────────────────────────────
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

        // Projectile에 값 덮어쓰기(단순 공개 필드 가정)
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.speed = projectileSpeed;
            p.damage = projectileDamage;
        }

        nextRangedTime = Time.time + rangedCooldown;
        // (선택) 머즐 플래시/사운드 가능
    }

    // ── 디버그 표시 ──────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, rangedRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
