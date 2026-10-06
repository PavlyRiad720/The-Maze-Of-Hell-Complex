using Unity.VisualScripting;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    [Tooltip("Mouse Sensitivity Along X and Y axes")]
    public float mouseSensitivity = 100f;

    [Header("Pitch Constraints")]
    [Tooltip("Maximum angle you can look up in degrees")]
    public float minPitch = -80f;
    [Tooltip("Maximum angly you can look down in degrees")]
    public float maxPitch = 80f;

    [Header("References")]
    [Tooltip("Parent Player Transform To Rotate Horizontally With The Camera")]
    public Transform playerBody;

    // Internal Rotation Tracking
    private float xRotation = 0f;
    private bool isCursorLocked = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Auto-assign player body if missing and camera is childed
        if (playerBody == null && transform.parent != null)
        {
            playerBody = transform.parent;
        }

        LockCursor(true);
    }

    // Update is called once per frame
    void Update()
    {
        // Optional: Toggle cursor lock state with Escape key for debugging/UI
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(!isCursorLocked);
        }

        // Left-click inside the Game view to instantly regain focus and lock cursor
        if (!isCursorLocked && Input.GetMouseButton(0))
        {
            LockCursor(true);
        }

        // Do not rotate camera while cursor is free/unlocked for UI interaction
        if (!isCursorLocked) return;

        // Get raw mouse input scaled by sensitivity and frame time
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical Look (Pitch) - Inverted axis for standard FPS controls
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        // Apply pitch locally to Camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal Look (Yaw) - Rotate the entire Player body
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    /// <summary>
    /// Locks/Unlocks mouse cursor for switching between gameplay and smartphone/UI menus.
    /// </summary>
    public void LockCursor(bool shouldLock)
    {
        isCursorLocked = shouldLock;
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
    }
}
