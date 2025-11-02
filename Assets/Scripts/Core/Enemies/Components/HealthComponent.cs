using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class HealthComponent : MonoBehaviour, IHealth
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public bool IsAlive()
    {
        return currentHealth > 0f;
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(float newMax, bool adjustCurrent = true)
    {
        if (newMax <= 0f) return;

        if (adjustCurrent)
            currentHealth = currentHealth / maxHealth * newMax;

        maxHealth = newMax;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
