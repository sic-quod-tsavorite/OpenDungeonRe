using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages player input for OpenDungeonRe
/// Handles input action events and delegates to appropriate systems
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }

    private PlayerInput playerInput;
    private InputActionAsset inputActions;

    // Action references
    public InputAction MoveAction { get; private set; }
    public InputAction LookAction { get; private set; }
    public InputAction AttackAction { get; private set; }
    public InputAction InteractAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction CrouchAction { get; private set; }
    public InputAction SprintAction { get; private set; }
    public InputAction PreviousAction { get; private set; }
    public InputAction NextAction { get; private set; }

    // Event delegates
    public delegate void AttackEvent();
    public event AttackEvent OnAttack;

    public delegate void InteractEvent();
    public event InteractEvent OnInteract;

    public delegate void JumpEvent();
    public event JumpEvent OnJump;

    public delegate void CrouchEvent();
    public event CrouchEvent OnCrouch;

    public delegate void SprintEvent(bool isSprinting);
    public event SprintEvent OnSprint;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null)
        {
            inputActions = playerInput.actions;
            CacheInputActions();
            SetupInputCallbacks();
        }
    }

    private void OnDestroy()
    {
        // Clean up events
        OnAttack = null;
        OnInteract = null;
        OnJump = null;
        OnCrouch = null;
        OnSprint = null;
        
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void CacheInputActions()
    {
        // Cache all input actions for quick access
        MoveAction = playerInput.actions["Move"];
        LookAction = playerInput.actions["Look"];
        AttackAction = playerInput.actions["Attack"];
        InteractAction = playerInput.actions["Interact"];
        JumpAction = playerInput.actions["Jump"];
        CrouchAction = playerInput.actions["Crouch"];
        SprintAction = playerInput.actions["Sprint"];
        PreviousAction = playerInput.actions["Previous"];
        NextAction = playerInput.actions["Next"];
    }

    private void SetupInputCallbacks()
    {
        // Setup callbacks for button presses
        if (AttackAction != null)
        {
            AttackAction.performed += ctx => OnAttack?.Invoke();
        }

        if (InteractAction != null)
        {
            InteractAction.performed += ctx => OnInteract?.Invoke();
        }

        if (JumpAction != null)
        {
            JumpAction.performed += ctx => OnJump?.Invoke();
        }

        if (CrouchAction != null)
        {
            CrouchAction.performed += ctx => OnCrouch?.Invoke();
        }

        if (SprintAction != null)
        {
            SprintAction.performed += ctx => OnSprint?.Invoke(true);
            SprintAction.canceled += ctx => OnSprint?.Invoke(false);
        }
    }

    /// <summary>
    /// Gets the current move input vector
    /// </summary>
    public Vector2 GetMoveInput()
    {
        if (MoveAction != null)
        {
            return MoveAction.ReadValue<Vector2>();
        }
        return Vector2.zero;
    }

    /// <summary>
    /// Gets the current look input vector
    /// </summary>
    public Vector2 GetLookInput()
    {
        if (LookAction != null)
        {
            return LookAction.ReadValue<Vector2>();
        }
        return Vector2.zero;
    }

    /// <summary>
    /// Checks if any movement input is active
    /// </summary>
    public bool IsMoving()
    {
        return GetMoveInput().sqrMagnitude > 0.01f;
    }

    /// <summary>
    /// Checks if the player is sprinting
    /// </summary>
    public bool IsSprinting()
    {
        if (SprintAction != null)
        {
            return SprintAction.IsPressed();
        }
        return false;
    }

    /// <summary>
    /// Enables player input
    /// </summary>
    public void EnableInput()
    {
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }
    }

    /// <summary>
    /// Disables player input
    /// </summary>
    public void DisableInput()
    {
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }
    }

    /// <summary>
    /// Switches to a different action map
    /// </summary>
    public void SwitchActionMap(string actionMapName)
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(actionMapName);
        }
    }

    /// <summary>
    /// Gets the current action map name
    /// </summary>
    public string GetCurrentActionMapName()
    {
        if (playerInput != null)
        {
            return playerInput.currentActionMap.name;
        }
        return string.Empty;
    }
}
