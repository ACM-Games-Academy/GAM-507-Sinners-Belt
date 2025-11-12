using UnityEngine;

public class GroundCheck : MonoBehaviour
{   
    [Header("Settings")]
    public float checkRadius = 0.3f;
    public LayerMask groundLayer;

    [HideInInspector] public bool isGrounded;

    private void Update()
    {
        // Check for ground using a small sphere at this object's position
        isGrounded = Physics.CheckSphere(transform.position, checkRadius, groundLayer);
        Debug.DrawRay(transform.position, Vector3.down * checkRadius, isGrounded ? Color.green : Color.red);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}

