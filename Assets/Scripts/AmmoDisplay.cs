using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    public Gun gun;
    public TextMeshProUGUI ammoText;

    // Update is called once per frame
    void Update()
    {
        ammoText.text = gun.currentAmmo + " / " + gun.maxAmmo;
    }
}
