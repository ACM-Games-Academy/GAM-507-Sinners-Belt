using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthComponent targetHealth;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private bool hideOnDeath = true;

    public GameObject ammoUI; 


    private void Start()
    {   

        //Try get TextMeshProUGUI if not assigned
        if(healthText == null)
        {
            healthText = GetComponentInChildren<TextMeshProUGUI>();
        }

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

        if (healthText == null)
        {
            Debug.LogError("[HealthUI] Health Text reference is missing.", this);
            enabled = false;
            return;
        }

        //Initialize slider values
        healthSlider.maxValue = targetHealth.MaxHealth;
        healthSlider.value = targetHealth.CurrentHealth;

        targetHealth.OnHealthChanged += UpdateHealthBar;
        targetHealth.OnDeath += HandleDeath;

        //Initial update
        UpdateHealthBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);

        //Update health text
        healthText.text = $"{targetHealth.CurrentHealth} / {targetHealth.MaxHealth}"; //Display in number format?



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

        // Debug.Log($"[HealthUI] Health updated: {currentHealth}/{maxHealth}", this);
        // Debug.Log($"[HealthUI] Slider value: {healthSlider.value}/{healthSlider.maxValue}", this);

        //Update health text
        healthText.text = $"{currentHealth} / {maxHealth}";
    }

    private void HandleDeath()
    {
        if(hideOnDeath)
        {
            healthSlider.gameObject.SetActive(false);
            healthText.gameObject.SetActive(false);

            //ammo UI hide
            if (ammoUI != null)
            {
                ammoUI.SetActive(false);
            }

        }
    }


}
