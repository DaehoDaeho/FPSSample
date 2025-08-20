using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMeshAgent�� �÷��̾ �����ϴ� '�ʼ� �ּ�' AI.
/// - ������ ������ �ڷ�ƾ���� 0.2�� ����(�����Ӹ��� ���� ����)
/// - StoppingDistance ������ ������ �����ϰ� �÷��̾ �ε巴�� �ٶ�
/// - ���� �� NavMesh ���� �ƴϸ� SamplePosition + Warp�� ���� ����
/// - (�ɼ�) �þ߼� üũ�� ���� ���̾��ũ �ʵ� ����
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChaseAgent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;     // Inspector���� ���� or �±׷� �ڵ� Ž��
    [SerializeField] private string playerTag = "Player";

    [Header("Chase Tuning")]
    [SerializeField] private float repathInterval = 0.2f;  // ������ ���� �ֱ�(��)
    [SerializeField] private float faceTurnSpeed = 10f;    // �÷��̾� �ٶ󺸱� �ӵ�(ȸ�� ���� ���)

    [Header("Optional LOS")]
    [SerializeField] private bool useLineOfSight = false;  // �þ߼� üũ Ȱ��ȭ ����
    [SerializeField] private LayerMask losBlockMask;       // ��/��ֹ� ���̾� ����
    [SerializeField] private float losMaxDistance = 50f;   // �þ߼� �ִ� �Ÿ�
    private Vector3 lastSeenPos;                           // ���������� �� �÷��̾� ��ġ(�ɼ�)

    private NavMeshAgent agent;
    private Coroutine chaseRoutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // �÷��̾� �ڵ� Ž��(Inspector ������ ��)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null)
            {
                player = p.transform;
            }
        }

        // ���� ��ġ�� NavMesh ������ Ȯ���ϰ� �ƴϸ� �����ϰ� ����
        if (agent.isOnNavMesh == false)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position); // NavMesh ���� �����̵�
            }
            else
            {
                // ��ó�� NavMesh�� ���ٸ�, ���� ����/����ũ ������ ���� Ȯ���ؾ� ��.
                enabled = false; // ���� ����
                return;
            }
        }

        // ���� ��ƾ ����
        chaseRoutine = StartCoroutine(ChaseLoop());
    }

    private IEnumerator ChaseLoop()
    {
        var wait = new WaitForSeconds(repathInterval);

        while (true)
        {
            if (player == null)
            {
                // �÷��̾ �����ǰų� ���� ���� ���, ��� �� ��õ�
                yield return wait;
                continue;
            }

            Vector3 targetPos = player.position;

            // (�ɼ�) �þ߼� üũ: �������� ������ �� ��ġ�� ����
            if (useLineOfSight == true)
            {
                if (HasLineOfSight(targetPos))
                {
                    lastSeenPos = targetPos;
                }
                else
                {
                    targetPos = lastSeenPos; // ������ �� ��ġ�� ���� �̵�
                }
            }

            // NavMesh ���� �����Ͽ� ������ ����(��������)
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 2f, NavMesh.AllAreas) == true)
            {
                agent.isStopped = false; // �̵� Ȱ��ȭ
                agent.SetDestination(hit.position);
            }

            // ���� ����: ��� ����� ������(=pathPending=false), ���� �Ÿ��� StoppingDistance ���ϸ� ����
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;

                // �÷��̾ õõ�� �ٶ󺸱�(���� ȸ�� ����)
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
    /// �ܼ� �þ߼� üũ(�ɼ�): �÷��̾� �������� Raycast, ��/����(���� ���̾�)�� ������ false
    /// </summary>
    private bool HasLineOfSight(Vector3 targetPos)
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f; // ������ ����
        Vector3 dir = (targetPos - origin);
        float dist = Mathf.Min(dir.magnitude, losMaxDistance);
        if (dist <= 0.01f)
        {
            return true;
        }

        dir /= dist; // ����ȭ
        // �� ���̾ ������ LOS ����
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

    // ����� ����ȭ(������ ��)
    void OnDrawGizmosSelected()
    {
        if (useLineOfSight == true && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, player.position + Vector3.up * 1.0f);
        }
    }
}
