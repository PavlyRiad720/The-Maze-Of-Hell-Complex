using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerHeadBobAndBreathing : MonoBehaviour
{
    [Header("Breathing Settings")]
    public float idleBreathSpeed = 2f;
    public float idleBreathAmount = 0.02f;

    [Header("Head Bob Settings")]
    public float walkBobSpeed = 12f;
    public float walkBobAmount = 0.05f;
    public float runBobSpeed = 18f;
    public float runBobAmount = 0.1f;
    public float crouchBobSpeed = 8f;
    public float crouchBobAmount = 0.025f;

    private float timer = 0f;
    private Vector3 initialLocalPosition;
    private PlayerController playerController;
    private PlayerHealthAndStamina staminaSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialLocalPosition = transform.localPosition;
        playerController = GetComponentInParent<PlayerController>();
        staminaSystem = GetComponentInParent<PlayerHealthAndStamina>();
    }

    // Update is called once per frame
    void Update()
    {
        ApplyHeadBobAndBreathing();
    }

    private void ApplyHeadBobAndBreathing()
    {
        if (playerController == null) return;

        float speed = 0f;
        float amount = 0f;

        // Determine motion intensity
        if (!playerController.IsGrounded)
        {
            // freeze bobbing mid air
            speed = 0f;
            amount = 0f;
        }
        else if (playerController.IsMoving)
        {
            if (playerController.IsCrouching)
            {
                speed = crouchBobSpeed;
                amount = crouchBobAmount;
            }
            else if (playerController.IsSprinting)
            {
                speed = runBobSpeed;
                amount = runBobAmount;
            }
            else
            {
                speed = walkBobSpeed;
                amount = walkBobAmount;
            }
        }
        else
        {
            // Idle Breathing Logic (Faster/Heavy when stamina is low
            float staminaRatio = (staminaSystem != null) ? (staminaSystem.currentStamina / staminaSystem.maxStamina) : 1f;
            speed = Mathf.Lerp(idleBreathSpeed * 2.5f, idleBreathSpeed, staminaRatio);
            amount = Mathf.Lerp(idleBreathAmount * 2f, idleBreathAmount, staminaRatio);
        }

        if (playerController.IsMoving && playerController.IsGrounded)
        {
            timer += Time.deltaTime * speed;
            float newY = initialLocalPosition.y + Mathf.Sin(timer) * amount;
            float newX = initialLocalPosition.x + Mathf.Cos(timer * 0.5f) * amount;
            transform.localPosition = new Vector3(newX, newY, initialLocalPosition.z);
        }
        else
        {
            // Idle breathing Cycle
            timer += Time.deltaTime * speed;
            float newY = initialLocalPosition.y + Mathf.Sin(timer) * amount;
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(initialLocalPosition.x, newY, initialLocalPosition.z), Time.deltaTime * 4f);
        }
    }
}
