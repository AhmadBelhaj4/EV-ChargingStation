using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class ProximityUITrigger_button : MonoBehaviour
{
    [Header("UI")]
    public GameObject uiPanel;
    public Button removeLiquidButton;
    public Button addLiquidButton;

    [Header("Settings")]
    public string playerTag = "Player";

    [Header("References")]
    public PipeLiquidController pipeLiquidController;

    [Tooltip("Optional – if assigned, uses the enhanced animated UI instead of the raw panel.")]
    public LiquidButtonUI liquidButtonUI;

    private bool playerIsNear = false;

    private void Awake()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
        else
            Debug.LogError("ProximityUITrigger: uiPanel is not assigned!");

        if (removeLiquidButton != null)
            removeLiquidButton.onClick.AddListener(() =>
            {
                pipeLiquidController?.RemoveLiquid();
                UpdateButtons();
            });
        else
            Debug.LogError("ProximityUITrigger: removeLiquidButton is not assigned!");

        if (addLiquidButton != null)
            addLiquidButton.onClick.AddListener(() =>
            {
                pipeLiquidController?.AddLiquid();
                UpdateButtons();
            });
        else
            Debug.LogError("ProximityUITrigger: addLiquidButton is not assigned!");
    }

    private void Update()
    {
        // Keep checking fill every frame while player is near
        // so buttons update automatically when animation finishes
        if (playerIsNear)
            UpdateButtons();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsNear = true;

            if (liquidButtonUI != null)
                liquidButtonUI.Show();
            else if (uiPanel != null)
                uiPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PlayerMovement_QSDZ.FreezeLook = true;
            UpdateButtons();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsNear = false;

            if (liquidButtonUI != null)
                liquidButtonUI.Hide();
            else if (uiPanel != null)
                uiPanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            PlayerMovement_QSDZ.FreezeLook = false;
        }
    }

    void UpdateButtons()
    {
        if (pipeLiquidController == null) return;

        float fill = pipeLiquidController.GetCurrentFill();

        // Fill = 1 → pipes are full → only show Remove
        // Fill = 0 → pipes are empty → only show Add
        // In between → hide both (animation is running)
        bool isFull  = fill >= 0.99f;
        bool isEmpty = fill <= 0.01f;

        if (removeLiquidButton != null)
            removeLiquidButton.gameObject.SetActive(isFull);

        if (addLiquidButton != null)
            addLiquidButton.gameObject.SetActive(isEmpty);
    }
}