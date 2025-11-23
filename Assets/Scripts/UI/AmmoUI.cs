using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DualFireGunController weapon;
    [SerializeField] private Slider ammoSlider;
    [SerializeField] private TextMeshProUGUI ammoText;

    private void Start()
    {
        if (weapon == null)
        {
            weapon = Object.FindFirstObjectByType<DualFireGunController>();
            if (weapon == null)
            {
                Debug.LogError("[AmmoUI] No DualFireGunController found in scene!", this);
                enabled = false;
                return;
            }
        }

        if (ammoSlider == null)
        {
            Debug.LogError("[AmmoUI] Ammo Slider reference is missing.", this);
            enabled = false;
            return;
        }

        if (ammoText == null)
        {
            ammoText = GetComponentInChildren<TextMeshProUGUI>();
            if (ammoText == null)
            {
                Debug.LogError("[AmmoUI] Ammo Text reference is missing.", this);
                enabled = false;
                return;
            }
        }

        ammoSlider.maxValue = weapon.GetMaxAmmo();
        ammoSlider.value = weapon.GetCurrentAmmo();
        UpdateAmmoText();
    }

    private void Update()
    {
        ammoSlider.maxValue = weapon.GetMaxAmmo();
        ammoSlider.value = weapon.GetCurrentAmmo();
        UpdateAmmoText();
    }

    private void UpdateAmmoText()
    {
        ammoText.text = $"{weapon.GetCurrentAmmo()} / {weapon.GetMaxAmmo()}";
    }
}
