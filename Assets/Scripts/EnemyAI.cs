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
    public EnemyState state = EnemyState.Idle;  // 현재 상태.
    public Transform player; // 추적 대상.
    public float speed = 2.0f;  // 이동 속도.
    public float chaseRange = 5.0f; // 추적 시작 거리.
    public float attackRange = 1.5f;    // 공격 거리.
    public int attackDamage = 10;   // 공격력.
    public float attackCooldown = 1.0f; // 공격 간격 (초)
    public int hp = 10; // 적의 체력.
    private float lastAttackTime = 0.0f;    // 마지막 공격 시간.
    public NavMeshAgent agent;

    // Update is called once per frame
    void Update()
    {
        //Vector3.MoveTowards(transform.position, player.position, 1000.0f);
        // 구 객체 사이의 거리를 구함.
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
                                Debug.Log(("플레이어에게 " + attackDamage + "의 대미지!!"));
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
        Debug.Log("적이 죽었습니다.!!");
        Destroy(gameObject);
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }
}
