using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person player movement controller for OpenDungeonRe
/// Handles WASD/Arrow key movement with CharacterController
/// </summary>
[RequireComponent(typeof(UnityEngine.CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    private UnityEngine.CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private float currentHeight;
    private float targetHeight;

    // Input actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private void Awake()
    {
        controller = GetComponent<UnityEngine.CharacterController>();

        // Get input actions
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            sprintAction = playerInput.actions["Sprint"];
            crouchAction = playerInput.actions["Crouch"];
        }

        // Setup initial height
        currentHeight = standingHeight;
        targetHeight = standingHeight;
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
    }

    private void HandleMovement()
    {
        if (moveAction != null)
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();

            // Calculate movement direction relative to player's rotation
            Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection.y = 0f;

            // Determine speed based on sprint/crouch state
            float currentSpeed = walkSpeed;

            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
            }
            else if (sprintAction != null && sprintAction.IsPressed())
            {
                currentSpeed = runSpeed;
            }

            // Move the character
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
        // Apply gravity only if not grounded
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            // Reset velocity when landing
            velocity.y = -2f;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // Draw ground check sphere in editor
    private void OnDrawGizmosSelected()
    {
        if (controller != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                transform.position + Vector3.down * (controller.height / 2 - controller.radius),
                groundCheckDistance
            );
        }
    }
}
