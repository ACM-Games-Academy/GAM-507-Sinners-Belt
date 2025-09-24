using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] private bool isSprinting;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float staminaDrainRate = 1f;
    [SerializeField] private float staminaRegenRate = 2f;

    [SerializeField] private float sprintStaminaThreshold = 0.4f;

    [SerializeField] private float currentStamina;

    [Header("Attack Settings")]



    [Header("UI Components")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Gradient staminaGradient;
    [SerializeField] private Image staminaFillImage;



    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isJumpPressed;
    private bool sprintHeld;

    private Transform cam;

    public Animator animator;
    private void Start()
    {
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
            if (staminaFillImage != null && staminaGradient != null)
                staminaFillImage.color = staminaGradient.Evaluate(currentStamina / maxStamina);
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        // Ensure the camera is set to the main camera
        cam = Camera.main.transform;

        // Fill stamina at start
        currentStamina = maxStamina;
    }

    private void OnEnable()
    {
        inputActions.Enable();

        // Movement input
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        // Jump input
        inputActions.Player.Jump.performed += _ => isJumpPressed = true;

        // Sprint input
        inputActions.Player.Sprint.performed += _ => sprintHeld = true;
        inputActions.Player.Sprint.canceled += _ => sprintHeld = false;
    }

    private void OnDisable() //Some cleanup to prevent memory leaks
    {
        inputActions.Player.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled -= _ => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed -= _ => isJumpPressed = true;
        inputActions.Player.Sprint.performed -= _ => sprintHeld = true;
        inputActions.Player.Sprint.canceled -= _ => sprintHeld = false;
        inputActions.Disable();
    }

    private void Update()
    {
        HandleStamina();
        HandleMovement();
    }

    private void HandleMovement()
    {
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        // Get camera directions, ignore vertical tilt
        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Movement relative to camera
        Vector3 motion = camRight * moveInput.x + camForward * moveInput.y;

        if (motion.magnitude > 1f)
            motion.Normalize();

        motion *= speed;

        // Gravity / Jump
        if (controller.isGrounded)
        {
            velocity.y = -1f;
            if (isJumpPressed)
            {
                velocity.y = jumpForce;
                isJumpPressed = false;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        motion.y = velocity.y;

        // Actually move
        controller.Move(motion * Time.deltaTime);

        // Rotate player root to face camera's horizontal direction
        Vector3 lookDir = cam.forward;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }



    private void HandleStamina()
    {
        bool canSprint = sprintHeld && moveInput.magnitude > 0.1f && currentStamina > sprintStaminaThreshold;

        if (canSprint)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
        }
        else
        {
            isSprinting = false;
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }

        // Update UI
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
            if (staminaFillImage != null && staminaGradient != null)
                staminaFillImage.color = staminaGradient.Evaluate(currentStamina / maxStamina);
        }
    }
}

