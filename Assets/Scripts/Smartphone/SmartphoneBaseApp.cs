using Unity.VisualScripting;
using UnityEngine;

public class SmartphoneBaseApp : MonoBehaviour
{
    [Header("App Config")]
    public string appName = "App";
    public Sprite appIcon;
    public GameObject appUIContainer;

    [Header("Batter Consumption")]
    [Tooltip("Base Battery Drain Multiplier Per Second While This App is Open")]
    public float batteryDrainRateMultiplier = 1.0f;

    public bool IsOpen { get; protected set; }

    public virtual void OpenApp()
    {
        IsOpen = true;
        if (appUIContainer != null) appUIContainer.SetActive(true);
    }

    public virtual void CloseApp()
    {
        IsOpen = false;
        if (appUIContainer != null) appUIContainer.SetActive(false);
    }
}
