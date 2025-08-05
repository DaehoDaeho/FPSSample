using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f; // �ڵ� ���� �ð�
    public int damage = 5;

    Vector3 startPos;
    public float maxDist = 30.0f;

    void Start()
    {
        //Destroy(gameObject, lifeTime); // ���� �ð� �� ����
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

            Debug.Log("����: " + collision.gameObject.name);
            //Destroy(collision.gameObject);
            //Destroy(gameObject); // �浹 �� ����
            gameObject.SetActive(false);
        }
    }

    public void SetStartPos(Vector3 pos)
    {
        startPos = pos;
    }
}
