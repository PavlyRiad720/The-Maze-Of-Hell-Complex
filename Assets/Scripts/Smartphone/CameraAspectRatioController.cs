using UnityEngine;

public class CameraAspectRatioController : MonoBehaviour
{
    public enum AspectRatioMode { Free, Aspect4x3, Aspect16x9, Square1x1, Vertical9x16 }

    [Header("Target Camera")]
    public Camera targetCamera;

    [Header("Aspect Settings")]
    public AspectRatioMode cameraAppAspectRatio = AspectRatioMode.Aspect4x3;
    public bool applyToMainCamera = true;

    private Rect originalViewportRect;
    private bool isAspectLocked = false;

    public bool IsAspectLocked => isAspectLocked;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (targetCamera != null)
        {
            originalViewportRect = targetCamera.rect;
        }
    }

    public void EnableCameraAppAspectRatio()
    {
        if (targetCamera == null) return;

        float targetAspect = GetTargetAspectRatio(cameraAppAspectRatio);
        float currentScreenAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = currentScreenAspect / targetAspect;

        Rect newRect = targetCamera.rect;

        // Pillarbox (black bars on left/right)
        if (scaleHeight < 1.0f)
        {
            newRect.width = 1.0f;
            newRect.height = scaleHeight;
            newRect.x = 0;
            newRect.y = (1.0f - scaleHeight) / 2.0f;
        }
        // Letterbox (black bars on top/bottom)
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            newRect.width = scaleWidth;
            newRect.height = 1.0f;
            newRect.x = (1.0f - scaleWidth) / 2.0f;
            newRect.y = 0;
        }

        targetCamera.rect = newRect;
        isAspectLocked = true;
    }

    public void ResetToDefaultAspectRatio()
    {
        if (targetCamera == null) return;

        targetCamera.rect = originalViewportRect;
        isAspectLocked = false;
    }

    private float GetTargetAspectRatio(AspectRatioMode mode)
    {
        switch (mode)
        {
            case AspectRatioMode.Aspect4x3:
                return 4f / 3f;
            case AspectRatioMode.Aspect16x9:
                return 16f / 9f;
            case AspectRatioMode.Square1x1:
                return 1f;
            case AspectRatioMode.Vertical9x16:
                return 9f / 16f;
            default:
                return (float)Screen.width / (float)Screen.height;
        }
    }
}
