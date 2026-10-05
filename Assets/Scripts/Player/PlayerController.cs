using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent (typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 4.0f;
    public float runSpeed = 7.0f;
    public float crouchSpeed = 2.0f;
    public float exhaustedSpeed = 2.5f;
    public float severlyInjuredSpeed = 1.5f; // Slow Speed when crippled/injured
    public float jumpForce = 5.0f;

    [Header("Look Behind System")]
    public Transform cameraHolder;
    public KeyCode lookBehindKey = KeyCode.LeftAlt;
    public float lookBehindRotationSpeed = 12f;
    public bool IsLookingBehind { get; private set;  }

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
    public bool IsCrouching { get; private set;  }
    public bool IsGrounded { get; private set; }

    private Rigidbody rb;
    private PlayerHealthAndStamina staminaSystem;
    private float currentLean = 0f;
    private float targetLean = 0f;
    private float currentLeanOffset = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        staminaSystem = GetComponent<PlayerHealthAndStamina>();

        rb.freezeRotation = true;   // lock physics rotations
        standingHeight = capsule.height;
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
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
        IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    private void MovePlayer()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;
        IsMoving = moveDir.magnitude > 0.1f;

        // Determine Speed State
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);
        bool isExhausted = staminaSystem != null && staminaSystem.isExhausted;

        if (wantsToSprint && IsMoving && !IsCrouching && !isExhausted)
        {
            IsSprinting = true;
        }
        else
        {
            IsSprinting = false;
        }

        float targetSpeed = walkSpeed;
        if (isExhausted) targetSpeed = exhaustedSpeed;
        else if (IsCrouching) targetSpeed = runSpeed;
        else if (IsSprinting) targetSpeed = runSpeed;

        // Apply Rigidbody Velocity
        Vector3 targetVelocity = moveDir * targetSpeed;
        Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Jump Logic
        if (Input.GetButtonDown("Jump") && IsGrounded && !IsCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // --- Look Behind Mechanism ---
    private void HandleLookBehind()
    {
        IsLookingBehind = Input.GetKey(lookBehindKey);

        // taget 180 degree rotation on Y axis when looking back
        float targetYAngle = IsLookingBehind ? 180f : 0f;

        Quaternion targetRotation = Quaternion.Euler(0f, targetYAngle, currentLean);
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

        cameraHolder.localRotation = Quaternion.Euler(0, 0, currentLean);
        cameraHolder.localPosition = new Vector3(localX, cameraHolder.localPosition.y, cameraHolder.localPosition.z);
    }
}
