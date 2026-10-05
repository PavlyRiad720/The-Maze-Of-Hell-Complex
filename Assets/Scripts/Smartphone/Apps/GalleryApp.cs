using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GalleryApp : SmartphoneBaseApp
{
    [Header("Camera and Capture Setup")]
    public Camera photoCamera;
    public RenderTexture captureRenderTexture;

    [Header("Gallery UI Elements")]
    public Transform gridContentParent;
    public GameObject mediaThumbnailPrefab;
    public RawImage fullScreenImageViewer;
    public VideoPlayer videoPlayer;

    private List<Texture2D> capturedPhotos = new List<Texture2D>();
    private List<string> recordedVideoPaths = new List<string>();

    public override void OpenApp()
    {
        base.OpenApp();
        RefreshGalleryGrid();
    }

    public void CapturePhoto()
    {
        if (photoCamera == null || captureRenderTexture == null) return;

        // Render from capture camera to texture
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = captureRenderTexture;

        photoCamera.Render();

        Texture2D photo = new Texture2D(captureRenderTexture.width, captureRenderTexture.height, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, captureRenderTexture.width, captureRenderTexture.height), 0, 0);
        photo.Apply();

        RenderTexture.active = currentRT;

        capturedPhotos.Add(photo);
    }

    private void RefreshGalleryGrid()
    {
        // Clear old thumbnails
        foreach (Transform child in gridContentParent)
        {
            Destroy(child.gameObject);
        }

        // Display captured photos
        foreach (var tex in capturedPhotos)
        {
            GameObject thumb = Instantiate(mediaThumbnailPrefab, gridContentParent);
            RawImage img = thumb.GetComponentInChildren<RawImage>();
            Button btn = thumb.GetComponent<Button>();

            if (img != null) img.texture = tex;
            if (btn != null) btn.onClick.AddListener(() => DisplayFullScreenPhoto(tex));
        }
    }

    public void DisplayFullScreenPhoto(Texture2D tex)
    {
        if (fullScreenImageViewer == null) return;

        fullScreenImageViewer.gameObject.SetActive(true);
        fullScreenImageViewer.texture = tex;
    }

    public void CloseFullScreenViewer()
    {
        if (fullScreenImageViewer != null) fullScreenImageViewer.gameObject.SetActive(false);
        if (videoPlayer != null && videoPlayer.isPlaying) videoPlayer.Stop();
    }
}
