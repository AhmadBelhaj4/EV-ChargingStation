using UnityEngine;

public class CarEnter : MonoBehaviour
{
    public GameObject player;
    public Transform cameraSeat;
    public CharacterController characterController;
    public Projectcar projectcarScript;

    [Header("Buttons")]
    public GameObject enterCarButton;     // bouton Remonter
    public GameObject driveButtonsUI;     // parent des boutons conduite, si tu en as un

    public GameObject forwardButton;
    public GameObject backButton;
    public GameObject leftButton;
    public GameObject rightButton;
    public GameObject stopButton;

    [Header("Car")]
    public CarMovement carMovement;

    public void EnterCar()
    {
        if (player == null || cameraSeat == null) return;

        if (characterController != null)
            characterController.enabled = false;

        // Remettre le joueur dans la voiture
        player.transform.SetParent(cameraSeat);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
        Vector3 ws = cameraSeat.lossyScale;
        player.transform.localScale = new Vector3(ws.x != 0f ? 1f / ws.x : 1f, ws.y != 0f ? 1f / ws.y : 1f, ws.z != 0f ? 1f / ws.z : 1f);

        // Do NOT re-enable CharacterController while inside zero-scale car seat.
        // CarExit.ExitCar() will re-enable it when the player leaves.

        // Dans la voiture : regarder oui, marcher non
        if (projectcarScript != null)
        {
            projectcarScript.enabled = true;
            projectcarScript.allowMovement = false;
            projectcarScript.allowLook = true;
            projectcarScript.CloseUI();
        }

        // Cacher Remonter
        if (enterCarButton != null)
            enterCarButton.SetActive(false);

        // Afficher le groupe des boutons conduite
        if (driveButtonsUI != null)
            driveButtonsUI.SetActive(true);

        // Afficher les boutons un par un
        if (forwardButton != null)
            forwardButton.SetActive(true);

        if (backButton != null)
            backButton.SetActive(true);

        if (leftButton != null)
            leftButton.SetActive(true);

        if (rightButton != null)
            rightButton.SetActive(true);

        if (stopButton != null)
            stopButton.SetActive(true);

        // R�initialiser Start / Stop / Exit comme au d�but
        if (carMovement != null)
            carMovement.ResetCarButtons();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}