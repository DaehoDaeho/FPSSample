// EnemyStatus.cs
using UnityEngine;
using UnityEngine.AI;

public class EnemyStatus : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float destroyDelay = 2f;

    public MonoBehaviour aiScript; // EnemyController_OO 등
    public NavMeshAgent agent;

    void Start()
    {
        currentHP = maxHP;
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (aiScript != null)
        {
            aiScript.enabled = false;
        }
        if (agent != null)
        {
            agent.enabled = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 간단히 파괴(연출은 추후 추가)
        Destroy(gameObject, destroyDelay);
    }
}
