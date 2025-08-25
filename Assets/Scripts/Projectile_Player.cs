// Projectile_Player.cs
using UnityEngine;

public class Projectile_Player : MonoBehaviour
{
    [Header("Move")]
    public float speed = 28f;
    public float lifetime = 3f;

    [Header("Damage")]
    public float damage = 12f;

    [Header("Tags")]
    public string enemyTag = "Enemy"; // 선택: 태그 체크에 사용할 경우

    private Rigidbody rb;
    private float timer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = this.transform.forward * speed;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 적 우선 처리
        EnemyStatus es = other.GetComponent<EnemyStatus>();
        if (es != null)
        {
            es.TakeDamage(damage);
            Destroy(this.gameObject);
            return;
        }

        // 그 외(벽/바닥 등) 닿아도 제거
        Destroy(this.gameObject);
    }
}
