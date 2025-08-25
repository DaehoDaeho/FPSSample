// EnemyController_OO.cs
// 상태를 클래스로 분리(Idle/Chase/Attack), 공격 방식은 전략 클래스로 분리(근접/원거리).
// 프로퍼티/람다/이벤트 없이 공개 필드 + 메서드로만 구성.

using UnityEngine;
using UnityEngine.AI;

// ──────────────────────────────────────────────────────────────
// 2) 컨텍스트(공유 데이터 / 행동 유틸) - 적 본체
// ──────────────────────────────────────────────────────────────
public class EnemyController_OO : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public string playerTag = "Player";
    public NavMeshAgent agent;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Ranges")]
    public float detectRange = 18f;
    public float meleeRange = 2.0f;
    public float rangedRange = 14f;

    [Header("Turn")]
    public float turnSpeed = 10f;

    [Header("Hybrid Hysteresis")]
    public float meleeExitBuffer = 0.3f; // 근접 유지 여유
    public float rangedExitBuffer = 1.0f; // 원거리 유지 여유(필요시)

    public enum AttackAIMode { MeleeOnly, RangedOnly, Hybrid }
    public AttackAIMode attackMode = AttackAIMode.Hybrid;

    [Header("Melee")]
    public float meleeDamage = 15f;
    public float hitRadius = 1.2f;      // 내 앞쪽 작은 원
    public LayerMask playerMask;          // Player 레이어

    [Header("Melee Cooldown")]
    public float meleeCooldown = 1.2f;
    [HideInInspector] public float nextMeleeTime = 0f;

    [Header("Ranged")]
    public float projectileSpeed = 20f;
    public float projectileDamage = 10f;

    [Header("Ranged Cooldown")]
    public float rangedCooldown = 1.0f;
    [HideInInspector] public float nextRangedTime = 0f;

    // 하이브리드에서 현재 어떤 공격을 쓰는 중인지 기억(덜덜 방지)
    [HideInInspector] public bool usingMelee = false;

    // 현재 상태 인스턴스
    EnemyAIState currentState;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.stoppingDistance < 1.0f) agent.stoppingDistance = 1.5f;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        currentState = new IdleState();   // 시작 상태
        currentState.Enter(this);
    }

    void OnDisable()
    {
        // 비활성화 시 코루틴만 정리(회전/에이전트는 건드리지 않음)
        StopAllCoroutines();
    }

    void Update()
    {
        if (player == null || agent == null) return;
        EnemyAIState next = currentState.Update(this);
        if (next != null)
        {
            currentState.Exit(this);
            currentState = next;
            currentState.Enter(this);
        }
    }

    // ── 공통 유틸 ─────────────────────────────
    public float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    public void FacePlayerHorizontally()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
        }
    }
}
