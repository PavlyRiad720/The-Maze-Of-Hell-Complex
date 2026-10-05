using UnityEngine;

public class WiFiNode : MonoBehaviour
{
    [Header("Network Config")]
    public string networkSSID = "SECURE_DOOR_WIFI";
    public int signalStrength = 85;

    [Header("Story Message Trigger")]
    public string storySenderName = "";
    [TextArea(2, 4)] public string incomingStoryText = "";

    [Header("Linked Door & Access Code")]
    public InteractiveDoor linkedDoor;
    public string accessCode = "1337";
}
