using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthComponent targetHealth;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private bool hideOnDeath = true;


    private void Start()
    {
        //Try to find HealthComponent if not assigned
        if (targetHealth == null)
        {
            targetHealth = GetComponentInParent<HealthComponent>();
        }

        if (targetHealth == null)
        {
            Debug.LogError("[HealthUI] No HealthComponent found on parent objects.", this);
            enabled = false;
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogError("[HealthUI] Health Slider reference is missing.", this);
            enabled = false;
            return;
        }

        //Initialize slider values
        healthSlider.maxValue = targetHealth.MaxHealth;
        healthSlider.value = targetHealth.CurrentHealth;

        targetHealth.OnHealthChanged += UpdateHealthBar;
        targetHealth.OnDeath += HandleDeath;

    }

    private void OnDestroy()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateHealthBar;
            targetHealth.OnDeath -= HandleDeath;
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        Debug.Log($"[HealthUI] Health updated: {currentHealth}/{maxHealth}", this);
        Debug.Log($"[HealthUI] Slider value: {healthSlider.value}/{healthSlider.maxValue}", this);
    }

    private void HandleDeath()
    {
        if(hideOnDeath)
        {
            healthSlider.gameObject.SetActive(false);
        }
    }


}
