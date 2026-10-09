using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProximityUITrigger : MonoBehaviour
{
    public GameObject uiSliderPanel;
    public string playerTag = "Player";

    [Tooltip("Optional – if assigned, uses animated Show/Hide instead of SetActive.")]
    public VoltageSliderUI voltageSliderUI;

    private void Awake()
    {
        // Force the UI to hide immediately when the scene loads
        if (uiSliderPanel != null)
        {
            uiSliderPanel.SetActive(false);
            Debug.Log("Start: Slider hidden.");
        }
        else
        {
            Debug.LogError("Error: You forgot to drag the UI Panel into the script!");
        }
    }

    

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Something entered the zone: {other.gameObject.name} with Tag: {other.tag}");

        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player recognized! Showing UI.");

            if (voltageSliderUI != null)
                voltageSliderUI.Show();
            else if (uiSliderPanel != null)
                uiSliderPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PlayerMovement_QSDZ.FreezeLook = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Something left the zone: {other.gameObject.name}");

        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player left! Hiding UI.");

            if (voltageSliderUI != null)
                voltageSliderUI.Hide();
            else if (uiSliderPanel != null)
                uiSliderPanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            PlayerMovement_QSDZ.FreezeLook = false;
        }
    }
}