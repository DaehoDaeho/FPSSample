// EnemyRangedShooter.cs (SAFE)
// ��Ȱ��ȭ�Ǹ�(agent/ȸ��) ���� �ǵ帮�� �ʰ�, �ڷ�ƾ�� ����.
// �߻�/ȸ���� Ȱ�� ���¿����� �����.

using UnityEngine;

public class EnemyRangedShooter : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public string playerTag = "Player";
    public Transform firePoint;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float projectileDamage = 10f;

    [Header("Fire Settings")]
    public float fireRange = 15f;
    public float fireCooldown = 1.0f;
    private float nextFireTime = 0f;

    [Header("Aiming")]
    public float turnSpeed = 10f;

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
        // �߿�: ��Ȱ��ȭ �ÿ��� ȸ��/�߻�/agent ���¸� �ǵ帮�� �ʴ´�.
        StopAllCoroutines();
    }

    void Update()
    {
        if (player == null || firePoint == null || projectilePrefab == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > fireRange) return;

        // ���� ����
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
        }

        // ��Ÿ�� �߻�
        if (Time.time >= nextFireTime)
        {
            FireOnce();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void FireOnce()
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Projectile�� �� ����(�ܼ� ���� �ʵ� ����)
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.speed = projectileSpeed;
            p.damage = projectileDamage;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fireRange);
    }
}
