using TMPro;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class SmartphoneController : MonoBehaviour
{
    [Header("UI & Display")]
    public GameObject phoneScreenRoot;
    public GameObject homeScreenContainer;
    public GameObject phoneShutdownOverlay; // Black Screen When Battery Dies
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI batteryText;

    [Header("Battery Settings")]
    public float maxBattery = 100f;
    public float currentBattery = 100f;
    public float idleDrainRate = 0.05f; // Drain per second when phone is raised/on home screen
    public bool isCharging { get; private set; }

    [Header("Subsystems & Modules")]
    public SmartphoneFlashlight flashlight;
    public List<SmartphoneBaseApp> installedApps = new List<SmartphoneBaseApp>();

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip phoneRaiseSound;
    public AudioClip buttonClickSound;
    public AudioClip batteryLowSound;

    public bool IsPhoneRaised { get; private set; }
    public SmartphoneBaseApp currentActiveApp { get; private set; }

    private Animator phoneAnimator;
    private PowerBankPickupItem attachedPowerBank;

    private void Awake()
    {
        phoneAnimator = GetComponent<Animator>();
        currentBattery = maxBattery;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CloseAllApps();
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle phone view (e.g., Press 'TAB' or 'P')
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSmartphone();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (flashlight != null) flashlight.ToggleFlashlight();
        }

        HandleBatteryDrainAndCharging();

        if (IsPhoneRaised)
        {
            UpdateStatusHeader();
        }
    }

    private void HandleBatteryDrainAndCharging()
    {
        // 1. Handle Power Bank Charging
        if (isCharging && attachedPowerBank != null)
        {
            if (attachedPowerBank.HasCharge && currentBattery < maxBattery)
            {
                float neededBattery = maxBattery - currentBattery;
                float chargeThisFrame = attachedPowerBank.chargeTransferRate * Time.deltaTime;
                float actualChargeGiven = attachedPowerBank.DrainPowerBank(Mathf.Min(neededBattery, chargeThisFrame));

                currentBattery += actualChargeGiven;

                // Turn off shutdown screen if battery restored above 0
                if (currentBattery > 1f && phoneShutdownOverlay != null && phoneShutdownOverlay.activeSelf)
                {
                    phoneShutdownOverlay.SetActive(false);
                }
            }
            else if (!attachedPowerBank.HasCharge)
            {
                DetachPowerBank(); // Discard empty power bank
            }
        }

        // 2. Handle Battery Drain
        if (currentBattery > 0f)
        {
            float totalDrain = 0f;

            // Idle battery drain when phone is raised
            if (IsPhoneRaised)
            {
                totalDrain += idleDrainRate;
            }

            // App battery drain
            if (currentActiveApp != null && currentActiveApp.IsOpen)
            {
                totalDrain += idleDrainRate * currentActiveApp.batteryDrainRateMultiplier;
            }

            // Flashlight battery drain (highest priority drain)
            if (flashlight != null && flashlight.IsOn)
            {
                totalDrain += flashlight.flashlightBatteryDrainRate;
            }

            currentBattery -= totalDrain * Time.deltaTime;

            if (currentBattery <= 0f)
            {
                currentBattery = 0f;
                OnBatteryDepleted();
            }
        }
    }

    private void OnBatteryDepleted()
    {
        Debug.Log("Smartphone battery completely drained!");

        // Turn off flashlight
        if (flashlight != null) flashlight.SetFlashlightState(false);

        // Force close all apps
        CloseAllApps();

        // Show shutdown screen overlay
        if (phoneShutdownOverlay != null)
        {
            phoneShutdownOverlay.SetActive(true);
        }

        if (audioSource != null && batteryLowSound != null)
        {
            audioSource.PlayOneShot(batteryLowSound);
        }
    }

    public void AttachPowerBank(PowerBankPickupItem powerBank)
    {
        if (powerBank == null || !powerBank.HasCharge) return;

        attachedPowerBank = powerBank;
        isCharging = true;
        Debug.Log("Power Bank connected to Smartphone.");
    }

    public void DetachPowerBank()
    {
        attachedPowerBank = null;
        isCharging = false;
        Debug.Log("Power Bank disconnected.");
    }

    public void ToggleSmartphone()
    {
        IsPhoneRaised = !IsPhoneRaised;

        if (phoneAnimator != null)
        {
            phoneAnimator.SetBool("IsRaised", IsPhoneRaised);
        }
        else if (phoneScreenRoot != null)
        {
            phoneScreenRoot.SetActive(IsPhoneRaised);
        }

        if (audioSource && phoneRaiseSound) audioSource.PlayOneShot(phoneRaiseSound);

        if (!IsPhoneRaised)
        {
            CloseAllApps();
        }
    }

    public void OpenApp(SmartphoneBaseApp appToOpen)
    {
        if (appToOpen == null) return;

        CloseAllApps();
        currentActiveApp = appToOpen;
        currentActiveApp.OpenApp();

        if (homeScreenContainer != null) homeScreenContainer.SetActive(false);
        if (audioSource && buttonClickSound) audioSource.PlayOneShot(buttonClickSound);
    }

    public void ReturnToHomeScreen()
    {
        CloseAllApps();
        if (homeScreenContainer != null) homeScreenContainer.SetActive(true);
        if (audioSource && buttonClickSound) audioSource.PlayOneShot(buttonClickSound);
    }

    private void CloseAllApps()
    {
        foreach (var app in installedApps)
        {
            app.CloseApp();
        }
        currentActiveApp = null;
    }

    private void UpdateStatusHeader()
    {
        if (clockText != null)
        {
            clockText.text = System.DateTime.Now.ToString("HH:mm");
        }

        if (batteryText != null)
        {
            // Calculate battery display based on System info or gameplay state
            int batteryLevel = Mathf.RoundToInt(SystemInfo.batteryLevel * 100);
            batteryText.text = batteryLevel > 0 ? $"{batteryLevel}%" : "84%";
        }
    }
}
