using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class HealthComponent : MonoBehaviour, IEnemy
{
    [Header("Health")]
    public float maxHealth = 100f;

    public float Health { get; private set; }
    public bool IsAlive => Health > 0f;

    public event Action<float> OnDamaged;   // amount
    public event Action OnKilled;

    private void Awake()
    {
        Health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        Health -= Mathf.Max(0f, amount);
        OnDamaged?.Invoke(amount);

        if (Health <= 0f)
        {
            Health = 0f;
            OnKilled?.Invoke();
        }
    }

    // explicit interface implementation to match IEnemy
    void IEnemy.TakeDamage(float amount) => TakeDamage(amount);
    float IEnemy.Health => Health;
    bool IEnemy.IsAlive => IsAlive;

    public void Kill()
    {
        if (!IsAlive) return;
        Health = 0f;
        OnKilled?.Invoke();
    }

    private void OnValidate()
    {
        if (maxHealth < 1f) maxHealth = 1f;
    }
}
