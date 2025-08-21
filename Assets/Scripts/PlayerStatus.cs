using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    [SerializeField] private float currentHP;

    public float CurrentHP => currentHP;
    public bool IsDead { get; private set; }

    // 이벤트 등록을 하기 위함.
    public event Action<float, float> OnHpChanged; // (curr, max)
    public event Action OnDied;

    void Start()
    {
        currentHP = maxHP;
        RaiseHpChanged();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHP = Mathf.Max(0f, currentHP - amount);
        RaiseHpChanged();
        Debug.Log("플레이어 피격!!!");

        if (currentHP <= 0f)
        {
            Debug.Log("플레이어 사망!!!");
            IsDead = true;
            OnDied?.Invoke();

            // ── 핵심: Game Over 호출(중복 방지 포함) ──
            GameManager.Instance?.GameOver();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        RaiseHpChanged();
    }

    private void RaiseHpChanged() => OnHpChanged?.Invoke(currentHP, maxHP);
}
