using UnityEngine;
using Unity.Cinemachine;

public class CameraSens : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController axisController;

    [Header("Sensitivity")]
    [Range(0.1f, 20f)]
    public float mouseSensitivity = 1f;

    private float lastAppliedSensitivity = -1f;

    private void Update()
    {
        float scaledSens = mouseSensitivity;
        if (Mathf.Abs(scaledSens - lastAppliedSensitivity) > 0.001f)
        {
            ApplySensitivity(scaledSens);
            lastAppliedSensitivity = scaledSens;
        }
    }

    private void ApplySensitivity(float newSens)
    {
        if (axisController == null) return;

        int count = axisController.Controllers.Count;

        for (int i = 0; i < count; i++)
        {
            var c = axisController.Controllers[i];

            bool isVertical = (i == 1);
            float gain = isVertical ? -Mathf.Abs(newSens) : Mathf.Abs(newSens);

            if (c.Input != null)
            {
                c.Input.Gain = gain;
                c.Input.LegacyGain = gain;
            }
        }

        Debug.Log($"[CameraSens] Applied sensitivity {newSens}");
    }
}