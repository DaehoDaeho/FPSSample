using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Gun
{
    // Start is called before the first frame update
    protected override void Start()
    {
        damage = 20;

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
