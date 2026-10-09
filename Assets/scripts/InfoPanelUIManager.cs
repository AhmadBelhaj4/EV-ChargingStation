using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the information panel UI for the solar charging station digital twin.
/// Handles fade animations, positioning, and text updates.
/// 
/// Setup:
/// 1. Create a Canvas in your scene
/// 2. Create a Panel child with Image component and Canvas Group
/// 3. Add TextMeshPro for title and description
/// 4. Assign this script to the Canvas or a manager GameObject
/// </summary>
public class InfoPanelUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RectTransform panelRectTransform;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float scaleAnimationDuration = 0.4f;
    [SerializeField] private Vector3 minScale = new Vector3(0.9f, 0.9f, 1f);
    [SerializeField] private Vector3 maxScale = Vector3.one;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Panel Settings")]
    [SerializeField] private Vector2 panelOffset = new Vector2(20, -20);
    [SerializeField] private Canvas parentCanvas;

    // State management
    private Coroutine fadeCoroutine;
    private Coroutine scaleCoroutine;
    private bool isPanelVisible = false;

    private void OnEnable()
    {
        // Initialize panel as hidden
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Shows the info panel with the provided title and description.
    /// Includes fade-in and scale animation effects.
    /// </summary>
    public void ShowPanel(string title, string description)
    {
        // Stop any ongoing animations
        StopAllAnimations();

        // Update text content
        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        isPanelVisible = true;

        // Start fade and scale animations
        fadeCoroutine = StartCoroutine(FadeIn());
        scaleCoroutine = StartCoroutine(ScaleIn());
    }

    /// <summary>
    /// Hides the info panel with fade-out and scale animations.
    /// </summary>
    public void HidePanel()
    {
        if (!isPanelVisible)
            return;

        // Stop any ongoing animations
        StopAllAnimations();

        isPanelVisible = false;

        // Start fade and scale animations
        fadeCoroutine = StartCoroutine(FadeOut());
        scaleCoroutine = StartCoroutine(ScaleOut());
    }

    /// <summary>
    /// Toggles the panel visibility (show if hidden, hide if shown).
    /// </summary>
    public void TogglePanel()
    {
        if (isPanelVisible)
            HidePanel();
        else
            ShowPanel("Toggle Info", "Panel toggled via keyboard input");
    }

    /// <summary>
    /// Stops all active animations to prevent conflicts.
    /// </summary>
    private void StopAllAnimations()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
    }

    /// <summary>
    /// Coroutine for fade-in animation.
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeDuration);
            panelCanvasGroup.alpha = fadeInCurve.Evaluate(normalizedTime);
            yield return null;
        }

        panelCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Coroutine for fade-out animation.
    /// </summary>
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeDuration);
            panelCanvasGroup.alpha = fadeOutCurve.Evaluate(normalizedTime);
            yield return null;
        }

        panelCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Coroutine for scale-in animation (entrance effect).
    /// </summary>
    private IEnumerator ScaleIn()
    {
        float elapsedTime = 0f;
        panelRectTransform.localScale = minScale;

        while (elapsedTime < scaleAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / scaleAnimationDuration);
            panelRectTransform.localScale = Vector3.Lerp(minScale, maxScale, scaleCurve.Evaluate(normalizedTime));
            yield return null;
        }

        panelRectTransform.localScale = maxScale;
    }

    /// <summary>
    /// Coroutine for scale-out animation (exit effect).
    /// </summary>
    private IEnumerator ScaleOut()
    {
        float elapsedTime = 0f;
        Vector3 startScale = panelRectTransform.localScale;

        while (elapsedTime < scaleAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / scaleAnimationDuration);
            panelRectTransform.localScale = Vector3.Lerp(startScale, minScale, scaleCurve.Evaluate(normalizedTime));
            yield return null;
        }

        panelRectTransform.localScale = minScale;
    }

    /// <summary>
    /// Gets the current visibility state of the panel.
    /// </summary>
    public bool IsPanelVisible => isPanelVisible;
}
