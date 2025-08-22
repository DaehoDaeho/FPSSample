// EnemyRangedShooter.cs (SAFE)
// 비활성화되면(agent/회전) 절대 건드리지 않고, 코루틴만 정리.
// 발사/회전은 활성 상태에서만 수행됨.

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
        // 중요: 비활성화 시에는 회전/발사/agent 상태를 건드리지 않는다.
        StopAllCoroutines();
    }

    void Update()
    {
        if (player == null || firePoint == null || projectilePrefab == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > fireRange) return;

        // 수평 조준
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
        }

        // 쿨타임 발사
        if (Time.time >= nextFireTime)
        {
            FireOnce();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void FireOnce()
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Projectile에 값 전달(단순 공개 필드 가정)
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
