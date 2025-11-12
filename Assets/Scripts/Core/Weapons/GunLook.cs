using UnityEngine;

public class GunLook : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 offsetFromCamera = new Vector3(0.3f, -0.2f, 0.5f);
    [SerializeField] private Vector3 rotationOffsetEuler = new Vector3(0, 90f, 0);
    [SerializeField] private float smoothSpeed = 15f;

    private void LateUpdate()
    {
        // Compute where gun should be
        Quaternion cameraRot = cameraTransform.rotation;
        Quaternion rotationOffset = Quaternion.Euler(rotationOffsetEuler);

        Quaternion targetRotation = cameraRot * rotationOffset;
        Vector3 targetPosition = cameraTransform.position + cameraTransform.TransformVector(offsetFromCamera);

        // Smoothly move and rotate gun
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}
