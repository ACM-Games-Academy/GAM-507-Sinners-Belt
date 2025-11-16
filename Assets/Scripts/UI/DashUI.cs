using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour
{
    [Header("References")]
    public PlayerMotor player;          // Assign your PlayerMotor in Inspector
    public Image[] dashIcons;           // Assign the 3 dash icons in order

    [Header("Colors")]
    public Color availableColor = Color.white;
    public Color unavailableColor = new Color(1f, 1f, 1f, 0.25f);

    private void Update()
    {
        UpdateDashIcons();
    }

    private void UpdateDashIcons()
    {
        if (player == null || dashIcons == null) return;

        int available = player.AvailableDashes;  // We'll expose this next

        for (int i = 0; i < dashIcons.Length; i++)
        {
            if (dashIcons[i] == null) continue;

            dashIcons[i].color = i < available ? availableColor : unavailableColor;
        }
    }
}
