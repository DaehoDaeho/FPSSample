// MeleeWeapon.cs
using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [Header("Melee")]
    public Transform owner;       // 보통 플레이어의 몸통/카메라
    public float attackRange = 2.0f;
    public float hitRadius = 1.2f;
    public float damage = 25f;
    public LayerMask enemyMask;   // Enemy 레이어만

    void Start()
    {
        if (owner == null)
        {
            owner = this.transform; // 없으면 자기 자신 기준
        }
        // 시작 시 비활성화(장착될 때만 보이게)
        if (viewModel != null)
        {
            viewModel.SetActive(false);
        }
    }

    public override void Fire()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        // 내 앞쪽(절반 지점) 중심으로 작은 원
        Vector3 center = owner.position + owner.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, enemyMask, QueryTriggerInteraction.Ignore);

        int count = hits.Length;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                EnemyStatus es = hits[i].GetComponent<EnemyStatus>();
                if (es != null)
                {
                    es.TakeDamage(damage);
                    break; // 한 번만
                }
            }
        }

        nextFireTime = Time.time + cooldown;
    }

    void OnDrawGizmosSelected()
    {
        if (owner != null)
        {
            Gizmos.color = Color.red;
            Vector3 center = owner.position + owner.forward * (attackRange * 0.5f);
            Gizmos.DrawWireSphere(center, hitRadius);
        }
    }
}
