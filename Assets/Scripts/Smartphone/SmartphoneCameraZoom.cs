using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SmartphoneCameraZoom : MonoBehaviour
{
    [Header("Camera and Render Targets")]
    public Camera phoneRenderCamera;    // Dedicated camera outputting to Phone Render Texture
    public float minFOV = 10f;          // Maximum Zoom in (e.g., 10x Optical)
    public float maxFOV = 60f;          // Default FOV (1x Normal View)
    public float zoomSpeed = 20f;
    public float smoothTime = 0.1f;

    [Header("UI Readout")]
    public TextMeshProUGUI zoomMultiplierText; // Displays "1.0x", "2.5x", etc.
    public RectTransform zoomSliderIndicator; // Optional UI Tick Mark/Bar

    [Header("Audio and Haptics")]
    public AudioSource phoneAudioSource;
    public AudioClip zoomClickSound;

    private float targetFOV;
    private float fovVelocity;
    private PlayerInteractionSystem interactionSystem;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (phoneRenderCamera == null)
        {
            phoneRenderCamera = GetComponent<Camera>();
        }

        if (phoneRenderCamera != null)
        {
            targetFOV = phoneRenderCamera.fieldOfView;
        }

        interactionSystem = GetComponentInParent<PlayerInteractionSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleZoomInput();
        ApplySmoothZoom();
        UpdateUI(); 
    }

    private void HandleZoomInput()
    {
        // Only allow zooming when actively using the Smartphone
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            // Scroll Up = Zoom In (Decrease FOV), Scroll Down = Zoom Out (Increase FOV)
            targetFOV -= scrollInput * zoomSpeed;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);

            if (phoneAudioSource != null && zoomClickSound != null && !phoneAudioSource.isPlaying)
            {
                phoneAudioSource.PlayOneShot(zoomClickSound, 0.3f);
            }
        }
    }

    private void ApplySmoothZoom()
    {
        if (phoneRenderCamera == null) return;

        // Smooth interpolate the camera's FOV toward the target
        phoneRenderCamera.fieldOfView = Mathf.SmoothDamp(
            phoneRenderCamera.fieldOfView,
            targetFOV,
            ref fovVelocity,
            smoothTime
            );
    }

    private void UpdateUI()
    {
        if (zoomMultiplierText != null && phoneRenderCamera != null)
        {
            // Calculate current zoom level ratio relative to max FOV
            float zoomRatio = maxFOV / phoneRenderCamera.fieldOfView;
            zoomMultiplierText.text = $"{zoomRatio:F1}x";
        }

        if (zoomSliderIndicator != null)
        {
            // Map FOV to 0-1 range for a UI slider graphic
            float t = Mathf.InverseLerp(maxFOV, minFOV, phoneRenderCamera.fieldOfView);
            zoomSliderIndicator.anchorMin = new Vector2(t, zoomSliderIndicator.anchorMin.y);
            zoomSliderIndicator.anchorMax = new Vector2(t, zoomSliderIndicator.anchorMax.y);
        }
    }

    // Helper method to reset zoom when phone is put away
    public void ResetZoom()
    {
        targetFOV = maxFOV;
        if (phoneRenderCamera != null)
        {
            phoneRenderCamera.fieldOfView = maxFOV;
        }
    }
}
