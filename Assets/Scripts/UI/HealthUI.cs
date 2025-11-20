using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthComponent targetHealth;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private Image healthFill;

    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private bool hideOnDeath = true;

    [SerializeField] private Image[] extraFlashImages; 
    [SerializeField]private Image HealthVignette;

    public GameObject deathUI;
    public GameObject ammoUI; 

    private Color originalFillColor;
    private Color[] originalExtraColors;
    private Coroutine flashRoutine;

    private void Start()
    {   

        originalFillColor = healthFill.color;

        // Store original colours images
        if (extraFlashImages != null)
        {
            originalExtraColors = new Color[extraFlashImages.Length];
            for (int i = 0; i < extraFlashImages.Length; i++)
            originalExtraColors[i] = extraFlashImages[i].color;
        }

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
        //Update Slider, 
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        healthText.text = $"{currentHealth} / {maxHealth}";

        // Update vignette
        if (HealthVignette != null)
        {
            float hp = currentHealth / maxHealth;
            HealthVignette.color = new Color(1f, 0f, 0f, 1f - hp);
        }

        // FLASH UI ELEMENTS
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashDamageUI());
    }

    private IEnumerator FlashDamageUI()
    {
        // Turn all UI elements RED
        healthFill.color = Color.red;

        if (extraFlashImages != null)
        {
            foreach (var img in extraFlashImages)   
                img.color = Color.red;
        }

        yield return new WaitForSeconds(0.2f);

        // Restore original colors
        healthFill.color = originalFillColor;

        if (extraFlashImages != null)
        {
            for (int i = 0; i < extraFlashImages.Length; i++)
                extraFlashImages[i].color = originalExtraColors[i];
        }
    }

    private void HandleDeath()
    {
        if(hideOnDeath)
        {
            healthSlider.gameObject.SetActive(false);
            healthText.gameObject.SetActive(false);
            deathUI.SetActive(true);

            //ammo UI hide
            if (ammoUI != null)
            {
                ammoUI.SetActive(false);
            }

            //Enable Mouse
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
    }

    


}
