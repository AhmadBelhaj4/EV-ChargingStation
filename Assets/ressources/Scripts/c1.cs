using UnityEngine;

public class PlugDrag : MonoBehaviour
{
    private Camera cam;
    private bool dragging;
    private float distance;

    public Transform startPoint;
    public float maxDistance = 2f;

    void Start()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        distance = Vector3.Distance(transform.position, cam.transform.position);
        dragging = true;
    }

    void OnMouseUp()
    {
        dragging = false;
    }

    void Update()
    {
        if (dragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = distance;
            Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

            if (Vector3.Distance(worldPos, startPoint.position) < maxDistance)
            {
                transform.position = worldPos;
            }
        }
    }
}
