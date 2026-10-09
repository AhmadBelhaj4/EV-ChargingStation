using UnityEngine;

public class MouseGrabber : MonoBehaviour
{
    public LayerMask grabbableLayer;
    public float moveSpeed = 15f;
    public float groundedY = 0f;       // ← set this to where the can's pivot sits on the floor

    private Camera cam;
    private Rigidbody grabbedRb;
    private float grabDistance;
    private Vector3 grabOffset;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryGrab();

        if (Input.GetMouseButtonUp(0))
            Release();
    }

    void FixedUpdate()
    {
        if (grabbedRb != null)
            MoveGrabbedObject();
    }

    void TryGrab()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, grabbableLayer))
        {
            grabbedRb = hit.rigidbody;
            if (grabbedRb != null)
            {
                grabDistance = Vector3.Distance(cam.transform.position, hit.point);
                grabOffset = grabbedRb.position - hit.point;
                grabOffset.y = 0f;         // don't carry vertical offset

                grabbedRb.angularVelocity = Vector3.zero;
                grabbedRb.useGravity = false;  // disable gravity while dragging
            }
        }
    }

    void MoveGrabbedObject()
    {
        // Cast a horizontal plane at ground level
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, groundedY, 0f));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance) + grabOffset;

            // Force Y to stay on the ground — no lifting ever
            targetPoint.y = groundedY;

            Vector3 direction = targetPoint - grabbedRb.position;
            grabbedRb.linearVelocity = new Vector3(
                direction.x * moveSpeed,
                0f,                        // ← Y velocity always zero
                direction.z * moveSpeed
            );
        }
    }

    void Release()
    {
        if (grabbedRb != null)
        {
            if (!grabbedRb.isKinematic)
            {
                grabbedRb.linearVelocity = Vector3.zero;
                grabbedRb.useGravity = true;   // restore gravity on release
            }
            grabbedRb = null;
        }
    }
}