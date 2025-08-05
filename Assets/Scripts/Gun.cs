using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("�ѱ� �⺻ ����")]
    public int maxAmmo = 30;    // �ִ� ź�� ����.
    public int currentAmmo; // ���� ���� ź�� ����.
    public float fireRate = 0.2f;   // ���� �ӵ�.
    public float range = 50.0f; // �Ѿ� �����Ÿ�.
    public int damage = 10;    // �� �� �� �����.
    public Camera fpsCamera;    // �߻� ī�޶�.
    public ParticleSystem muzzleFlash;  // �ѱ� ����Ʈ.
    public GameObject impactEffect; // ���� ����Ʈ.
    public TextMeshProUGUI ammoText;    // ź�� UI.
    private float nextTimeToFire = 0.0f;

    protected virtual void Start()
    {
        // ź�� �ʱ�ȭ.
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    private void Update()
    {
        // ���콺 ���� Ŭ���� �ư� �߻� �ð��� �ư� ź���� 0���� ũ��.
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
        // ź�� ����.
        --currentAmmo;
        UpdateAmmoUI();

        // �ѱ� ����Ʈ.
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Raycast�� �̿��� ���� ����.
        RaycastHit hit;
        if(Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            // ����� ó��.
            if(hit.transform.CompareTag("Enemy"))
            {
                Enemy target = hit.transform.GetComponent<Enemy>();
                if(target != null)
                {
                    target.TakeDamage(damage);
                }
            }

            // ���� ����Ʈ.
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
