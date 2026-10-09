using UnityEngine;

public class CarExit : MonoBehaviour
{
    public Transform exitPoint;
    public GameObject player;
    public GameObject exitButtonObject;

    public Projectcar projectcarScript;
    public CharacterController characterController;

    [Header("Buttons")]
    public GameObject enterCarButton;      // bouton Remonter

    public GameObject driveButtonsUI;      // parent optionnel des boutons conduite

    public GameObject forwardButton;
    public GameObject backButton;
    public GameObject leftButton;
    public GameObject rightButton;
    public GameObject stopButton;

    [Header("Audio")]
    public AudioSource exitAudio;
    private bool exitAudioPlayed = false;

    [Header("Ground Check")]
    public LayerMask groundMask = ~0;
    public float raycastHeight = 2f;
    public float raycastDistance = 10f;
    public float groundOffset = 0.1f;

    public void ExitCar()
    {
        if (player == null || exitPoint == null) return;

        // Audio seulement la première fois
        if (!exitAudioPlayed && exitAudio != null)
        {
            exitAudio.Play();
            exitAudioPlayed = true;
        }

        // Sortir le joueur de la voiture
        player.transform.SetParent(null);
        player.transform.localScale = Vector3.one;

        if (characterController != null)
            characterController.enabled = false;

        Vector3 targetPosition = exitPoint.position;
        Quaternion targetRotation = Quaternion.Euler(0f, exitPoint.eulerAngles.y, 0f);

        Vector3 rayOrigin = exitPoint.position + Vector3.up * raycastHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            targetPosition = hit.point + Vector3.up * groundOffset;
        }

        player.transform.position = targetPosition;
        player.transform.rotation = targetRotation;

        if (characterController != null)
        {
            characterController.stepOffset = 0.3f;
            characterController.enabled = true;
        }

        if (projectcarScript != null)
        {
            projectcarScript.enabled = true;
            projectcarScript.allowMovement = true;
            projectcarScript.allowLook = true;
            projectcarScript.CloseUI();
        }

        // Cacher bouton Exit
        if (exitButtonObject != null)
            exitButtonObject.SetActive(false);

        // Cacher le groupe complet des boutons conduite
        if (driveButtonsUI != null)
            driveButtonsUI.SetActive(false);

        // Cacher aussi chaque bouton individuellement
        if (forwardButton != null)
            forwardButton.SetActive(false);

        if (backButton != null)
            backButton.SetActive(false);

        if (leftButton != null)
            leftButton.SetActive(false);

        if (rightButton != null)
            rightButton.SetActive(false);

        if (stopButton != null)
            stopButton.SetActive(false);

        // Afficher toujours Remonter après descente
        if (enterCarButton != null)
            enterCarButton.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}