using UnityEngine;
using static PlayerInteractionSystem;

public class PowerBankStation : MonoBehaviour, IDoorInteractable
{
    [Header("Station Settings")]
    public PowerBankPickupItem stockedPowerBank;
    public GameObject powerBankMesh;
    public AudioClip pickupSound;

    public void Interact()
    {
        if (stockedPowerBank == null || !stockedPowerBank.HasCharge)
        {
            Debug.Log("Power Bank station is empty!");
            return;
        }

        SmartphoneController phone = FindFirstObjectByType<SmartphoneController>();
        if (phone != null)
        {
            // Attach power bank directly to phone
            phone.AttachPowerBank(stockedPowerBank);

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            if (powerBankMesh != null)
            {
                powerBankMesh.SetActive(false); // Hide power bank from dock
            }

            Debug.Log("Took Power Bank from station.");
        }
    }
}
}
