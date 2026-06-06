using UnityEngine;

/// <summary>
/// Base class for all interactable objects
/// Provides common interaction functionality
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] protected float maxInteractionDistance = 3f;
    [SerializeField] protected float maxInteractionAngle = 45f;
    
    // Reference to the player's camera
    protected Camera playerCamera;
    
    protected virtual void Awake()
    {
        // Find the player camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }
    
    protected virtual void OnEnable()
    {
        SubscribeToInputManager();
    }
    
    private void SubscribeToInputManager()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnInteract += OnInteractInput;
        }
        else
        {
            // Retry after a short delay in case PlayerInputManager hasn't initialized yet
            Invoke(nameof(SubscribeToInputManager), 0.1f);
        }
    }
    
    protected virtual void OnDisable()
    {
        // Unsubscribe from interact event
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnInteract -= OnInteractInput;
        }
    }
    
    /// <summary>
    /// Called when the player presses the interact button
    /// </summary>
    protected virtual void OnInteractInput()
    {
        if (!CanInteract()) return;
        
        Interact();
    }
    
    /// <summary>
    /// Check if the player can interact with this object
    /// </summary>
    protected virtual bool CanInteract()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return false;
        }
        
        // Check distance
        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        if (distance > maxInteractionDistance) return false;
        
        // Check if player is looking at this object
        return IsPlayerLookingAtObject();
    }
    
    /// <summary>
    /// Check if the player is looking at this object
    /// </summary>
    protected virtual bool IsPlayerLookingAtObject()
    {
        if (playerCamera == null) return false;
        
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        // Raycast against all physics layers, including trigger colliders
        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance, ~0, QueryTriggerInteraction.Collide))
        {
            // Check if we hit this object or one of its children
            if (hit.collider.gameObject == gameObject || 
                hit.collider.transform.IsChildOf(transform))
            {
                // Check angle between camera forward and direction to object
                Vector3 toObject = (transform.position - playerCamera.transform.position).normalized;
                float angle = Vector3.Angle(playerCamera.transform.forward, toObject);
                
                return angle < maxInteractionAngle;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Called when the player successfully interacts with this object
    /// Override this in child classes
    /// </summary>
    public abstract void Interact();
    
    /// <summary>
    /// Get the interaction distance
    /// </summary>
    public virtual float GetInteractionDistance()
    {
        return maxInteractionDistance;
    }
    
    /// <summary>
    /// Check if this object is currently interactable
    /// </summary>
    public virtual bool IsInteractable()
    {
        return enabled && gameObject.activeInHierarchy;
    }
}
