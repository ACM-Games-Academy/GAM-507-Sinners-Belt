using System;

public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    event Action<float, float> OnHealthChanged; // (current, max)
    event Action OnDeath;
    void TakeDamage(float amount);
    void Heal(float amount);
    bool IsAlive();
    void RestoreFullHealth();
    void SetMaxHealth(float newMax, bool adjustCurrent = true);
}