using System.Collections.Generic;
using UnityEngine;

public class GridSnapManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Transform tableTransform;  // Assign the welding table object
    public int columns = 10;
    public int rows = 10;
    public float cellSize = 0.5f;

    [Header("Snap Settings")]
    public float snapRange = 0.75f;

    [Header("Layer")]
    public LayerMask pipeLayer;
    public LayerMask tableSurfaceLayer; // Layer for the table surface (for raycasting onto it)

    Camera cam;
    Draggable heldPipe;
    Vector3 grabOffset;

    List<Vector3> gridPoints = new List<Vector3>();

    void Start()
    {
        cam = Camera.main;
        BuildGrid();
    }

    void BuildGrid()
    {
        gridPoints.Clear();

        if (tableTransform == null)
        {
            Debug.LogError("GridSnapManager: No table Transform assigned!");
            return;
        }

        // Grid is built in the table's local XZ plane, then converted to world space
        // so it works even if the table is rotated/moved
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Centre the grid on the table pivot
                float x = (col - (columns - 1) / 2f) * cellSize;
                float z = (row - (rows  - 1) / 2f) * cellSize;

                Vector3 localPoint = new Vector3(x, 0f, z);
                Vector3 worldPoint = tableTransform.TransformPoint(localPoint);
                gridPoints.Add(worldPoint);
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryGrab();

        if (Input.GetMouseButton(0) && heldPipe != null)
            DragPipe();

        if (Input.GetMouseButtonUp(0) && heldPipe != null)
            ReleasePipe();
    }

    void TryGrab()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, pipeLayer))
        {
            Draggable pipe = hit.collider.GetComponent<Draggable>();
            if (pipe == null) return;

            heldPipe = pipe;
            // Store the offset between the pipe's position and the hit point
            // so the pipe doesn't jump to the cursor origin
            grabOffset = heldPipe.transform.position - hit.point;
            heldPipe.OnPickUp();
        }
    }

    void DragPipe()
    {
        // Raycast onto the table surface to get a world position to drag along
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tableSurfaceLayer))
        {
            Vector3 targetPos = hit.point + grabOffset;

            // Keep the pipe's Y fixed to the table surface Y + its own height offset
            targetPos.y = heldPipe.heightAboveTable + tableTransform.position.y;

            Vector3 snappedPos = TrySnap(targetPos);
            heldPipe.transform.position = snappedPos;
        }
    }

    void ReleasePipe()
    {
        heldPipe.OnRelease();
        heldPipe = null;
    }

    Vector3 TrySnap(Vector3 pos)
    {
        float bestDist = snapRange;
        Vector3 bestPoint = pos;

        foreach (Vector3 point in gridPoints)
        {
            // Compare on XZ only so height difference doesn't affect snapping
            float dist = Vector2.Distance(new Vector2(pos.x, pos.z),
                                          new Vector2(point.x, point.z));
            if (dist < bestDist)
            {
                bestDist = dist;
                bestPoint = point;
            }
        }

        // Preserve the pipe's height when returning an unsnapped position
        if (bestPoint == pos)
            return pos;

        bestPoint.y = heldPipe.heightAboveTable + tableTransform.position.y;
        return bestPoint;
    }

    void OnDrawGizmos()
    {
        if (tableTransform == null) return;

        Gizmos.color = Color.cyan;
        foreach (Vector3 point in gridPoints)
        {
            // Draw flat squares on the table surface
            Gizmos.DrawWireCube(point, new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f));
        }
    }
}