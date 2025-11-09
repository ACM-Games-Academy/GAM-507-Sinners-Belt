using UnityEngine;

public class BillboardHealthBar : MonoBehaviour
{
    private Camera mainCamera;  // Reference to the main camera

    void Start()
    {
        // Automatically find the main camera at the start of the game
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Please ensure there is a camera with the 'MainCamera' tag.");
        }
    }

    void Update()
    {
        if (mainCamera != null)
        {
            // Make sure the health bar canvas always faces the camera
            transform.LookAt(mainCamera.transform.position);
            //Lock the rotation on the x and z axes to keep it upright
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

            //Dont be an idiot (to myself Matt)
           
        }
    }
}
