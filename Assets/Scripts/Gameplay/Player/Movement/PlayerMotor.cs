using UnityEngine;
using Unity.Cinemachine;
public class PlayerMotor : MonoBehaviour
{
    [Header("References")]
    public InputReader input;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 12f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f; 

    [Header("Jump Settings")]
    public float minJumpForce = 3f;
    public float maxJumpForce = 9f;
    public float maxChargeTime = 3f;      // how long you can hold jump to reach max height
    public float jumpCooldown = 0.3f;     // delay after landing before next jump
    public float gravity = -9f;

    [Header("Dash Settings")]
    public float dashDistance = 20f;
    public float dashCooldown = 1f;
    public float dashAmount = 3f;

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
        HandlePlayerRotation();
        HandleJumpCharge();
        HandleGravityAndJump();
        HandleDash();
    }


    private void HandlePlayerRotation()
    {
        // Make player rotation match the cameras forward direction (horizontal plane)
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        if (cameraForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }



    private void HandleMovement()
    {
        // Get the forward and right direction of the camera
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = Camera.main.transform.right;
        right.y = 0;
        right.Normalize();

        // Calculate the move direction based on player input
        Vector3 moveDirection = forward * input.Move.y + right * input.Move.x;

        // Determine the players speed (sprinting or walking)
        float currentSpeed = input.IsSprinting ? sprintSpeed : walkSpeed;

        // Move the character
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Check if the player is grounded
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            lastLandTime = Time.time;
        }
        // // Calculate the target rotation based on the move direction
        // Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        // // Smoothly rotate the player towards the target rotation
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // // If the move direction is not zero rotate the player
        // if (moveDirection != Vector3.zero)
        // {
            
        // }
    }

    private void HandleJumpCharge()
    {
        // Time how long the jump button is held down
        if (isGrounded && (Time.time - lastLandTime) > jumpCooldown)
        {
            if (input.JumpHeld)
            {
                // Start charging while holding jump
                isCharging = true;
                jumpChargeTimer += Time.deltaTime;
                jumpChargeTimer = Mathf.Clamp(jumpChargeTimer, 0, maxChargeTime);
            }
            else if (isCharging && !input.JumpHeld)
            {
                // Jumps higher based on how long the jump was held
                float chargePercent = jumpChargeTimer / maxChargeTime;
                float jumpPower = Mathf.Lerp(minJumpForce, maxJumpForce, chargePercent);

                velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);

                // Reset charge state
                isCharging = false;
                jumpChargeTimer = 0f;
                lastLandTime = Time.time;
            }
        }
        else
        {
            // If we are not grounded, stop charging
            isCharging = false;
        }
    }

    private void HandleGravityAndJump()
    {
        // Stick to the ground if grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // stick to ground 
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Dash Mechanics 
    private void HandleDash()
    {
        //Ignore Cooldown for now, basic dash implementation


        if (input.DashPressed)
        {
            Dash();
        }



        Debug.Log("Dash executed");
    }
    
    private void Dash()
    {
        // Dash in the direction the player is moving
        Vector3 dashDirection = Vector3.zero;

        // Get the forward and right direction of the camera
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = Camera.main.transform.right;
        right.y = 0;
        right.Normalize();

        dashDirection = forward * input.Move.y + right * input.Move.x;

        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.forward; // Dash forward if no input
        }

        dashDirection.Normalize();

        controller.Move(dashDirection * dashDistance);
    }

    // Crouch Mechanics 
    private void Crouch()
    {
    }
}