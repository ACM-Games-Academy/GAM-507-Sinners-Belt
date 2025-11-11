using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("Player Settings")]
    [Range(1f, 20f)] public float mouseSensitivity = 5f;


    // Sensitivity multiplier due to cinemachine using high values for sensitivity
    private const float sensMultiplier = 40f;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SetMouseSensitivity(float newSens)
    {
        mouseSensitivity = newSens;

       
        OnSensitivityChanged?.Invoke(mouseSensitivity * sensMultiplier);
    }

    
    public event System.Action<float> OnSensitivityChanged;
}
