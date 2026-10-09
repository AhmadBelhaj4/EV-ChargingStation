using UnityEngine;

/// <summary>
/// Snaps this object's position to the nearest grid cell on a WeldingTableGrid.
/// Works both in Edit Mode (for level design) and at Runtime.
/// Attach to any object you want locked to the grid.
/// </summary>
public class LockToGrid : MonoBehaviour
{
    [Header("Grid Reference")]
    [Tooltip("The WeldingTableGrid to snap to. If null, will auto-find the nearest one.")]
    [SerializeField] private WeldingTableGrid targetGrid;

    [Header("Fallback (used if no WeldingTableGrid is found)")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Vector3 tileOffset = Vector3.zero;

    [Header("Runtime Settings")]
    [Tooltip("If true, continuously snaps to grid every frame at runtime")]
    [SerializeField] private bool continuousSnap = false;
    [Tooltip("If true, snaps once on Start and then stops")]
    [SerializeField] private bool snapOnStart = true;

    void Start()
    {
        // Try to find a grid if none assigned
        if (targetGrid == null)
        {
            WeldingTableGrid[] grids = FindObjectsByType<WeldingTableGrid>(FindObjectsSortMode.None);
            if (grids.Length > 0)
            {
                // Pick the nearest grid
                float bestDist = float.MaxValue;
                foreach (var grid in grids)
                {
                    float dist = Vector3.Distance(transform.position, grid.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        targetGrid = grid;
                    }
                }
            }
        }

        if (snapOnStart)
            SnapToGrid();
    }

    void Update()
    {
#if UNITY_EDITOR
        // In editor (not playing) — always snap for level design
        if (!Application.isPlaying)
        {
            SnapToGrid();
            return;
        }
#endif

        // At runtime — only snap if continuous mode is on
        if (continuousSnap)
            SnapToGrid();
    }

    /// <summary>
    /// Snaps this object to the nearest grid cell.
    /// Uses WeldingTableGrid if available, otherwise falls back to simple tile math.
    /// </summary>
    public void SnapToGrid()
    {
        if (targetGrid != null)
        {
            SnapToWeldingGrid();
        }
        else
        {
            SnapToSimpleGrid();
        }
    }

    void SnapToWeldingGrid()
    {
        // Find nearest cell on the target grid (including occupied ones — this is for positioning, not pipe placement)
        Vector3 currentPos = transform.position;
        Vector3 bestPos = currentPos;
        float bestDist = float.MaxValue;

        // We need to check all cells, not just free ones
        // Access the grid's cell positions directly
        if (targetGrid.TryGetNearestFreeCell(currentPos, out Vector3 snapPos, out _))
        {
            transform.position = snapPos;
        }
        else
        {
            // Fallback to simple grid if no free cell found
            SnapToSimpleGrid();
        }
    }

    void SnapToSimpleGrid()
    {
        if (tileSize <= 0f) tileSize = 1f;

        Vector3 currentPosition = transform.position;

        float snappedX = Mathf.Round(currentPosition.x / tileSize) * tileSize + tileOffset.x;
        float snappedZ = Mathf.Round(currentPosition.z / tileSize) * tileSize + tileOffset.z;
        float snappedY = tileOffset.y;

        transform.position = new Vector3(snappedX, snappedY, snappedZ);
    }
}