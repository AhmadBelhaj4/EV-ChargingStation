using UnityEngine;

public class PlusGrag : MonoBehaviour
{
    private Camera cam;
    private bool isDragging;
    private float distance;

    public Transform cableStart;
    public float maxDistance = 2f;

    void Start()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        distance = Vector3.Distance(transform.position, cam.transform.position);
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (!isDragging || cableStart == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = distance;
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

        if (Vector3.Distance(worldPos, cableStart.position) <= maxDistance)
        {
            transform.position = worldPos;
        }
    }
}
