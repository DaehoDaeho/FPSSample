// Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Move")]
    public float speed = 20f;        // ���� �ӵ� (m/s)
    public float lifetime = 3f;      // ���� (��)

    [Header("Damage")]
    public float damage = 10f;       // �÷��̾�� �� ������

    [Header("Layer Tags")]
    public string playerTag = "Player";

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // ������ ���� �̵� ����
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        // ���� ������ �ڵ� ����
        Destroy(gameObject, lifetime);
    }

    // Trigger �浹 ó��(Projectile�� Collider�� IsTrigger=true)
    void OnTriggerEnter(Collider other)
    {
        // �÷��̾ ����ٸ� ������ �ֱ�
        if (other.CompareTag(playerTag))
        {
            PlayerStatus ps = other.GetComponent<PlayerStatus>();
            if (ps != null)
            {
                ps.TakeDamage(damage);
            }
            Destroy(gameObject); // �������� ź ����
            return;
        }

        // �� ��(��/�ٴ� ��)�� �ε����� ź ����
        // (�ʿ��ϸ� LayerMask�� "ȯ��"�� ��� ó���ص� �����ϴ�)
        Destroy(gameObject);
    }
}
