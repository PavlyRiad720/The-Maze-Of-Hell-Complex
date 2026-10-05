using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthAndStamina : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float severeInjuryThreshold = 25f;   // Critical health threshold for limping
    public bool isSeverlyInjured { get; private set;  }

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 20f;
    public float staminaRagenRate = 15f;
    public bool isExhausted { get; private set; }

    [Header("UI and Vignette Overlays")]
    public Image bloodOverlayImage; // UI Image anchored to Full Screen
    public CanvasGroup fatigueBlueGroup; // CanvasGroup containing a Blue UI Material/Image

    [Header("Audio")]
    public AudioSource breathingAudioSource;
    public AudioClip HeavyBreathingClip;
    public AudioClip heartBeatSound;

    private PlayerController movementController;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        movementController = GetComponent<PlayerController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleStamina();
        CheckInjuryState();
        UpdateUIOvelays();
    }

    private void CheckInjuryState()
    {
        // Player is severly injured when health falls below threshold
        isSeverlyInjured = (currentHealth <= severeInjuryThreshold && currentHealth > 0f);
    }

    private void HandleStamina()
    {
        if (movementController.IsSprinting && movementController.IsMoving)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;
            }
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRagenRate * Time.deltaTime;
                if (currentStamina >= 25f) // Threshold to recover from total exhuastion
                {
                    isExhausted = false;
                }
            }
        }
    }

    private void UpdateUIOvelays()
    {
        // 1. Damage Blood Overlay (Alpha scales inversely with Health)
        if (bloodOverlayImage != null)
        {
            float healthPercent = currentHealth / maxHealth;
            float alpha = Mathf.Clamp01(1f - healthPercent);
            Color c = bloodOverlayImage.color;
            c.a = alpha;
            bloodOverlayImage.color = c;
        }

        // 2. Low Stamina Blur and Fatigue Effect
        if (fatigueBlueGroup != null)
        {
            float staminaPercent = currentStamina / maxStamina;
            float targetBlueAlpha = (staminaPercent < 0.3f) ? Mathf.InverseLerp(0.3f, 0f, staminaPercent) : 0f;
            fatigueBlueGroup.alpha = Mathf.Lerp(fatigueBlueGroup.alpha, targetBlueAlpha, Time.deltaTime * 4f);
        }

        // 3. Dynamic Breathing Audio Intensity
        if (breathingAudioSource != null && HeavyBreathingClip != null)
        {
            if (!breathingAudioSource.isPlaying)
            {
                breathingAudioSource.Play();

                float targetVolume = isExhausted ? 0.9f : Mathf.Lerp(0.1f, 0.6f, 1f - (currentStamina / maxStamina));
                breathingAudioSource.volume = Mathf.Lerp(breathingAudioSource.volume, targetVolume, Time.deltaTime * 3f);
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <=  0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    // --- Medkit Healing System ---
    public bool UseMedkit (float healAmount)
    {
        if (currentHealth >= maxHealth)
        {
            return false;
        }

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        return true;
    }

    private void Die()
    {
        // Turn screen completely red on death
        if (bloodOverlayImage != null)
        {
            bloodOverlayImage.color = new Color(1f, 0f, 0f, 1f);
        }

        movementController.enabled = false;
    }
}
