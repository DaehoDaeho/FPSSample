using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f; // 자동 제거 시간
    public int damage = 5;

    Vector3 startPos;
    public float maxDist = 30.0f;

    void Start()
    {
        //Destroy(gameObject, lifeTime); // 일정 시간 후 제거
    }

    private void Update()
    {
        float dist = Vector3.Distance(transform.position, startPos);
        if(dist >= maxDist)
        {
            gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if(enemy != null)
            {
                //enemy.Dead();
                //enemy.ProcessDead();
                enemy.TakeDamage(damage);
            }

            Debug.Log("명중: " + collision.gameObject.name);
            //Destroy(collision.gameObject);
            //Destroy(gameObject); // 충돌 시 제거
            gameObject.SetActive(false);
        }
    }

    public void SetStartPos(Vector3 pos)
    {
        startPos = pos;
    }
}
