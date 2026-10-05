using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.LookDev;

public class MessagingApp : SmartphoneBaseApp
{
    [System.Serializable]
    public class ChoiceOption
    {
        public string choiceText;
        public MessageNode nextNode;
    }

    [System.Serializable]
    public class MessageNode
    {
        public string senderName;
        [TextArea(2, 5)] public string messageContent;
        public bool isFromPlayer;
        public List<ChoiceOption> choices = new List<ChoiceOption>();
    }

    [System.Serializable]
    public class ContactThread
    {
        public string contactName;
        public Sprite contactAvatar;
        public List<MessageNode> messageHistory = new List<MessageNode>();
    }

    [Header("Contacts and Threads")]
    public List<ContactThread> activeThreads = new List<ContactThread>();
    private ContactThread currentThread;

    [Header("UI References")]
    public Transform chatContainerParent;
    public GameObject incomingMessagePrefab;
    public GameObject outgoingMessagePrefab;
    public Transform choicesContainerParent;
    public GameObject choiceButtonPrefab;
    public TextMeshProUGUI activeContactHeader;

    [Header("Notification Settings")]
    public AudioSource audioSource;
    public AudioClip messageReceivedSound;
    public GameObject unreadBadgeIndicator;

    public bool HasUnreadMessages { get; private set; }

    public override void OpenApp()
    {
        base.OpenApp();
        HasUnreadMessages = false;
        if (unreadBadgeIndicator != null) unreadBadgeIndicator.SetActive(false);

        if (activeThreads.Count > 0)
        {
            OpenThread(activeThreads[0]);
        }
    }

    public void OpenThread(ContactThread thread)
    {
        currentThread = thread;
        if (activeContactHeader != null) activeContactHeader.text = thread.contactName;

        RefreshChatUI();
    }

    public void ReceiveIncomingMessage(string contactName, string textMessage, List<ChoiceOption> responseChoices = null)
    {
        ContactThread thread = activeThreads.Find(t => t.contactName == contactName);
        if (thread == null)
        {
            thread = new ContactThread { contactName = contactName };
            activeThreads.Add(thread);
        }

        MessageNode newNode = new MessageNode
        {
            senderName = contactName,
            messageContent = textMessage,
            isFromPlayer = false,
            choices = responseChoices ?? new List<ChoiceOption>()
        };

        thread.messageHistory.Add(newNode);

        if (!IsOpen)
        {
            HasUnreadMessages = true;
            if (unreadBadgeIndicator != null) unreadBadgeIndicator.SetActive(true);
        }

        if (audioSource != null && messageReceivedSound != null)
        {
            audioSource.PlayOneShot(messageReceivedSound);
        }

        if (IsOpen && currentThread == thread)
        {
            RefreshChatUI();
        }
    }

    private void RefreshChatUI()
    {
        if (currentThread == null) return;

        // Clear existing message bubbles
        foreach (Transform child in chatContainerParent) Destroy(child.gameObject);
        foreach (Transform child in choicesContainerParent) Destroy(child.gameObject);

        // Render Chat History
        MessageNode lastNode = null;
        foreach (var msg in currentThread.messageHistory)
        {
            GameObject prefab = msg.isFromPlayer ? outgoingMessagePrefab : incomingMessagePrefab;
            GameObject bubble = Instantiate(prefab, chatContainerParent);
            TextMeshProUGUI txt = bubble.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = msg.messageContent;

            lastNode = msg;
        }

        // Display Choices if available on the last incoming message
        if (lastNode != null && !lastNode.isFromPlayer && lastNode.choices.Count > 0)
        {
            foreach (var choice in lastNode.choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainerParent);
                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                Button btn = btnObj.GetComponent<Button>();

                if (btnText != null) btnText.text = choice.choiceText;
                if (btn != null)
                {
                    ChoiceOption opt = choice;
                    btn.onClick.AddListener(() => SelectPlayerChoice(opt));
                }
            }
        }
    }

    private void SelectPlayerChoice(ChoiceOption selectedChoice)
    {
        if (currentThread == null) return;

        // Add player reply to history
        MessageNode playerReply = new MessageNode
        {
            senderName = "Player",
            messageContent = selectedChoice.choiceText,
            isFromPlayer = true
        };
        currentThread.messageHistory.Add(playerReply);

        // Trigger next NPC node response if connected
        if (selectedChoice.nextNode != null)
        {
            currentThread.messageHistory.Add(selectedChoice.nextNode);
        }

        RefreshChatUI();
    }
}
