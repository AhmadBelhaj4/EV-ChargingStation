using UnityEngine;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    public float interactRange = 2.5f;
    public TextMeshProUGUI hintText; // drag your hint UI here

    private IInteractable currentTarget;

    void Update()
    {
        // Raycast from center of screen forward
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width/2, Screen.height/2));

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            IInteractable interactable =
                hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                currentTarget = interactable;
                hintText.text = "[E] " + interactable.GetHintText();
                hintText.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                    interactable.Interact();

                return;
            }
        }

        currentTarget = null;
        hintText.gameObject.SetActive(false);
    }
}