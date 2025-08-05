using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public GameObject bulletPrefab;
    public int poolSize = 20;
    List<GameObject> pool = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // �ʿ��� ������ �ʱ�ȭ.
        for(int i=0; i<poolSize; ++i)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform);
            bullet.SetActive(false);
            pool.Add(bullet);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ����� �Ѿ� Ȱ��ȭ.
    // �ʿ��� ��� �������� �ε�.
    public GameObject GetBullet(Vector3 pos, Quaternion rot)
    {
        foreach(GameObject bullet in pool)
        {
            if(bullet.activeInHierarchy == false)
            {
                bullet.transform.position = pos;
                bullet.transform.rotation = rot;
                bullet.SetActive(true);
                return bullet;
            }
        }

        // ��� ������ �Ѿ��� ������ ���� ����
        GameObject newBullet = Instantiate(bulletPrefab, pos, rot, transform);
        pool.Add(newBullet);
        return newBullet;
    }
}
