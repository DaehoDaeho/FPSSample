using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("총기 기본 세팅")]
    public int maxAmmo = 30;    // 최대 탄약 개수.
    public int currentAmmo; // 현재 남은 탄약 개수.
    public float fireRate = 0.2f;   // 연사 속도.
    public float range = 50.0f; // 총알 사정거리.
    public int damage = 10;    // 한 발 당 대미지.
    public Camera fpsCamera;    // 발사 카메라.
    public ParticleSystem muzzleFlash;  // 총구 이펙트.
    public GameObject impactEffect; // 명중 이펙트.
    public TextMeshProUGUI ammoText;    // 탄약 UI.
    private float nextTimeToFire = 0.0f;

    protected virtual void Start()
    {
        // 탄약 초기화.
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    private void Update()
    {
        // 마우스 왼쪽 클릭이 됐고 발사 시간이 됐고 탄약이 0보다 크면.
        if(Input.GetButton("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        if(Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    protected virtual IEnumerator Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Reload!!!!!");

        yield return new WaitForSeconds(3.0f);

        UpdateAmmoUI();
        Debug.Log("Update UI!!!!!");
    }

    public void Shoot()
    {
        // 탄약 감소.
        --currentAmmo;
        UpdateAmmoUI();

        // 총구 이펙트.
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Raycast를 이용한 명중 판정.
        RaycastHit hit;
        if(Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            // 대미지 처리.
            if(hit.transform.CompareTag("Enemy"))
            {
                Enemy target = hit.transform.GetComponent<Enemy>();
                if(target != null)
                {
                    target.TakeDamage(damage);
                }
            }

            // 명중 이펙트.
            GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 1.0f);
        }
    }

    public void UpdateAmmoUI()
    {
        if(ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }
}
