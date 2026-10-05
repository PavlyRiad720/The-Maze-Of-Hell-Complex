using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveDoor : MonoBehaviour
{
    public enum DoorLockType { Unlocked, QRKeyRequired, AuthenticatorRequired, WiFiHackOnly }
    public enum MotionType { HingeRotation, SlidingTranslation }

    [Header("Lock Configuration")]
    public DoorLockType lockType = DoorLockType.AuthenticatorRequired;
    public string doorKeyID = "DOOR_SEC_01";
    public bool isLocked = true;
    public bool isOpen = false;

    [Header("Door Motion Setup")]
    public MotionType motionType = MotionType.HingeRotation;
    [Tooltip("Hinge or sliding panel transform. If left unassigned, uses this transform.")]
    public Transform doorPanelTransform;

    [Header("Rotation Limits (Local Y-Axis)")]
    public float closedAngle = 0f;
    public float openAngle = 90f;

    [Header("Sliding Limits (Local Position Offset)")]
    public Vector3 closedLocalPos = Vector3.zero;
    public Vector3 openLocalPos = new Vector3(1.5f, 0f, 0f);

    [Header("Opening Speeds & Thresholds")]
    [Tooltip("Time in seconds to differentiate a tap from a click-and-hold drag.")]
    public float clickHoldThreshold = 0.2f;
    public float normalOpenSpeed = 3f;
    public float slowPeekDragSensitivity = 80f;
    public float smashOpenSpeed = 12f;

    [Header("Audio & Visual Feedback")]
    public AudioSource audioSource;
    public AudioClip unlockSound;
    public AudioClip normalOpenSound;
    public AudioClip closeSound;
    public AudioClip smashOpenSound;
    public AudioClip lockedErrorSound;

    [Header("Events")]
    public UnityEvent OnDoorUnlocked;
    public UnityEvent OnDoorOpened;
    public UnityEvent OnDoorClosed;
    public UnityEvent OnDoorSmashedOpen; // Useful for alerting nearby AI stalkers

    // Internal Dragging & Click Tracking Variables
    private bool isDragging = false;
    private bool isAutoAnimating = false;
    private float mouseDownTime = 0f;
    private float currentNormalizedProgress = 0f; // 0.0 = Closed, 1.0 = Fully Open
    private Vector3 previousMousePosition;

    private void Awake()
    {
        if (doorPanelTransform == null)
        {
            doorPanelTransform = transform;
        }

        currentNormalizedProgress = isOpen ? 1f : 0f;
        ApplyDoorProgress(currentNormalizedProgress);
    }

    private void OnMouseDown()
    {
        if (isAutoAnimating) return;

        if (isLocked)
        {
            PlaySound(lockedErrorSound);
            Debug.Log($"[InteractiveDoor] {gameObject.name} is LOCKED. Hack required ({lockType}).");
            return;
        }

        mouseDownTime = Time.time;
        isDragging = true;
        previousMousePosition = Input.mousePosition;
    }

    private void OnMouseDrag()
    {
        if (!isDragging || isLocked || isAutoAnimating) return;

        // Start slow dragging once held longer than the tap threshold
        if (Time.time - mouseDownTime > clickHoldThreshold)
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 mouseDelta = currentMousePos - previousMousePosition;

            float deltaInput = mouseDelta.x / Screen.width;
            float progressDelta = deltaInput * slowPeekDragSensitivity * Time.deltaTime;

            currentNormalizedProgress = Mathf.Clamp01(currentNormalizedProgress + progressDelta);
            ApplyDoorProgress(currentNormalizedProgress);

            previousMousePosition = currentMousePos;
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging || isLocked || isAutoAnimating) return;
        isDragging = false;

        float holdDuration = Time.time - mouseDownTime;

        // Check if player is sprinting into/towards the door
        PlayerController playerMovement = FindAnyObjectByType<PlayerController>();
        bool isSprinting = playerMovement != null && playerMovement.IsSprinting && playerMovement.IsMoving;

        if (isSprinting)
        {
            // 1. SPRINT SMASH OPEN
            SmashOpen();
        }
        else if (holdDuration <= clickHoldThreshold)
        {
            // 2. NORMAL CLICK (TAP) OPEN/CLOSE
            ToggleDoorNormal();
        }
        else
        {
            // 3. SLOW DRAG PEEK RELEASE (Snap to nearest state)
            if (currentNormalizedProgress >= 0.5f)
            {
                StartCoroutine(SmoothSnapDoorRoutine(1f, true, normalOpenSpeed, normalOpenSound));
            }
            else
            {
                StartCoroutine(SmoothSnapDoorRoutine(0f, false, normalOpenSpeed, closeSound));
            }
        }
    }

    /// <summary>
    /// Opens or closes the door at standard speed.
    /// </summary>
    public void ToggleDoorNormal()
    {
        if (isAutoAnimating) return;

        bool targetState = !isOpen;
        AudioClip clipToPlay = targetState ? normalOpenSound : closeSound;
        StartCoroutine(SmoothSnapDoorRoutine(targetState ? 1f : 0f, targetState, normalOpenSpeed, clipToPlay));
    }

    /// <summary>
    /// Violently smashes the door open when sprinting.
    /// </summary>
    public void SmashOpen()
    {
        if (isLocked)
        {
            PlaySound(lockedErrorSound);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(SmoothSnapDoorRoutine(1f, true, smashOpenSpeed, smashOpenSound));
        OnDoorSmashedOpen?.Invoke();
        Debug.Log($"[InteractiveDoor] {gameObject.name} WAS SMASHED OPEN!");
    }

    /// <summary>
    /// Called by Smartphone Hack App upon a successful hack solve.
    /// </summary>
    public void UnlockViaSmartphoneHack()
    {
        if (!isLocked && isOpen) return;

        isLocked = false;
        PlaySound(unlockSound);
        OnDoorUnlocked?.Invoke();
        Debug.Log($"[InteractiveDoor] {gameObject.name} UNLOCKED via Smartphone Hack App!");

        StartCoroutine(SmoothSnapDoorRoutine(1f, true, normalOpenSpeed, normalOpenSound));
    }

    private IEnumerator SmoothSnapDoorRoutine(float targetProgress, bool targetOpenState, float speed, AudioClip soundToPlay)
    {
        isAutoAnimating = true;
        PlaySound(soundToPlay);

        while (Mathf.Abs(currentNormalizedProgress - targetProgress) > 0.01f)
        {
            currentNormalizedProgress = Mathf.MoveTowards(currentNormalizedProgress, targetProgress, Time.deltaTime * speed);
            ApplyDoorProgress(currentNormalizedProgress);
            yield return null;
        }

        currentNormalizedProgress = targetProgress;
        ApplyDoorProgress(currentNormalizedProgress);

        isOpen = targetOpenState;
        isAutoAnimating = false;

        if (isOpen) OnDoorOpened?.Invoke();
        else OnDoorClosed?.Invoke();
    }

    private void ApplyDoorProgress(float progress)
    {
        if (doorPanelTransform == null) return;

        if (motionType == MotionType.HingeRotation)
        {
            float targetYAngle = Mathf.Lerp(closedAngle, openAngle, progress);
            doorPanelTransform.localEulerAngles = new Vector3(
                doorPanelTransform.localEulerAngles.x,
                targetYAngle,
                doorPanelTransform.localEulerAngles.z
            );
        }
        else if (motionType == MotionType.SlidingTranslation)
        {
            Vector3 targetPos = Vector3.Lerp(closedLocalPos, openLocalPos, progress);
            doorPanelTransform.localPosition = targetPos;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Trigger door smash if sprinting directly into the door collision
    private void OnCollisionEnter(Collision collision)
    {
        if (isLocked || isOpen || isAutoAnimating) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null && player.IsSprinting && player.IsMoving)
        {
            SmashOpen();
        }
    }
}
