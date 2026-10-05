using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaAndHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float severeInjuryThreshold = 25f; // Critical health threshold for limping
    public bool isSeverelyInjured { get; private set; }

    [Header("Blood Vignette UI Overlay")]
    public Image bloodOverlayImage;
    public float bloodFadeSpeed = 3f; // Speed at which blood overlay fades in or out

    [Header("Hit Impact Flash Effect")]
    [Tooltip("Extra flash intensity added to overlay when taking damage.")]
    public float impactFlashIntensity = 0.5f;
    [Tooltip("How fast the hit flash decays down to the baseline health vignette.")]
    public float impactDecaySpeed = 4f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 15f;
    public bool isExhausted { get; private set; }

    [Header("Fatigue UI Overlay")]
    public CanvasGroup fatigueBlurGroup;

    [Header("Audio")]
    public AudioSource breathingAudioSource;
    public AudioSource hurtAudioSource;
    public AudioClip heavyBreathingClip;
    public AudioClip heartBeatSound;
    public AudioClip[] hurtImpactClips;

    private PlayerController movementController;
    private float targetBloodAlpha = 0f;
    private float currentHitFlashAlpha = 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        movementController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        HandleStamina();
        CheckInjuryState();
        UpdateBloodOverlay();
        UpdateFatigueAndAudioOverlays();
    }

    private void CheckInjuryState()
    {
        // Player is severely injured when health falls below threshold
        isSeverelyInjured = (currentHealth <= severeInjuryThreshold && currentHealth > 0f);
    }

    // --- Combined Persistent + Hit Flash Overlay Logic ---
    private void UpdateBloodOverlay()
    {
        if (bloodOverlayImage == null) return;

        // 1. Calculate baseline target alpha based on missing health percentage
        float missingHealthRatio = 1f - Mathf.Clamp01(currentHealth / maxHealth);

        if (isSeverelyInjured)
        {
            targetBloodAlpha = Mathf.Max(missingHealthRatio, 0.6f);
        }
        else
        {
            targetBloodAlpha = missingHealthRatio;
        }

        // 2. Decay the hit impact flash down toward zero over time
        if (currentHitFlashAlpha > 0f)
        {
            currentHitFlashAlpha = Mathf.MoveTowards(currentHitFlashAlpha, 0f, Time.deltaTime * impactDecaySpeed);
        }

        // 3. Combine baseline health alpha with hit impact flash alpha (capped at 1.0)
        float totalTargetAlpha = Mathf.Clamp01(targetBloodAlpha + currentHitFlashAlpha);

        // 4. Smoothly lerp overlay color
        Color currentColor = bloodOverlayImage.color;
        float newAlpha = Mathf.Lerp(currentColor.a, totalTargetAlpha, Time.deltaTime * bloodFadeSpeed);

        bloodOverlayImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
    }

    // --- Damage Routine ---
    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= damageAmount;

        // Trigger immediate hit flash effect proportional to damage taken
        float damageRatio = Mathf.Clamp01(damageAmount / maxHealth);
        currentHitFlashAlpha = Mathf.Clamp01(currentHitFlashAlpha + impactFlashIntensity + (damageRatio * 0.5f));

        // Play hurt audio impact cue
        if (hurtAudioSource != null && hurtImpactClips != null && hurtImpactClips.Length > 0)
        {
            AudioClip clip = hurtImpactClips[Random.Range(0, hurtImpactClips.Length)];
            hurtAudioSource.PlayOneShot(clip);
        }

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    // --- Medkit Healing Routine ---
    public bool UseMedkit(float healAmount)
    {
        if (currentHealth >= maxHealth)
        {
            Debug.Log("Health is already full.");
            return false; // Medkit not consumed
        }

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log($"Medkit consumed! Restored {healAmount} HP. Current Health: {currentHealth}");

        // Check injury state immediately so movement penalties drop if healed above threshold
        CheckInjuryState();

        return true; // Medkit successfully consumed
    }

    private void HandleStamina()
    {
        if (movementController != null && movementController.IsSprinting && movementController.IsMoving)
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
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (currentStamina >= 25f)
                {
                    isExhausted = false;
                }
            }
        }
    }

    private void UpdateFatigueAndAudioOverlays()
    {
        // Low Stamina Blur
        if (fatigueBlurGroup != null)
        {
            float staminaPercent = currentStamina / maxStamina;
            float targetBlurAlpha = (staminaPercent < 0.3f) ? Mathf.InverseLerp(0.3f, 0f, staminaPercent) : 0f;
            fatigueBlurGroup.alpha = Mathf.Lerp(fatigueBlurGroup.alpha, targetBlurAlpha, Time.deltaTime * 4f);
        }

        // Breathing Cues
        if (breathingAudioSource != null)
        {
            if (!breathingAudioSource.isPlaying) breathingAudioSource.Play();

            float targetVolume = (isExhausted || isSeverelyInjured) ? 0.9f : Mathf.Lerp(0.1f, 0.6f, 1f - (currentStamina / maxStamina));
            breathingAudioSource.volume = Mathf.Lerp(breathingAudioSource.volume, targetVolume, Time.deltaTime * 3f);
        }
    }

    private void Die()
    {
        if (bloodOverlayImage != null)
        {
            bloodOverlayImage.color = new Color(1f, 0f, 0f, 1f);
        }

        if (movementController != null)
        {
            movementController.enabled = false;
        }

        Debug.Log("Player has died.");
    }
}