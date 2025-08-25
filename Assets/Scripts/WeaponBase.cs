// WeaponBase.cs
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Common")]
    public string weaponName = "Weapon";
    public GameObject viewModel;   // �տ� �� ��(���� ������Ʈ). ��ü �� SetActive�� on/off
    public float cooldown = 0.3f;

    protected float nextFireTime = 0f;

    public virtual void OnEquip()
    {
        if (viewModel != null)
        {
            viewModel.SetActive(true);
        }
    }

    public virtual void OnUnequip()
    {
        if (viewModel != null)
        {
            viewModel.SetActive(false);
        }
    }

    public virtual void TickUpdate()
    {
        // �ʿ� �� �ڽĿ��� �����Ӻ� ó��
    }

    public abstract void Fire();
}
