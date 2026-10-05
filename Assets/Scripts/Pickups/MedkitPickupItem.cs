using UnityEngine;
using static PlayerInteractionSystem;
public class MedkitPickupItem : MonoBehaviour, IDoorInteractable
{
    public float healAmount = 50f;
    public AudioClip pickupSound;

    public void Interact()
    {
        PlayerHealthAndStamina healthSystem = FindFirstObjectByType<PlayerHealthAndStamina>();

        if (healthSystem != null)
        {
            bool used = healthSystem.UseMedkit(healAmount);

            if (used)
            {
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                Destroy(gameObject); //Consume item from the world
            }
        }
    }
}
