using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteractionSystem : MonoBehaviour
{
    [Header("Interaction Config")]
    public float interactRange = 2.5f;
    public LayerMask interactableLayer;
    public Camera playerCamera;

    [Header("Smartphone Systems")]
    public GameObject phone3DModel; // Handheld 3D Phone Object
    public GameObject phoneOverlayCanvas; // 2D Fullscreen App UI Canvas
    public Light phoneFlashLight;
    private bool isPhoneOverlayActive = false;

    [Header("Backpack Systems")]
    public GameObject backpackCanvas; // Physical Backpack Overlay UI
    private bool isBackPackOpen = false;

    private PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (phoneOverlayCanvas != null)
        {
            phoneOverlayCanvas.SetActive(false);
        }
        if (backpackCanvas != null)
        {
            backpackCanvas.SetActive(false); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputTogggles();
        PerformInteractionRaycast();
    }

    private void HandleInputTogggles()
    {
        // Quick Toggle Flashlight (3D Hand Mode)
        if (Input.GetKeyDown(KeyCode.F) && phoneFlashLight != null)
        {
            phoneFlashLight.enabled = !phoneFlashLight.enabled;
        }

        // Toggle Smartphone Overlay Mode (Tab)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSmartphoneOverlay();
        }

        // Toggle Backpack Inventory (B)
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBackpack();
        }

        // Auto-close overlay screens if player sprints
        if (playerController.IsSprinting && playerController.IsMoving)
        {
            if (isPhoneOverlayActive) ToggleSmartphoneOverlay();
            if (isBackPackOpen) ToggleBackpack();
        }
    }

    public void ToggleSmartphoneOverlay()
    {
        isPhoneOverlayActive = !isPhoneOverlayActive;
        if (phoneOverlayCanvas == null) phoneOverlayCanvas.SetActive(isPhoneOverlayActive);
        if (phone3DModel != null) phone3DModel.SetActive(!isPhoneOverlayActive);
    }

    public void ToggleBackpack()
    {
        isBackPackOpen = !isBackPackOpen;
        if (backpackCanvas != null) backpackCanvas.SetActive(isBackPackOpen);
    }

    private void PerformInteractionRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Trigger Door opening, item pickup, or WiFi checkpoint access
                IDoorInteractable door = hit.collider.GetComponent<IDoorInteractable>();

                if (door != null)
                {
                    door.Interact();
                }
            }
        }
    }

    public interface IDoorInteractable
    {
        void Interact();
    }
}
