using UnityEngine;
using static PlayerInteractionSystem;
public class MapPickupItem : MonoBehaviour, IDoorInteractable
{
    public string targetSectorToUnlock = ""; // Leave blank to reveal whole map
    public AudioClip pickupSound;

    public void Interact()
    {
        MapApp mapApp = FindFirstObjectByType<MapApp>();

        if (mapApp != null)
        {
            if (string.IsNullOrEmpty(targetSectorToUnlock))
            {
                mapApp.RevealFullMapLayout();
            }
            else
            {
                mapApp.DiscoverSector(targetSectorToUnlock);
            }

            if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            Destroy(gameObject);
        }
    }
}
