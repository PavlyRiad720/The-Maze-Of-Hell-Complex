using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 4.0f;
    public float runSpeed = 7.0f;
    public float crouchSpeed = 2.0f;
    public float exhaustedSpeed = 2.5f;
    public float severelyInjuredSpeed = 1.5f;
    public float jumpForce = 5.0f;

    [Header("Look Behind System")]
    public Transform cameraHolder;
    public KeyCode lookBehindKey = KeyCode.LeftAlt;
    public float lookBehindRotationSpeed = 12f;
    public bool IsLookingBehind { get; private set; }

    [Header("Stealth and Leaning")]
    public float leanAngle = 15f;
    public float leanOffset = 0.5f;
    public float leanSpeed = 6f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1.0f;
    private float standingHeight;
    private CapsuleCollider capsule;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    // Public States
    public bool IsMoving { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsGrounded { get; private set; }

    private Rigidbody rb;
    private PlayerHealthAndStamina staminaSystem;
    private float currentLean = 0f;
    private float targetLean = 0f;
    private float currentLeanOffset = 0f;

    // Input Caching for Physics
    private float inputX;
    private float inputZ;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        staminaSystem = GetComponent<PlayerHealthAndStamina>();

        rb.freezeRotation = true;
        standingHeight = capsule.height;
    }

    private void Update()
    {
        CheckGround();

        // Cache Movement Inputs
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump") && IsGrounded && !IsCrouching)
        {
            jumpRequested = true;
        }

        HandleLookBehind();
        HandleCrouch();
        HandleLeaning();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void CheckGround()
    {
        if (groundCheck != null)
        {
            IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            // Fallback ground check if groundCheck transform isn't assigned
            IsGrounded = Physics.Raycast(transform.position, Vector3.down, (standingHeight / 2f) + 0.1f, groundMask);
        }
    }

    private void MovePlayer()
    {
        Vector3 moveDir = (transform.right * inputX + transform.forward * inputZ).normalized;
        IsMoving = moveDir.magnitude > 0.1f;

        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);
        bool isExhausted = staminaSystem != null && staminaSystem.isExhausted;

        IsSprinting = wantsToSprint && IsMoving && !IsCrouching && !isExhausted;

        // Speed Selection Matrix
        float targetSpeed = walkSpeed;
        if (isExhausted) targetSpeed = exhaustedSpeed;
        else if (IsCrouching) targetSpeed = crouchSpeed;
        else if (IsSprinting) targetSpeed = runSpeed;

        // Apply Rigidbody Horizontal Velocity
        Vector3 targetVelocity = moveDir * targetSpeed;
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - new Vector3(currentVelocity.x, 0, currentVelocity.z);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Execute Jump Impulse
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    private void HandleLookBehind()
    {
        if (cameraHolder == null) return;

        IsLookingBehind = Input.GetKey(lookBehindKey);
        float targetYAngle = IsLookingBehind ? 180f : 0f;

        // Preserve current pitch (X axis) applied by PlayerCameraController
        float currentPitch = cameraHolder.localEulerAngles.x;

        Quaternion targetRotation = Quaternion.Euler(currentPitch, targetYAngle, currentLean);
        cameraHolder.localRotation = Quaternion.Slerp(cameraHolder.localRotation, targetRotation, Time.deltaTime * lookBehindRotationSpeed);
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
        {
            IsCrouching = !IsCrouching;
            capsule.height = IsCrouching ? crouchHeight : standingHeight;
        }
    }

    private void HandleLeaning()
    {
        if (cameraHolder == null) return;

        if (Input.GetKey(KeyCode.Q))
        {
            targetLean = leanAngle;
            currentLeanOffset = -leanOffset;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            targetLean = -leanAngle;
            currentLeanOffset = leanOffset;
        }
        else
        {
            targetLean = 0f;
            currentLeanOffset = 0f;
        }

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);
        float localX = Mathf.Lerp(cameraHolder.localPosition.x, currentLeanOffset, Time.deltaTime * leanSpeed);

        cameraHolder.localPosition = new Vector3(localX, cameraHolder.localPosition.y, cameraHolder.localPosition.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
