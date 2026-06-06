using UnityEngine;

/// <summary>
/// Door controller that animates open/closed when the player presses E (Interact)
/// Attach this to the DoorControl GameObject (parent of DoorObject, HingePoint, and Handle)
/// </summary>
public class Door : Interactable
{
    [Header("Door References")]
    [Tooltip("The actual door mesh that will rotate")]
    [SerializeField] private Transform doorObject;
    
    [Tooltip("The hinge point transform - door will rotate around this")]
    [SerializeField] private Transform hingePoint;
    
    [Tooltip("The door handle transform (optional)")]
    [SerializeField] private Transform handle;

    [Header("Animation Settings")]
    [Tooltip("How many degrees the door opens")]
    [SerializeField] private float openAngle = 90f;
    
    [Tooltip("Speed of the door animation (higher = faster)")]
    [SerializeField] private float animationSpeed = 2f;
    
    [Tooltip("Which axis the door rotates around")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;

    [Header("State")]
    [SerializeField] private bool startOpen = false;
    
    [Header("Auto-Close (Optional)")]
    [Tooltip("If enabled, door will automatically close after this many seconds. Set to 0 to disable.")]
    [SerializeField] private float autoCloseDelay = 0f;
    
    private bool isOpen;
    private bool isAnimating = false;
    private float autoCloseTimer = 0f;
    private Quaternion initialLocalRotation;
    private Quaternion targetLocalRotation;
    
    /// <summary>
    /// Set up initial state
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        
        // Auto-assign references if not set
        if (doorObject == null)
        {
            doorObject = transform.Find("DoorObject");
        }
        
        if (hingePoint == null)
        {
            hingePoint = transform.Find("HingePoint");
        }
        
        if (handle == null)
        {
            handle = transform.Find("Handle");
        }
        
        // Store initial rotation
        if (doorObject != null)
        {
            initialLocalRotation = doorObject.localRotation;
        }
        else
        {
            Debug.LogError($"{gameObject.name}: doorObject is NULL! Please assign DoorObject transform.");
        }
        
        // Set initial state
        isOpen = startOpen;
        autoCloseTimer = 0f;
        
        // Set initial rotation based on start state
        if (doorObject != null)
        {
            UpdateTargetRotation();
            doorObject.localRotation = targetLocalRotation;
        }
        else
        {
            Debug.LogError($"{gameObject.name}: doorObject reference is NULL! The door will not work. Please assign the DoorObject transform.");
        }
    }
    
    /// <summary>
    /// Handle door animation each frame
    /// </summary>
    private void Update()
    {
        if (isAnimating)
        {
            AnimateDoor();
        }
        
        // Handle auto-close
        if (autoCloseDelay > 0 && isOpen && !isAnimating)
        {
            autoCloseTimer += Time.deltaTime;
            if (autoCloseTimer >= autoCloseDelay)
            {
                Close();
                autoCloseTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// Override to use doorObject's position for interaction checks
    /// </summary>
    protected override bool CanInteract()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return false;
        }
        
        // Use doorObject's position if available, otherwise use this transform
        Transform targetTransform = doorObject != null ? doorObject : transform;
        
        // Check distance to the actual door position
        float distance = Vector3.Distance(playerCamera.transform.position, targetTransform.position);
        if (distance > maxInteractionDistance) return false;
        
        // Check if player is looking at the door
        return IsPlayerLookingAtDoor();
    }
    
    /// <summary>
    /// Check if player is looking at the door using its actual position
    /// </summary>
    private bool IsPlayerLookingAtDoor()
    {
        if (playerCamera == null) return false;
        
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        // Raycast against all physics layers, including trigger colliders
        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance, ~0, QueryTriggerInteraction.Collide))
        {
            // Check if we hit this door or any of its children
            if (hit.collider.gameObject == gameObject || 
                hit.collider.transform.IsChildOf(transform))
            {
                // Use the actual hit point for angle calculation
                Vector3 toHitPoint = (hit.point - playerCamera.transform.position).normalized;
                float angle = Vector3.Angle(playerCamera.transform.forward, toHitPoint);
                
                return angle < maxInteractionAngle;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Called when player presses E while looking at this door
    /// </summary>
    public override void Interact()
    {
        if (isAnimating) return;
        
        Toggle();
    }
    
    /// <summary>
    /// Toggle the door open/closed
    /// </summary>
    public void Toggle()
    {
        if (isAnimating) return;
        
        isOpen = !isOpen;
        autoCloseTimer = 0f;
        UpdateTargetRotation();
        isAnimating = true;
    }
    
    /// <summary>
    /// Open the door
    /// </summary>
    public void Open()
    {
        if (isOpen || isAnimating) return;
        
        isOpen = true;
        autoCloseTimer = 0f;
        UpdateTargetRotation();
        isAnimating = true;
    }
    
    /// <summary>
    /// Close the door
    /// </summary>
    public void Close()
    {
        if (!isOpen || isAnimating) return;
        
        isOpen = false;
        autoCloseTimer = 0f;
        UpdateTargetRotation();
        isAnimating = true;
    }
    
    /// <summary>
    /// Calculate and set the target rotation based on door state
    /// </summary>
    private void UpdateTargetRotation()
    {
        if (doorObject == null)
        {
            Debug.LogWarning($"{gameObject.name}: doorObject is null in UpdateTargetRotation");
            return;
        }
        
        if (isOpen)
        {
            targetLocalRotation = initialLocalRotation * Quaternion.AngleAxis(openAngle, rotationAxis);
        }
        else
        {
            targetLocalRotation = initialLocalRotation;
        }
    }
    
    /// <summary>
    /// Smoothly animate the door towards its target rotation
    /// </summary>
    private void AnimateDoor()
    {
        if (doorObject == null) return;
        
        // Slerp towards target
        doorObject.localRotation = Quaternion.Slerp(
            doorObject.localRotation,
            targetLocalRotation,
            animationSpeed * Time.deltaTime
        );
        
        // Check if we've reached the target (within threshold)
        float angleDiff = Quaternion.Angle(doorObject.localRotation, targetLocalRotation);
        if (angleDiff < 0.1f)
        {
            doorObject.localRotation = targetLocalRotation;
            isAnimating = false;
        }
    }
    
    /// <summary>
    /// Get current door state
    /// </summary>
    public bool IsOpenState() => isOpen;
    
    /// <summary>
    /// Get if door is currently animating
    /// </summary>
    public bool IsAnimatingState() => isAnimating;
    
    // Editor helpers for auto-assigning references
    [ContextMenu("Auto Assign DoorObject")]
    private void AutoAssignDoorObject()
    {
        Transform child = transform.Find("DoorObject");
        if (child != null)
        {
            doorObject = child;
            initialLocalRotation = doorObject.localRotation;
            Debug.Log("Auto-assigned DoorObject");
        }
    }
    
    [ContextMenu("Auto Assign HingePoint")]
    private void AutoAssignHingePoint()
    {
        Transform child = transform.Find("HingePoint");
        if (child != null)
        {
            hingePoint = child;
            Debug.Log("Auto-assigned HingePoint");
        }
    }
    
    [ContextMenu("Auto Assign Handle")]
    private void AutoAssignHandle()
    {
        Transform child = transform.Find("Handle");
        if (child != null)
        {
            handle = child;
            Debug.Log("Auto-assigned Handle");
        }
    }
    
    [ContextMenu("Auto Assign All")]
    private void AutoAssignAll()
    {
        AutoAssignDoorObject();
        AutoAssignHingePoint();
        AutoAssignHandle();
    }
}
