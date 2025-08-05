using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Die
}

public class EnemyAI : MonoBehaviour
{
    public EnemyState state = EnemyState.Idle;  // ���� ����.
    public Transform player; // ���� ���.
    public float speed = 2.0f;  // �̵� �ӵ�.
    public float chaseRange = 5.0f; // ���� ���� �Ÿ�.
    public float attackRange = 1.5f;    // ���� �Ÿ�.
    public int attackDamage = 10;   // ���ݷ�.
    public float attackCooldown = 1.0f; // ���� ���� (��)
    public int hp = 10; // ���� ü��.
    private float lastAttackTime = 0.0f;    // ������ ���� �ð�.
    public NavMeshAgent agent;

    // Update is called once per frame
    void Update()
    {
        //Vector3.MoveTowards(transform.position, player.position, 1000.0f);
        // �� ��ü ������ �Ÿ��� ����.
        float dist = Vector3.Distance(transform.position, player.position);
        //agent.SetDestination(player.position);

        switch (state)
        {
            case EnemyState.Idle:
                {
                    if(dist < chaseRange)
                    {
                        state = EnemyState.Chase;
                    }
                }
                break;

            case EnemyState.Chase:
                {
                    if(dist > chaseRange)
                    {
                        state = EnemyState.Idle;
                    }
                    else if(dist < attackRange)
                    {
                        state = EnemyState.Attack;
                    }
                    else
                    {
                        //Vector3 dir = (player.position - transform.position).normalized;
                        //transform.position += dir * speed * Time.deltaTime;
                        agent.SetDestination(player.position);
                    }
                }
                break;

            case EnemyState.Attack:
                {
                    if(dist > attackRange)
                    {
                        state = EnemyState.Chase;
                    }
                    else
                    {
                        if(Time.time - lastAttackTime >= attackCooldown)
                        {
                            PlayerHealth ph = player.GetComponent<PlayerHealth>();
                            if(ph != null)
                            {
                                ph.TakeDamage(attackDamage);
                                Debug.Log(("�÷��̾�� " + attackDamage + "�� �����!!"));
                            }
                        }
                        lastAttackTime = Time.time;
                    }
                }
                break;
        }   
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if(hp <= 0)
        {
            state = EnemyState.Die;
            Die();
        }
    }

    void Die()
    {
        Debug.Log("���� �׾����ϴ�.!!");
        Destroy(gameObject);
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }
}
