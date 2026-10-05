using UnityEngine;

[System.Serializable]
public class PowerBankPickupItem : MonoBehaviour
{
    [Header("Power Bank Capacity")]
    public float maxCapacity = 100f;
    public float currentCapacity = 100f;
    public float chargeTransferRate = 15f; // Charge per second transferred to phone

    public bool HasCharge => currentCapacity > 0f;

    public float DrainPowerBank(float requestedAmount)
    {
        float amountExtracted = Mathf.Min(currentCapacity, requestedAmount);
        currentCapacity -= amountExtracted;
        return amountExtracted;
    }
}
