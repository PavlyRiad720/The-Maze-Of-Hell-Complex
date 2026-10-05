using UnityEngine;

public class StoryMessageTrigger : MonoBehaviour
{
    public string senderContact = "Unknown Number";
    [TextArea(2, 4)] public string messageBody = "Don't open that door...";
    public bool destroyOnTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MessagingApp msgApp = FindFirstObjectByType<MessagingApp>();
            if (msgApp != null)
            {
                msgApp.ReceiveIncomingMessage(senderContact, messageBody);
            }

            if (destroyOnTrigger) Destroy(gameObject);
        }
    }
}
