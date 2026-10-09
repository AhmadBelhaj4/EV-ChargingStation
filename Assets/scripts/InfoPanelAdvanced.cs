using UnityEngine;
using TMPro;

/// <summary>
/// Optional advanced features for the info panel system.
/// Includes particle effects, real-time data binding, and enhanced visuals.
/// 
/// This script is OPTIONAL and extends the base system.
/// Only use features you need for your project.
/// </summary>
public class InfoPanelAdvanced : MonoBehaviour
{
    [Header("Particle Effects")]
    [SerializeField] private bool enableParticleEffects = false;
    [SerializeField] private ParticleSystem appearanceParticles;
    [SerializeField] private ParticleSystem disappearanceParticles;

    [Header("Sound Effects")]
    [SerializeField] private bool enableSoundEffects = false;
    [SerializeField] private AudioClip panelAppearSound;
    [SerializeField] private AudioClip panelDisappearSound;
    [SerializeField] private float soundVolume = 0.3f;
    private AudioSource audioSource;

    [Header("Dynamic Data Display")]
    [SerializeField] private bool enableDynamicData = false;
    [SerializeField] private TextMeshProUGUI dataText;

    [Header("Visual Enhancement")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private RectTransform glowElement;
    [SerializeField] private float glowPulseSpeed = 2f;

    // References
    private InfoPanelUIManager uiManager;
    private float glowIntensity = 0.5f;

    private void Start()
    {
        uiManager = GetComponent<InfoPanelUIManager>();

        // Setup audio source if sound effects are enabled
        if (enableSoundEffects && audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.volume = soundVolume;
        }
    }

    private void Update()
    {
        // Update glow pulse effect
        if (enableParticleEffects && glowElement != null && uiManager.IsPanelVisible)
        {
            UpdateGlowPulse();
        }
    }

    /// <summary>
    /// Shows the panel with enhanced effects (particles, sound, glow).
    /// Call this instead of UIManager.ShowPanel for advanced effects.
    /// </summary>
    public void ShowPanelWithEffects(string title, string description)
    {
        // Show the base panel
        uiManager.ShowPanel(title, description);

        // Play appearance effects
        if (enableParticleEffects && appearanceParticles != null)
        {
            appearanceParticles.Play();
        }

        // Play sound effect
        if (enableSoundEffects && audioSource != null && panelAppearSound != null)
        {
            audioSource.PlayOneShot(panelAppearSound);
        }

        // Start glow effect
        if (glowElement != null)
        {
            glowIntensity = 0.5f;
        }
    }

    /// <summary>
    /// Hides the panel with enhanced effects (particles, sound).
    /// Call this instead of UIManager.HidePanel for advanced effects.
    /// </summary>
    public void HidePanelWithEffects()
    {
        // Hide the base panel
        uiManager.HidePanel();

        // Play disappearance effects
        if (enableParticleEffects && disappearanceParticles != null)
        {
            disappearanceParticles.Play();
        }

        // Play sound effect
        if (enableSoundEffects && audioSource != null && panelDisappearSound != null)
        {
            audioSource.PlayOneShot(panelDisappearSound);
        }
    }

    /// <summary>
    /// Updates the dynamic data display with real-time information.
    /// Example: solar panel power output, battery percentage, etc.
    /// </summary>
    public void UpdateDynamicData(string dataContent)
    {
        if (enableDynamicData && dataText != null)
        {
            dataText.text = dataContent;
        }
    }

    /// <summary>
    /// Creates a pulsing glow effect on the panel for visual attention.
    /// </summary>
    private void UpdateGlowPulse()
    {
        glowIntensity = 0.5f + Mathf.Sin(Time.time * glowPulseSpeed) * 0.3f;

        if (glowElement != null)
        {
            CanvasGroup glowCanvasGroup = glowElement.GetComponent<CanvasGroup>();
            if (glowCanvasGroup != null)
            {
                glowCanvasGroup.alpha = glowIntensity;
            }
        }
    }

    /// <summary>
    /// Adds a fade-in effect to image elements for smoother transitions.
    /// </summary>
    public void FadeInElement(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeElementCoroutine(canvasGroup, 0f, 1f, duration));
        }
    }

    /// <summary>
    /// Adds a fade-out effect to image elements for smoother transitions.
    /// </summary>
    public void FadeOutElement(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeElementCoroutine(canvasGroup, 1f, 0f, duration));
        }
    }

    /// <summary>
    /// Coroutine for fading UI elements.
    /// </summary>
    private System.Collections.IEnumerator FadeElementCoroutine(
        CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    /// <summary>
    /// Shakes the panel for emphasis (warning or important info).
    /// </summary>
    public void ShakePanelWarning(float duration = 0.2f, float intensity = 10f)
    {
        StartCoroutine(ShakePanelCoroutine(duration, intensity));
    }

    /// <summary>
    /// Coroutine for panel shake effect.
    /// </summary>
    private System.Collections.IEnumerator ShakePanelCoroutine(float duration, float intensity)
    {
        RectTransform panelRect = panelCanvasGroup.GetComponent<RectTransform>();
        Vector2 originalPosition = panelRect.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Random offset for shake effect
            float randomX = Random.Range(-intensity, intensity);
            float randomY = Random.Range(-intensity, intensity);

            panelRect.anchoredPosition = originalPosition + new Vector2(randomX, randomY);
            yield return null;
        }

        panelRect.anchoredPosition = originalPosition;
    }

    /// <summary>
    /// Creates a ping-pong color effect for highlighting important information.
    /// </summary>
    public void PulsePanelColor(float duration = 1f)
    {
        StartCoroutine(PulsePanelColorCoroutine(duration));
    }

    /// <summary>
    /// Coroutine for color pulse effect.
    /// </summary>
    private System.Collections.IEnumerator PulsePanelColorCoroutine(float duration)
    {
        CanvasGroup canvasGroup = panelCanvasGroup;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float pulse = Mathf.PingPong(elapsedTime * 2f, 1f);
            canvasGroup.alpha = Mathf.Lerp(0.8f, 1f, pulse);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Bounces the panel from the sides for attention-grabbing effect.
    /// </summary>
    public void BouncePanel(float duration = 0.3f)
    {
        StartCoroutine(BouncePanelCoroutine(duration));
    }

    /// <summary>
    /// Coroutine for bounce effect.
    /// </summary>
    private System.Collections.IEnumerator BouncePanelCoroutine(float duration)
    {
        RectTransform panelRect = panelCanvasGroup.GetComponent<RectTransform>();
        Vector2 originalPosition = panelRect.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Bouncing motion
            float bounceAmount = Mathf.Sin(t * Mathf.PI) * 20f;
            panelRect.anchoredPosition = originalPosition + Vector2.right * bounceAmount;

            yield return null;
        }

        panelRect.anchoredPosition = originalPosition;
    }
}

/// <summary>
/// Example usage in an InteractiveElement script:
/// 
/// private InfoPanelAdvanced advancedEffects;
/// 
/// void Start()
/// {
///     advancedEffects = uiManager.GetComponent<InfoPanelAdvanced>();
/// }
/// 
/// void OnTriggerEnter(Collider collision)
/// {
///     if (collision.CompareTag("Player"))
///     {
///         advancedEffects.ShowPanelWithEffects(elementTitle, elementDescription);
///     }
/// }
/// 
/// void OnTriggerExit(Collider collision)
/// {
///     if (collision.CompareTag("Player"))
///     {
///         advancedEffects.HidePanelWithEffects();
///     }
/// }
/// </summary>
