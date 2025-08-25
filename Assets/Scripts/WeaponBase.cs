// WeaponBase.cs
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Common")]
    public string weaponName = "Weapon";
    public GameObject viewModel;   // 손에 든 모델(무기 오브젝트). 교체 시 SetActive로 on/off
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
        // 필요 시 자식에서 프레임별 처리
    }

    public abstract void Fire();
}
