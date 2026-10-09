using UnityEngine;

public class QuickGrab : MonoBehaviour
{
    Camera cam;
    bool held = false;
    float distToCamera;
    Vector3 offset;

    void Start()
    {
        cam = Camera.main;

        // Auto-add a collider if the pipe doesn't have one
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
            Debug.LogWarning(gameObject.name + ": No collider found, BoxCollider added automatically.");
        }
    }

    void OnMouseDown()
    {
        held = true;
        distToCamera = Vector3.Distance(transform.position, cam.transform.position);
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseUp()
    {
        held = false;
    }

    void Update()
    {
        if (held)
            transform.position = GetMouseWorldPos() + offset;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = distToCamera;
        return cam.ScreenToWorldPoint(mouseScreen);
    }
}