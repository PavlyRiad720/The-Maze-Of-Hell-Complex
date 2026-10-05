using System.Collections;
using UnityEngine;

public class CameraApp : SmartphoneBaseApp
{
    [Header("Aspect Ratio Controller")]
    public CameraAspectRatioController aspectController;

    [Header("Zoom System Reference")]
    public SmartphoneCameraZoom zoomController;

    [Header("Gallery Integration")]
    public GalleryApp galleryAppReference;

    [Header("Shutter Feedback & Audio")]
    public CanvasGroup shutterFlashGroup;
    public AudioSource audioSource;
    public AudioClip shutterSound;

    private void Awake()
    {
        if (zoomController == null)
        {
            zoomController = GetComponent<SmartphoneCameraZoom>();
        }

        if (aspectController == null)
        {
            aspectController = FindFirstObjectByType<CameraAspectRatioController>();
        }
    }

    public override void OpenApp()
    {
        base.OpenApp();

        // 1. Lock aspect ratio when opening camera app
        if (aspectController != null)
        {
            aspectController.EnableCameraAppAspectRatio();
        }

        // 2. Enable camera zoom script
        if (zoomController != null)
        {
            zoomController.enabled = true;
            if (zoomController.phoneRenderCamera != null)
            {
                zoomController.phoneRenderCamera.enabled = true;
            }
        }
    }

    public override void CloseApp()
    {
        base.CloseApp();

        // 1. Reset aspect ratio back to standard full screen
        if (aspectController != null)
        {
            aspectController.ResetToDefaultAspectRatio();
        }

        // 2. Reset zoom and disable render camera
        if (zoomController != null)
        {
            zoomController.ResetZoom();
            if (zoomController.phoneRenderCamera != null)
            {
                zoomController.phoneRenderCamera.enabled = false;
            }
            zoomController.enabled = false;
        }
    }

    public void TakePhoto()
    {
        // Capture photo and save to gallery
        if (galleryAppReference != null)
        {
            galleryAppReference.CapturePhoto();
        }

        // Shutter audio feedback
        if (audioSource != null && shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }

        // Visual flash feedback
        StartCoroutine(TriggerShutterFlash());
    }

    private IEnumerator TriggerShutterFlash()
    {
        if (shutterFlashGroup == null) yield break;

        shutterFlashGroup.alpha = 1f;
        while (shutterFlashGroup.alpha > 0f)
        {
            shutterFlashGroup.alpha -= Time.deltaTime * 6f;
            yield return null;
        }
        shutterFlashGroup.alpha = 0f;
    }
}
