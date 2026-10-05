using UnityEngine;

public class QRCodeTarget : MonoBehaviour
{
    public string keyIdentifier = "SAFE_KEY_01";
    public InteractiveDoor linkedDoor;
    public bool isScanned { get; private set; }

    public void OnScanned()
    {
        isScanned = true;
        Debug.Log($"QR Code scanned: {keyIdentifier}");
    }
}
