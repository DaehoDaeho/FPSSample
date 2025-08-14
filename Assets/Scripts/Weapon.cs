using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int magCapacity = 30;
    public int reserveAmmo = 90;
    public int CurrentAmmo;

    public event Action<int, int> OnAmmoChanged;

    // Start is called before the first frame update
    void Start()
    {
        CurrentAmmo = magCapacity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Shoot()
    {
        if(CurrentAmmo <= 0)
        {
            return false;
        }

        CurrentAmmo--;

        // 실제 발사 처리.

        RaiseAmmoChanged();
        return true;
    }

    public void Reload()
    {
        int needed = magCapacity - CurrentAmmo;

        if (needed <= 0 || reserveAmmo <= 0)
        {
            return;
        }

        int toLoad = Mathf.Min(needed, reserveAmmo);
        CurrentAmmo += toLoad;
        reserveAmmo -= toLoad;
        RaiseAmmoChanged();
    }

    private void RaiseAmmoChanged()
    {
        if(OnAmmoChanged != null)
        {
            OnAmmoChanged.Invoke(CurrentAmmo, reserveAmmo);
        }
    }
}
