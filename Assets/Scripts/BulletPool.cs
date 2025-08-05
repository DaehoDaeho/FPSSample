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
        // 필요한 데이터 초기화.
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

    // 사용할 총알 활성화.
    // 필요한 경우 프리팹을 로드.
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

        // 사용 가능한 총알이 없으면 새로 생성
        GameObject newBullet = Instantiate(bulletPrefab, pos, rot, transform);
        pool.Add(newBullet);
        return newBullet;
    }
}
