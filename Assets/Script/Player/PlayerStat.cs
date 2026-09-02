using System;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    private int maxHealth;
    private int attackDamage;

    public event Action<int, int> OnHealthChanged;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public int AttackDamage => attackDamage;
    public bool IsDead => CurrentHealth <= 0;

    public float IncomingDamageMultiplier { get; set; } = 1f;

    private void Start()
    {
        InitializeStats();

        GameManager.Instance.OnPlayStarted += InitializeStats;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayStarted -= InitializeStats;
    }

    private void InitializeStats()
    {
        maxHealth = PlayerData.Instance.MaxHealth;
        attackDamage = PlayerData.Instance.AttackDamage;

        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public int TakeDamage(int amount, bool fromBlock)
    {
        float multiplier = IncomingDamageMultiplier * PlayerData.RollDamageVariance();
        if (fromBlock)
        {
            multiplier *= PlayerData.Instance.BlockDamageMultiplier;
        }

        int appliedDamage = Mathf.RoundToInt(amount * multiplier);
        if (amount > 0 && appliedDamage < 1)
        {
            appliedDamage = 1;
        }

        CurrentHealth = Math.Max(0, CurrentHealth - appliedDamage);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        return appliedDamage;
    }
}
