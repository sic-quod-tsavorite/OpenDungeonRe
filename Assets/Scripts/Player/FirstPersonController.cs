using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Complete first-person controller for OpenDungeonRe
/// Combines movement, camera look, and input handling
/// </summary>
[RequireComponent(typeof(UnityEngine.CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraPivot;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;
    [SerializeField] private bool invertYAxis = false;
    [SerializeField] private bool lockCursor = true;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    private UnityEngine.CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool isGrounded;
    private bool isCrouching;
    private float currentHeight;
    private float targetHeight;

    // Input actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private void Awake()
    {
        controller = GetComponent<UnityEngine.CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        
        // Cache camera references
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
        
        if (cameraPivot == null && playerCamera != null)
        {
            cameraPivot = playerCamera.transform.parent;
        }
        
        // Get input actions
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            lookAction = playerInput.actions["Look"];
            jumpAction = playerInput.actions["Jump"];
            sprintAction = playerInput.actions["Sprint"];
            crouchAction = playerInput.actions["Crouch"];
        }
        
        // Setup initial height
        currentHeight = standingHeight;
        targetHeight = standingHeight;
        
        // Lock cursor
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnEnable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(
            transform.position + Vector3.down * (controller.height / 2 - controller.radius),
            groundCheckDistance,
            groundMask
        );

        // Handle crouch
        HandleCrouch();

        // Handle look
        HandleLook();

        // Handle movement
        HandleMovement();

        // Handle jump
        HandleJump();

        // Apply gravity
        ApplyGravity();
    }

    private void HandleCrouch()
    {
        if (crouchAction != null && crouchAction.WasPressedThisFrame())
        {
            isCrouching = !isCrouching;
            targetHeight = isCrouching ? crouchHeight : standingHeight;
        }

        // Smooth height transition
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        controller.height = currentHeight;
        
        // Adjust center
        Vector3 center = Vector3.zero;
        center.y = (currentHeight - standingHeight) / 2f;
        controller.center = center;
        
        // Adjust camera height
        if (playerCamera != null)
        {
            Vector3 camLocalPos = playerCamera.transform.localPosition;
            camLocalPos.y = (currentHeight - standingHeight) / 2f;
            playerCamera.transform.localPosition = camLocalPos;
        }
    }

    private void HandleLook()
    {
        if (lookAction != null)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            
            // Vertical rotation (pitch)
            float y = lookInput.y * (invertYAxis ? 1f : -1f) * mouseSensitivity * Time.deltaTime;
            xRotation -= y;
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
            
            // Horizontal rotation (yaw)
            float x = lookInput.x * mouseSensitivity * Time.deltaTime;
            yRotation += x;
            
            // Apply rotation
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    private void HandleMovement()
    {
        if (moveAction != null)
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            
            // Calculate movement direction
            Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection.y = 0f;
            
            // Determine speed
            float currentSpeed = walkSpeed;
            
            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
            }
            else if (sprintAction != null && sprintAction.IsPressed())
            {
                currentSpeed = runSpeed;
            }
            
            // Move
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (jumpAction != null && jumpAction.WasPressedThisFrame() && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Gets the player's camera
    /// </summary>
    public Camera GetPlayerCamera()
    {
        return playerCamera;
    }

    /// <summary>
    /// Gets whether the player is currently crouching
    /// </summary>
    public bool IsCrouching()
    {
        return isCrouching;
    }

    /// <summary>
    /// Gets whether the player is currently sprinting
    /// </summary>
    public bool IsSprinting()
    {
        return sprintAction != null && sprintAction.IsPressed();
    }

    /// <summary>
    /// Gets whether the player is grounded
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// Sets cursor lock state
    /// </summary>
    public void SetCursorLock(bool shouldLock)
    {
        lockCursor = shouldLock;
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
    }

    /// <summary>
    /// Toggles cursor lock state
    /// </summary>
    public void ToggleCursorLock()
    {
        SetCursorLock(!lockCursor);
    }

    /// <summary>
    /// Gets the current movement speed
    /// </summary>
    public float GetCurrentSpeed()
    {
        if (isCrouching) return crouchSpeed;
        if (sprintAction != null && sprintAction.IsPressed()) return runSpeed;
        return walkSpeed;
    }
}
