using UnityEngine;
using System.Collections;

/// <summary>
/// Handles interaction detection for solar charging station elements.
/// Detects when the player enters/exits the trigger zone and communicates with the InfoPanelUIManager.
/// 
/// Setup:
/// 1. Add this script to battery, transformer, solar panel, or grid objects
/// 2. Add a Sphere Collider with "Is Trigger" enabled
/// 3. Adjust the collider radius for detection distance
/// 4. Set Title and Description in the Inspector
/// 5. Assign the InfoPanelUIManager reference
/// </summary>
public class InteractiveElement : MonoBehaviour
{
    [Header("Element Information")]
    [SerializeField] private string elementTitle = "Solar Panel";
    [SerializeField] private string elementDescription = "This is a solar panel that converts sunlight into electrical energy.";

    [Header("References")]
    [SerializeField] private InfoPanelUIManager uiManager;
    [SerializeField] private Collider triggerCollider;

    [Header("Visual Feedback")]
    [SerializeField] private bool enableOutlineHighlight = true;
    [SerializeField] private Renderer elementRenderer;
    [SerializeField] private Color highlightColor = new Color(0.2f, 0.7f, 1f, 0.3f);
    [SerializeField] private Material highlightMaterial;

    [Header("Interaction")]
    [SerializeField] private KeyCode toggleKey = KeyCode.E;

    // State management
    private bool isPlayerInTrigger = false;
    private Material originalMaterial;
    private bool isDescriptionVisible = true;
    private Coroutine highlightFadeCoroutine;

    private void OnValidate()
    {
        // Auto-find trigger collider if not assigned
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();

        // Auto-find renderer if not assigned
        if (elementRenderer == null)
            elementRenderer = GetComponent<Renderer>();

        // Ensure collider is set as trigger
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        // Verify references
        if (uiManager == null)
        {
            Debug.LogError($"InfoPanelUIManager not assigned to {gameObject.name}. Please assign it in the Inspector.");
            enabled = false;
            return;
        }

        // Store original material for outline effect
        if (elementRenderer != null && enableOutlineHighlight)
        {
            originalMaterial = elementRenderer.material;
        }
    }

    private void Update()
    {
        // Check for toggle input only when player is in trigger zone
        if (isPlayerInTrigger && Input.GetKeyDown(toggleKey))
        {
            ToggleDescription();
        }
    }

    /// <summary>
    /// Called when an object enters the trigger collider (player).
    /// Shows the info panel and applies visual highlight.
    /// </summary>
    private void OnTriggerEnter(Collider collision)
    {
        // Check if the entering object is the player
        // You can customize this check based on your player setup
        if (collision.CompareTag("Player") || collision.name.Contains("Player"))
        {
            isPlayerInTrigger = true;
            isDescriptionVisible = true;

            // Show the info panel with this element's information
            uiManager.ShowPanel(elementTitle, elementDescription);

            // Apply visual highlight
            if (enableOutlineHighlight)
            {
                ApplyHighlight();
            }

            Debug.Log($"Player entered trigger for: {elementTitle}");
        }
    }

    /// <summary>
    /// Called when an object exits the trigger collider (player).
    /// Hides the info panel and removes visual highlight.
    /// </summary>
    private void OnTriggerExit(Collider collision)
    {
        // Check if the exiting object is the player
        if (collision.CompareTag("Player") || collision.name.Contains("Player"))
        {
            isPlayerInTrigger = false;

            // Hide the info panel
            uiManager.HidePanel();

            // Remove visual highlight
            if (enableOutlineHighlight)
            {
                RemoveHighlight();
            }

            Debug.Log($"Player exited trigger for: {elementTitle}");
        }
    }

    /// <summary>
    /// Toggles the description visibility with the E key.
    /// Shows/hides the description text in the panel.
    /// </summary>
    private void ToggleDescription()
    {
        isDescriptionVisible = !isDescriptionVisible;

        if (isDescriptionVisible)
        {
            // Show full description
            uiManager.ShowPanel(elementTitle, elementDescription);
            Debug.Log($"Description shown for: {elementTitle}");
        }
        else
        {
            // Show only title
            uiManager.ShowPanel(elementTitle, "Press E to view details");
            Debug.Log($"Description hidden for: {elementTitle}");
        }
    }

    /// <summary>
    /// Applies a visual highlight effect to the element.
    /// Uses either a highlight material or modifies the existing material's color.
    /// </summary>
    private void ApplyHighlight()
    {
        if (elementRenderer == null)
            return;

        // Stop any ongoing fade coroutine
        if (highlightFadeCoroutine != null)
        {
            StopCoroutine(highlightFadeCoroutine);
        }

        if (highlightMaterial != null)
        {
            // Use dedicated highlight material if provided
            elementRenderer.material = highlightMaterial;
        }
        else
        {
            // Apply color tint to existing material
            Material tempMaterial = new Material(elementRenderer.material);
            tempMaterial.color = new Color(
                tempMaterial.color.r + highlightColor.r,
                tempMaterial.color.g + highlightColor.g,
                tempMaterial.color.b + highlightColor.b,
                tempMaterial.color.a
            );
            elementRenderer.material = tempMaterial;
        }
    }

    /// <summary>
    /// Removes the visual highlight effect from the element.
    /// </summary>
    private void RemoveHighlight()
    {
        if (elementRenderer == null)
            return;

        // Fade out the highlight effect
        highlightFadeCoroutine = StartCoroutine(FadeOutHighlight());
    }

    /// <summary>
    /// Coroutine to smoothly fade out the highlight effect.
    /// </summary>
    private IEnumerator FadeOutHighlight()
    {
        float fadeDuration = 0.2f;
        float elapsedTime = 0f;
        Material currentMaterial = elementRenderer.material;
        Color startColor = currentMaterial.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            // Fade back to original color
            Color newColor = Color.Lerp(startColor, originalMaterial.color, t);
            currentMaterial.color = newColor;

            yield return null;
        }

        // Restore original material
        if (originalMaterial != null)
        {
            elementRenderer.material = originalMaterial;
        }
    }

    /// <summary>
    /// Gets the title of this element.
    /// </summary>
    public string GetElementTitle() => elementTitle;

    /// <summary>
    /// Gets the description of this element.
    /// </summary>
    public string GetElementDescription() => elementDescription;

    /// <summary>
    /// Checks if the player is currently in the trigger zone.
    /// </summary>
    public bool IsPlayerInTrigger => isPlayerInTrigger;
}
