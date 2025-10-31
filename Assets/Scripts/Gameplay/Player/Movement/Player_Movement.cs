using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] public float currentSpeed;
    [SerializeField] private bool isSprinting;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float staminaDrainRate = 1f;
    [SerializeField] private float staminaRegenRate = 2f;
    [SerializeField] private float sprintStaminaThreshold = 0f;
    [SerializeField] private float doubleJumpCost = 1.5f;
    [SerializeField] private float staminaRegenDelay = 1f;

    [SerializeField] private float currentStamina;
    private float regenTimer = 0f;

    [Header("Double Jump Settings")]
    [SerializeField] private int maxJumps = 2;
    private int jumpCount = 0;

    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isJumpPressed;
    private bool sprintHeld;

    private Transform cam;
    public Animator animator;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
        cam = Camera.main.transform;
        currentStamina = maxStamina;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Sprint.performed += OnSprintStarted;
        inputActions.Player.Sprint.canceled += OnSprintCanceled;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Sprint.performed -= OnSprintStarted;
        inputActions.Player.Sprint.canceled -= OnSprintCanceled;
        inputActions.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        isJumpPressed = true;
    }

    private void OnSprintStarted(InputAction.CallbackContext ctx)
    {
        sprintHeld = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        sprintHeld = false;
    }

    private void Update()
    {
        HandleStamina();
        HandleMovement();

        currentSpeed = controller.velocity.magnitude;
        if (animator != null)
            animator.SetFloat("Speed", currentSpeed);
    }

    private void HandleMovement()
    {
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 motion = camRight * moveInput.x + camForward * moveInput.y;

        if (motion.magnitude > 1f)
            motion.Normalize();

        motion *= speed;

        if (controller.isGrounded)
        {
            velocity.y = -1f;
            jumpCount = 0;

            if (isJumpPressed)
            {
                velocity.y = jumpForce;
                jumpCount = 1;
                isJumpPressed = false;
            }
        }
        else
        {
            if (isJumpPressed && jumpCount < maxJumps && currentStamina >= doubleJumpCost)
            {
                velocity.y = jumpForce;
                currentStamina -= doubleJumpCost;
                jumpCount++;
                isJumpPressed = false;
            }

            velocity.y += gravity * Time.deltaTime;
        }

        motion.y = velocity.y;
        controller.Move(motion * Time.deltaTime);

        Vector3 lookDir = cam.forward;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private void HandleStamina()
    {
        bool isMoving = moveInput.magnitude > 0.1f;
        bool canSprint = sprintHeld && isMoving && currentStamina > sprintStaminaThreshold;

        if (canSprint)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
            regenTimer = 0f;
        }
        else
        {
            isSprinting = false;

            if (currentStamina < maxStamina)
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= staminaRegenDelay)
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    currentStamina = Mathf.Min(currentStamina, maxStamina);
                }
            }
        }

        if (currentStamina <= 0f)
        {
            isSprinting = false;
            sprintHeld = false;
        }
    }
}
