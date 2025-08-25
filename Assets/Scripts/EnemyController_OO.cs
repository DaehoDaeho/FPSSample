// EnemyController_OO.cs
// ���¸� Ŭ������ �и�(Idle/Chase/Attack), ���� ����� ���� Ŭ������ �и�(����/���Ÿ�).
// ������Ƽ/����/�̺�Ʈ ���� ���� �ʵ� + �޼���θ� ����.

using UnityEngine;
using UnityEngine.AI;

// ����������������������������������������������������������������������������������������������������������������������������
// 2) ���ؽ�Ʈ(���� ������ / �ൿ ��ƿ) - �� ��ü
// ����������������������������������������������������������������������������������������������������������������������������
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
    public float meleeExitBuffer = 0.3f; // ���� ���� ����
    public float rangedExitBuffer = 1.0f; // ���Ÿ� ���� ����(�ʿ��)

    public enum AttackAIMode { MeleeOnly, RangedOnly, Hybrid }
    public AttackAIMode attackMode = AttackAIMode.Hybrid;

    [Header("Melee")]
    public float meleeDamage = 15f;
    public float hitRadius = 1.2f;      // �� ���� ���� ��
    public LayerMask playerMask;          // Player ���̾�

    [Header("Melee Cooldown")]
    public float meleeCooldown = 1.2f;
    [HideInInspector] public float nextMeleeTime = 0f;

    [Header("Ranged")]
    public float projectileSpeed = 20f;
    public float projectileDamage = 10f;

    [Header("Ranged Cooldown")]
    public float rangedCooldown = 1.0f;
    [HideInInspector] public float nextRangedTime = 0f;

    // ���̺긮�忡�� ���� � ������ ���� ������ ���(���� ����)
    [HideInInspector] public bool usingMelee = false;

    // ���� ���� �ν��Ͻ�
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

        currentState = new IdleState();   // ���� ����
        currentState.Enter(this);
    }

    void OnDisable()
    {
        // ��Ȱ��ȭ �� �ڷ�ƾ�� ����(ȸ��/������Ʈ�� �ǵ帮�� ����)
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

    // ���� ���� ��ƿ ����������������������������������������������������������
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
