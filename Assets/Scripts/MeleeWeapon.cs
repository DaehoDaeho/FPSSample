// MeleeWeapon.cs
using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [Header("Melee")]
    public Transform owner;       // ���� �÷��̾��� ����/ī�޶�
    public float attackRange = 2.0f;
    public float hitRadius = 1.2f;
    public float damage = 25f;
    public LayerMask enemyMask;   // Enemy ���̾

    void Start()
    {
        if (owner == null)
        {
            owner = this.transform; // ������ �ڱ� �ڽ� ����
        }
        // ���� �� ��Ȱ��ȭ(������ ���� ���̰�)
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

        // �� ����(���� ����) �߽����� ���� ��
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
                    break; // �� ����
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
