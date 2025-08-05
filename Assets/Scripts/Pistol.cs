using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Gun
{
    // Start is called before the first frame update
    protected override void Start()
    {
        maxAmmo = 10;
        fireRate = 0.4f;
        damage = 15;

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
