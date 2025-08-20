using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMeshAgent로 플레이어를 추적하는 '필수 최소' AI.
/// - 목적지 갱신은 코루틴으로 0.2초 간격(프레임마다 재계산 방지)
/// - StoppingDistance 안으로 들어오면 정지하고 플레이어를 부드럽게 바라봄
/// - 시작 시 NavMesh 위가 아니면 SamplePosition + Warp로 안전 보정
/// - (옵션) 시야선 체크를 위한 레이어마스크 필드 제공
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChaseAgent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;     // Inspector에서 지정 or 태그로 자동 탐색
    [SerializeField] private string playerTag = "Player";

    [Header("Chase Tuning")]
    [SerializeField] private float repathInterval = 0.2f;  // 목적지 재계산 주기(초)
    [SerializeField] private float faceTurnSpeed = 10f;    // 플레이어 바라보기 속도(회전 보간 계수)

    [Header("Optional LOS")]
    [SerializeField] private bool useLineOfSight = false;  // 시야선 체크 활성화 여부
    [SerializeField] private LayerMask losBlockMask;       // 벽/장애물 레이어 선택
    [SerializeField] private float losMaxDistance = 50f;   // 시야선 최대 거리
    private Vector3 lastSeenPos;                           // 마지막으로 본 플레이어 위치(옵션)

    private NavMeshAgent agent;
    private Coroutine chaseRoutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // 플레이어 자동 탐색(Inspector 미지정 시)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null)
            {
                player = p.transform;
            }
        }

        // 시작 위치가 NavMesh 위인지 확인하고 아니면 안전하게 워프
        if (agent.isOnNavMesh == false)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position); // NavMesh 위로 순간이동
            }
            else
            {
                // 근처에 NavMesh가 없다면, 레벨 구성/베이크 설정을 먼저 확인해야 함.
                enabled = false; // 안전 차단
                return;
            }
        }

        // 추적 루틴 시작
        chaseRoutine = StartCoroutine(ChaseLoop());
    }

    private IEnumerator ChaseLoop()
    {
        var wait = new WaitForSeconds(repathInterval);

        while (true)
        {
            if (player == null)
            {
                // 플레이어가 삭제되거나 씬에 없을 경우, 잠시 후 재시도
                yield return wait;
                continue;
            }

            Vector3 targetPos = player.position;

            // (옵션) 시야선 체크: 막혔으면 마지막 본 위치로 향함
            if (useLineOfSight == true)
            {
                if (HasLineOfSight(targetPos))
                {
                    lastSeenPos = targetPos;
                }
                else
                {
                    targetPos = lastSeenPos; // 마지막 본 위치를 향해 이동
                }
            }

            // NavMesh 위로 스냅하여 목적지 설정(안전성↑)
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 2f, NavMesh.AllAreas) == true)
            {
                agent.isStopped = false; // 이동 활성화
                agent.SetDestination(hit.position);
            }

            // 도착 판정: 경로 계산이 끝났고(=pathPending=false), 남은 거리가 StoppingDistance 이하면 멈춤
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;

                // 플레이어를 천천히 바라보기(수직 회전 제외)
                Vector3 flatDir = (player.position - transform.position);
                flatDir.y = 0f;
                if (flatDir.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * faceTurnSpeed);
                }
            }

            yield return wait;
        }
    }

    /// <summary>
    /// 단순 시야선 체크(옵션): 플레이어 방향으로 Raycast, 벽/지형(지정 레이어)이 막으면 false
    /// </summary>
    private bool HasLineOfSight(Vector3 targetPos)
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f; // 눈높이 보정
        Vector3 dir = (targetPos - origin);
        float dist = Mathf.Min(dir.magnitude, losMaxDistance);
        if (dist <= 0.01f)
        {
            return true;
        }

        dir /= dist; // 정규화
        // 벽 레이어에 막히면 LOS 실패
        return !Physics.Raycast(origin, dir, dist, losBlockMask, QueryTriggerInteraction.Ignore);
    }

    void OnDisable()
    {
        if (chaseRoutine != null)
        {
            StopCoroutine(chaseRoutine);
        }

        agent.isStopped = true;
    }

    // 디버그 가시화(에디터 뷰)
    void OnDrawGizmosSelected()
    {
        if (useLineOfSight == true && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, player.position + Vector3.up * 1.0f);
        }
    }
}
