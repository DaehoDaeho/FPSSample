// Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Move")]
    public float speed = 20f;        // 비행 속도 (m/s)
    public float lifetime = 3f;      // 수명 (초)

    [Header("Damage")]
    public float damage = 10f;       // 플레이어에게 줄 데미지

    [Header("Layer Tags")]
    public string playerTag = "Player";

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 앞으로 직선 이동 시작
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        // 수명 지나면 자동 삭제
        Destroy(gameObject, lifetime);
    }

    // Trigger 충돌 처리(Projectile의 Collider는 IsTrigger=true)
    void OnTriggerEnter(Collider other)
    {
        // 플레이어를 맞췄다면 데미지 주기
        if (other.CompareTag(playerTag))
        {
            PlayerStatus ps = other.GetComponent<PlayerStatus>();
            if (ps != null)
            {
                ps.TakeDamage(damage);
            }
            Destroy(gameObject); // 맞췄으면 탄 제거
            return;
        }

        // 그 외(벽/바닥 등)에 부딪혀도 탄 제거
        // (필요하면 LayerMask로 "환경"만 골라서 처리해도 좋습니다)
        Destroy(gameObject);
    }
}
