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
    public string enemyTag = "Enemy"; // ����: �±� üũ�� ����� ���

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
        // �� �켱 ó��
        EnemyStatus es = other.GetComponent<EnemyStatus>();
        if (es != null)
        {
            es.TakeDamage(damage);
            Destroy(this.gameObject);
            return;
        }

        // �� ��(��/�ٴ� ��) ��Ƶ� ����
        Destroy(this.gameObject);
    }
}
