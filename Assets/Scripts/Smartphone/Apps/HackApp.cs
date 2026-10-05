using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HackApp : SmartphoneBaseApp
{
    public enum HackMode { QRScanner, Authenticator, WiFiConnector }

    [Header("Mode Navigation")]
    public HackMode currentMode = HackMode.QRScanner;
    public GameObject qrScannerPanel;
    public GameObject authenticatorPanel;
    public GameObject wifiPanel;

    [Header("QR Scanner Subsystem")]
    public Camera phoneCamera;
    public RectTransform scanReticle;
    public float raycastRange = 5f;
    public LayerMask qrCodeLayer;
    public TextMeshProUGUI qrStatusText;

    [Header("Authenticator Keypad Subsystem")]
    public GameObject keypadContainer;
    public TextMeshProUGUI authenticatorCodeDisplay;
    public TextMeshProUGUI minigamePromptText;
    private string targetCode = "";
    private string enteredCode = "";

    [Header("WiFi Connector Subsystem")]
    public float wifiScanRange = 10f;
    public LayerMask wifiTargetLayer;
    public Transform wifiDeviceListParent;
    public GameObject wifiDeviceButtonPrefab;
    public MessagingApp messagingAppReference;

    [Header("Mini-Game Subsystems")]
    public SequenceMemoryMiniGame sequenceGame;
    public ConnectDotsMinigame connectDotsGame;
    public ClearTheWayMinigame clearTheWayGame;

    private InteractiveDoor pendingDoorUnlock;

    public override void OpenApp()
    {
        base.OpenApp();
        SetMode(HackMode.QRScanner);
    }

    public override void CloseApp()
    {
        base.CloseApp();

        if (phoneCamera != null)
        {
            phoneCamera.enabled = false;
        }

        HideAllMiniGames();
    }

    private void Update()
    {
        if (!IsOpen) return;

        switch (currentMode)
        {
            case HackMode.QRScanner:
                UpdateQRScanner();
                break;
            case HackMode.WiFiConnector:
                // Passive Scanning handled via Button Refresh or Periodic Updates
                break;
        }
    }

    public void SetMode(int modeIndex)
    {
        SetMode((HackMode)modeIndex);
    }

    public void SetMode(HackMode mode)
    {
        currentMode = mode;

        if (qrScannerPanel != null) qrScannerPanel.SetActive(mode == HackMode.QRScanner);
        if (authenticatorPanel != null) authenticatorPanel.SetActive(mode == HackMode.Authenticator);
        if (wifiPanel != null) wifiPanel.SetActive(mode == HackMode.WiFiConnector);

        if (phoneCamera != null)
        {
            phoneCamera.enabled = (mode == HackMode.QRScanner);
        }

        if (mode != HackMode.Authenticator)
        {
            HideAllMiniGames();
        }

        if (mode == HackMode.WiFiConnector)
        {
            ScanWiFiDevices();
        }
    }

    // =================================
    // 1. QR CODE SCANNER MODE
    // =================================
    private void UpdateQRScanner()
    {
        if (phoneCamera == null) return;

        Ray ray = phoneCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, raycastRange, qrCodeLayer))
        {
            QRCodeTarget qrTarget = hit.collider.GetComponent<QRCodeTarget>();
            if (qrTarget != null && !qrTarget.isScanned)
            {
                qrTarget.OnScanned();
                if (qrStatusText != null) qrStatusText.text = $"Key Found:\n{qrTarget.keyIdentifier}";

                // Unlock linked safe or door directly
                if (qrTarget.linkedDoor != null)
                {
                    qrTarget.linkedDoor.UnlockViaSmartphoneHack();
                }
            }
        }
        else
        {
            if (qrStatusText != null) qrStatusText.text = "Align reticle with QR Key...";
        }
    }

    // ==================================
    // 2. AUTHENTICATOR MINI-GAME MODE
    // ==================================
    public void StartKeypadMinigame(InteractiveDoor targetDoor, string generatedCode)
    {
        SetMode(HackMode.Authenticator);
        HideAllMiniGames();

        pendingDoorUnlock = targetDoor;
        targetCode = generatedCode;
        enteredCode = "";

        if (keypadContainer != null) keypadContainer.SetActive(true);
        if (authenticatorCodeDisplay != null) authenticatorCodeDisplay.text = "____";
        if (minigamePromptText != null) minigamePromptText.text = $"Override target: {targetDoor.gameObject.name}";
    }

    public void InputKeypadDigit(string digit)
    {
        if (enteredCode.Length < targetCode.Length)
        {
            enteredCode += digit;
            if (authenticatorCodeDisplay != null) authenticatorCodeDisplay.text = enteredCode;

            if (enteredCode.Length == targetCode.Length)
            {
                VerifyAuthenticatorCode();
            }
        }
    }

    private void VerifyAuthenticatorCode()
    {
        if (enteredCode == targetCode)
        {
            if (authenticatorCodeDisplay != null) authenticatorCodeDisplay.text = "SUCCESS";
            OnPuzzleSolved();
        }
        else
        {
            if (authenticatorCodeDisplay != null) authenticatorCodeDisplay.text = "DENIED";
            enteredCode = "";
            OnPuzzleFailed();
        }
    }

    public void LaunchSequenceGame(InteractiveDoor door)
    {
        SetMode(HackMode.Authenticator);
        HideAllMiniGames();

        pendingDoorUnlock = door;
        if (sequenceGame != null)
        {
            sequenceGame.gameObject.SetActive(true);
            sequenceGame.InitializeMinigame(OnPuzzleSolved, OnPuzzleFailed);
        }
    }

    public void LaunchConnectDotsGame(InteractiveDoor door)
    {
        SetMode(HackMode.Authenticator);
        HideAllMiniGames();

        pendingDoorUnlock = door;
        if (connectDotsGame != null)
        {
            connectDotsGame.gameObject.SetActive(true);
            connectDotsGame.InitializeMinigame(OnPuzzleSolved, OnPuzzleFailed);
        }
    }

    public void LaunchClearTheWayGame(InteractiveDoor door)
    {
        SetMode(HackMode.Authenticator);
        HideAllMiniGames();

        pendingDoorUnlock = door;
        if (clearTheWayGame != null)
        {
            clearTheWayGame.gameObject.SetActive(true);
            clearTheWayGame.InitializeMinigame(OnPuzzleSolved, OnPuzzleFailed);
        }
    }

    private void HideAllMiniGames()
    {
        if (keypadContainer != null) keypadContainer.SetActive(false);
        if (sequenceGame != null) sequenceGame.gameObject.SetActive(false);
        if (connectDotsGame != null) connectDotsGame.gameObject.SetActive(false);
        if (clearTheWayGame != null) clearTheWayGame.gameObject.SetActive(false);
    }

    private void OnPuzzleSolved()
    {
        Debug.Log("[HackApp] Lock override successful!");
        if (pendingDoorUnlock != null)
        {
            pendingDoorUnlock.UnlockViaSmartphoneHack();
            pendingDoorUnlock = null;
        }
    }

    private void OnPuzzleFailed()
    {
        Debug.Log("[HackApp] Lock override failed.");
    }

    // =================================
    // 3. WIFI CONNECTOR MODE
    // =================================
    public void ScanWiFiDevices()
    {
        if (wifiDeviceListParent == null) return;

        foreach (Transform child in wifiDeviceListParent)
        {
            Destroy(child.gameObject);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, wifiScanRange, wifiTargetLayer);
        foreach (var hit in hits)
        {
            WiFiNode node = hit.GetComponent<WiFiNode>();
            if (node != null)
            {
                GameObject btnObj = Instantiate(wifiDeviceButtonPrefab, wifiDeviceListParent);
                TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                Button btn = btnObj.GetComponent<Button>();

                if (label != null) label.text = $"{node.networkSSID} ({node.signalStrength}%)";
                if (btn != null)
                {
                    WiFiNode targetNode = node;
                    btn.onClick.AddListener(() => ConnectToWiFiNode(targetNode));
                }
            }
        }
    }

    private void ConnectToWiFiNode(WiFiNode node)
    {
        if (node == null) return;

        // 1. Trigger story texts if router has payload
        if (!string.IsNullOrEmpty(node.storySenderName) && messagingAppReference != null)
        {
            messagingAppReference.ReceiveIncomingMessage(node.storySenderName, node.incomingStoryText);
        }

        // 2. Launch linked mini-game or keypad code override
        if (node.linkedDoor != null)
        {
            if (!string.IsNullOrEmpty(node.accessCode))
            {
                StartKeypadMinigame(node.linkedDoor, node.accessCode);
            }
            else
            {
                LaunchSequenceGame(node.linkedDoor);
            }
        }
    }
}
