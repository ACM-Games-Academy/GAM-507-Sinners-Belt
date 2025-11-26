using UnityEngine;
using System;
using System.Collections;


[RequireComponent(typeof(Collider))]
public class HealthComponent : MonoBehaviour, IHealth
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private Renderer rend;
    // private Material mat;
    // private Color originalColor;

    

    private void Awake()
    {
        currentHealth = maxHealth;
        // rend = GetComponent<Renderer>();
        // if (rend != null)
        // {
        //     mat = rend.material; // this creates a unique material for this renderer
        //     originalColor = mat.color;
        // }

        if (CompareTag("Player"))
        {
            StartCoroutine(RegenerateHealth());
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        // StartCoroutine(DamageFlash());

        if (currentHealth <= 0f)
            OnDeath?.Invoke();
    }

    // private IEnumerator DamageFlash()
    // {
    //     if (mat == null) yield break;

    //     mat.color = Color.red;
    //     yield return new WaitForSeconds(0.1f);
    //     mat.color = originalColor;
    // }


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

    private IEnumerator RegenerateHealth()
    {
        while (IsAlive())
        {
            if (currentHealth < maxHealth)
            {
                float newHealth = Math.Min(currentHealth + (15f * Time.deltaTime), maxHealth);
                currentHealth = newHealth;
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }

            yield return null;
        }
    }
}
