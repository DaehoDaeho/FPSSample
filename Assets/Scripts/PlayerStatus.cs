using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    [SerializeField] private float currentHP;

    public float CurrentHP => currentHP;
    public bool IsDead { get; private set; }

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

        if (currentHP <= 0f)
        {
            IsDead = true;
            OnDied?.Invoke();

            // ���� �ٽ�: Game Over ȣ��(�ߺ� ���� ����) ����
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
