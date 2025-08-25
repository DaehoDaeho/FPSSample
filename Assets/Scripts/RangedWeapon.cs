// RangedWeapon.cs
using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [Header("Ranged")]
    public Transform firePoint;           // �ѱ�
    public GameObject projectilePrefab;   // Projectile_Player
    public float projectileSpeed = 28f;
    public float projectileDamage = 12f;

    void Start()
    {
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

        if (firePoint == null || projectilePrefab == null)
        {
            return;
        }

        GameObject proj = GameObject.Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // �ʿ� �� ���̾� ����
        int projLayer = LayerMask.NameToLayer("Projectile_Player");
        if (projLayer != -1)
        {
            proj.layer = projLayer;
        }

        Projectile_Player p = proj.GetComponent<Projectile_Player>();
        if (p != null)
        {
            p.speed = projectileSpeed;
            p.damage = projectileDamage;
        }

        nextFireTime = Time.time + cooldown;
    }
}
