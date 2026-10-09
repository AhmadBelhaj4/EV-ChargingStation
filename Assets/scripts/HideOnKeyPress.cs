using UnityEngine;

public class HideOnKeyPress : MonoBehaviour
{
    public KeyCode key = KeyCode.H;

    private bool isVisible = true;
    private Renderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            isVisible = !isVisible;

            foreach (Renderer r in renderers)
            {
                r.enabled = isVisible;
            }
        }
    }
}