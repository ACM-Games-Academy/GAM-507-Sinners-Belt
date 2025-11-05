using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    [Header("References")]
    public InputReader input;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 12f;

    [Header("Jump Settings")]
    public float minJumpForce = 3f;
    public float maxJumpForce = 9f;
    public float maxChargeTime = 3f;      // how long you can hold jump to reach max height
    public float jumpCooldown = 0.3f;     // delay after landing before next jump
    public float gravity = -9f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float jumpChargeTimer;
    private float lastLandTime;
    private bool isCharging;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        HandleJumpCharge();
        HandleGravityAndJump();
    }

        private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        Vector2 moveInput = input.Move;
        Transform cam = Camera.main.transform;

        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 move = (camRight * moveInput.x + camForward * moveInput.y).normalized;

        float targetSpeed = input.IsSprinting ? sprintSpeed : walkSpeed;
        controller.Move(move * targetSpeed * Time.deltaTime);

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void HandleJumpCharge()
    {
        // time how long jump button is held down
        if (isGrounded && (Time.time - lastLandTime) > jumpCooldown)
        {
            if (input.JumpHeld)
            {
                // start charging while holding jump
                isCharging = true;
                jumpChargeTimer += Time.deltaTime;
                jumpChargeTimer = Mathf.Clamp(jumpChargeTimer, 0, maxChargeTime);
            }
            else if (isCharging && !input.JumpHeld)
            {
                // jumps higher based on how long jump was held
                float chargePercent = jumpChargeTimer / maxChargeTime;
                float jumpPower = Mathf.Lerp(minJumpForce, maxJumpForce, chargePercent);

                velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);

                // reset charge state
                isCharging = false;
                jumpChargeTimer = 0f;
                lastLandTime = Time.time;
            }
        }
        else
        {
            // if we're not grounded, stop charging
            isCharging = false;
        }
    }

    private void HandleGravityAndJump()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // stick to ground
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Crouch()
    {
        
    }
}

