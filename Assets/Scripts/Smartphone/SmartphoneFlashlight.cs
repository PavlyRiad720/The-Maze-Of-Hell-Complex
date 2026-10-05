using UnityEngine;

public class SmartphoneFlashlight : MonoBehaviour
{
    [Header("Light Config")]
    public Light flashlightSpotlight;
    public float flashlightBatteryDrainRate = 1.5f; // Extra Drain Per Second When Lit

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip toggleSound;

    public bool IsOn { get; private set; }

    private void Start()
    {
        if (flashlightSpotlight != null)
        {
            flashlightSpotlight.enabled = false;
        }
    }

    public void ToggleFlashlight()
    {
        SetFlashlightState(!IsOn);
    }

    public void SetFlashlightState(bool turnOn)
    {
        IsOn = turnOn;
        if (flashlightSpotlight != null)
        {
            flashlightSpotlight.enabled = IsOn;
        }

        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }
    }
}
