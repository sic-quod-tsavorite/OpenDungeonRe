using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person mouse look controller for OpenDungeonRe
/// Handles mouse camera rotation with configurable sensitivity and clamping
/// </summary>
public class MouseLook : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;
    [SerializeField] private bool invertYAxis = false;
    [SerializeField] private bool lockCursor = true;

    [Header("Smoothing")]
    [SerializeField] private bool useSmoothing = false;
    [SerializeField] private float smoothTime = 0.1f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector2 currentMouseDelta;
    private Vector2 mouseDeltaVelocity;

    // Input actions
    private InputAction lookAction;

    // Camera transform (cached for performance)
    private Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;

        // Get input action
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            lookAction = playerInput.actions["Look"];
        }

        // Lock and hide cursor
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnEnable()
    {
        // Re-lock cursor when enabled
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDisable()
    {
        // Unlock cursor when disabled
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (lookAction != null)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            if (useSmoothing)
            {
                // Apply smoothing
                currentMouseDelta = Vector2.SmoothDamp(
                    currentMouseDelta,
                    lookInput * mouseSensitivity * Time.deltaTime,
                    ref mouseDeltaVelocity,
                    smoothTime
                );
            }
            else
            {
                currentMouseDelta = lookInput * mouseSensitivity * Time.deltaTime;
            }

            HandleRotation(currentMouseDelta);
        }
    }

    private void HandleRotation(Vector2 mouseDelta)
    {
        // Vertical rotation (up/down - pitch)
        float y = mouseDelta.y * (invertYAxis ? 1f : -1f);
        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        // Horizontal rotation (left/right - yaw)
        float x = mouseDelta.x;
        yRotation += x;

        // Apply rotation to camera (vertical)
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Apply rotation to player body (horizontal)
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    /// <summary>
    /// Sets the cursor lock state
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
    /// Resets camera rotation to default
    /// </summary>
    public void ResetRotation()
    {
        xRotation = 0f;
        yRotation = 0f;
        cameraTransform.localRotation = Quaternion.identity;
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Gets the current camera transform
    /// </summary>
    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    /// <summary>
    /// Gets the current vertical rotation angle
    /// </summary>
    public float GetVerticalAngle()
    {
        return xRotation;
    }

    /// <summary>
    /// Gets the current horizontal rotation angle
    /// </summary>
    public float GetHorizontalAngle()
    {
        return yRotation;
    }
}
