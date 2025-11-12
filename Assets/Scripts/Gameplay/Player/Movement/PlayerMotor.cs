using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine.XR;
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
    public float dashCooldown = 6f;
    public float dashAmount = 3f;

    private float lastDashTime;
    private float[] dashRechargeTimers;
    private int availableDashes;

    private bool prevDashPressed = false;


    private CharacterController controller;
    private Vector3 velocity;

    private float jumpChargeTimer;
    private float lastLandTime;
    private bool isCharging;

    private bool isGrounded;

    public GroundCheck groundCheck;
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

    private void Start()
    {
        dashRechargeTimers = new float[(int)dashAmount];
        availableDashes = (int)dashAmount;
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


        if (isGrounded && velocity.y < 0)
        {
            lastLandTime = Time.time;
        }

    }

    private void HandleGravityAndJump()
    {

        // Only stick if grounded and not jumping up
        if (isGrounded && velocity.y < -0.1f && !input.JumpPressed)
            velocity.y = -2f;

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }


    private void HandleJumpCharge()
    {   
        // Use GroundCheck for grounded state
        isGrounded = groundCheck != null && groundCheck.isGrounded;

        Debug.Log($"IsGrounded: {isGrounded}, TimeSinceLastLand: {Time.time - lastLandTime}, JumpCooldown: {jumpCooldown}");
        if (isGrounded && (Time.time - lastLandTime) > jumpCooldown)
        {
            if (input.JumpHeld)
            {
                isCharging = true;
                jumpChargeTimer += Time.deltaTime;
                jumpChargeTimer = Mathf.Clamp(jumpChargeTimer, 0, maxChargeTime);
            }
            else if (isCharging && !input.JumpHeld)
            {
                float chargePercent = jumpChargeTimer / maxChargeTime;
                float jumpPower = Mathf.Lerp(minJumpForce, maxJumpForce, chargePercent);

                velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);

                isCharging = false;
                jumpChargeTimer = 0f;
                lastLandTime = Time.time;
            }
        }
        else
        {
            isCharging = false;
        }
    }

       

    // Dash Mechanics 
    private void HandleDash()
    {
        // update recharge timers
        for (int i = 0; i < dashRechargeTimers.Length; i++)
        {
            if (dashRechargeTimers[i] > 0f)
            {
                dashRechargeTimers[i] -= Time.deltaTime;
                if (dashRechargeTimers[i] <= 0f)
                {
                    dashRechargeTimers[i] = 0f;
                    availableDashes = Mathf.Min(availableDashes + 1, (int)dashAmount);
                    Debug.Log($"Dash recharged! Available Dashes: {availableDashes}");
                }
            }
        }

        // holding key shouldnt dash multiple times
        bool currentDashPressed = input.DashPressed;
        bool pressedThisFrame = currentDashPressed && !prevDashPressed;
        prevDashPressed = currentDashPressed;

        if (!pressedThisFrame) return;

        if (availableDashes <= 0) return;
        if (Time.time - lastDashTime < 0.1f) return; // small buffer to avoid 2 dashes in 1 frame

        // consume a dash
        Dash();
        availableDashes--;

        // start recharge timer for the first free slot
        for (int i = 0; i < dashRechargeTimers.Length; i++)
        {
            if (dashRechargeTimers[i] <= 0f)
            {
                dashRechargeTimers[i] = dashCooldown;
                break;
            }
        }

        lastDashTime = Time.time;
        Debug.Log($"Dash executed | Remaining Dashes: {availableDashes}");
    }
    private void Dash()
    {
        // Dash direction based on camera
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = Camera.main.transform.right;
        right.y = 0;
        right.Normalize();

        Vector3 dashDirection = forward * input.Move.y + right * input.Move.x;

        if (dashDirection == Vector3.zero)
            dashDirection = transform.forward;

        controller.Move(dashDirection.normalized * dashDistance);

        // small cooldown buffer between dashes (for spam control)
        lastDashTime = Time.time;
    }

}