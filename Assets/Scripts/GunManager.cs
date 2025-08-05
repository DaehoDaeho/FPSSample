using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GunType
{
    Gun,
    Pistol,
    Rifle
}

public class GunManager : MonoBehaviour
{
    public Gun[] gun;

    GunType currentGun = GunType.Gun;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1) == true)
        {
            currentGun = GunType.Gun;
            gun[0].gameObject.SetActive(true);
            gun[1].gameObject.SetActive(false);
            gun[2].gameObject.SetActive(false);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2) == true)
        {
            currentGun = GunType.Pistol;
            gun[0].gameObject.SetActive(false);
            gun[1].gameObject.SetActive(true);
            gun[2].gameObject.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) == true)
        {
            currentGun = GunType.Rifle;
            gun[0].gameObject.SetActive(false);
            gun[1].gameObject.SetActive(false);
            gun[2].gameObject.SetActive(true);
        }
    }
}
