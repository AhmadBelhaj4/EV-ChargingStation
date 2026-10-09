using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach to the welding table GameObject.
/// Defines a grid of snap cells on the table surface.
/// PipeGrabSystem uses this to snap pipes.
/// </summary>
public class TableGrid : MonoBehaviour
{
    [Header("Grid Size")]
    public int columns = 6;
    public int rows    = 4;
    public float cellSize = 0.25f;

    [Header("Position")]
    [Tooltip("Offset from the table pivot to the grid centre (local space)")]
    public Vector3 gridOffset = new Vector3(0f, 0.05f, 0f);

    [Header("Snapping")]
    [Tooltip("Max XZ distance for a pipe to snap to a cell")]
    public float snapRadius = 0.2f;

    // --- Runtime data ---
    Vector3[] cellWorldPositions;
    GridPipe[] cellOccupants;       // null = free

    /// <summary>Total number of cells.</summary>
    public int CellCount => columns * rows;

    void Awake()
    {
        RebuildGrid();
    }

    /// <summary>
    /// Recomputes all cell world positions. Call again if the table moves at runtime.
    /// </summary>
    public void RebuildGrid()
    {
        int count = columns * rows;
        cellWorldPositions = new Vector3[count];
        cellOccupants      = new GridPipe[count];

        float halfW = (columns - 1) * cellSize * 0.5f;
        float halfD = (rows    - 1) * cellSize * 0.5f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 local = gridOffset + new Vector3(
                    c * cellSize - halfW,
                    0f,
                    r * cellSize - halfD
                );
                cellWorldPositions[r * columns + c] = transform.TransformPoint(local);
            }
        }
    }

    // ────────────────── Public API ──────────────────

    /// <summary>
    /// Finds the nearest FREE cell to <paramref name="worldPos"/>.
    /// Returns true if one was found within <see cref="snapRadius"/>.
    /// </summary>
    public bool GetNearestFreeCell(Vector3 worldPos, out int cellIndex, out Vector3 cellWorldPos)
    {
        cellIndex    = -1;
        cellWorldPos = worldPos;
        float best   = snapRadius;

        for (int i = 0; i < cellWorldPositions.Length; i++)
        {
            if (cellOccupants[i] != null) continue;   // occupied

            Vector3 p = cellWorldPositions[i];
            // compare on XZ only so height doesn't matter
            float dx = worldPos.x - p.x;
            float dz = worldPos.z - p.z;
            float dist = Mathf.Sqrt(dx * dx + dz * dz);

            if (dist < best)
            {
                best         = dist;
                cellIndex    = i;
                cellWorldPos = p;
            }
        }

        return cellIndex >= 0;
    }

    /// <summary>
    /// Returns the world position of any cell (occupied or not) nearest to worldPos.
    /// Useful for preview highlighting.
    /// </summary>
    public bool GetNearestCell(Vector3 worldPos, out int cellIndex, out Vector3 cellWorldPos)
    {
        cellIndex    = -1;
        cellWorldPos = worldPos;
        float best   = snapRadius;

        for (int i = 0; i < cellWorldPositions.Length; i++)
        {
            Vector3 p = cellWorldPositions[i];
            float dx = worldPos.x - p.x;
            float dz = worldPos.z - p.z;
            float dist = Mathf.Sqrt(dx * dx + dz * dz);

            if (dist < best)
            {
                best         = dist;
                cellIndex    = i;
                cellWorldPos = p;
            }
        }

        return cellIndex >= 0;
    }

    public void Occupy(int cellIndex, GridPipe pipe)
    {
        if (cellIndex >= 0 && cellIndex < cellOccupants.Length)
            cellOccupants[cellIndex] = pipe;
    }

    public void Free(int cellIndex)
    {
        if (cellIndex >= 0 && cellIndex < cellOccupants.Length)
            cellOccupants[cellIndex] = null;
    }

    public void FreeByPipe(GridPipe pipe)
    {
        for (int i = 0; i < cellOccupants.Length; i++)
        {
            if (cellOccupants[i] == pipe)
            {
                cellOccupants[i] = null;
                return;
            }
        }
    }

    public Vector3 GetCellPosition(int index)
    {
        return cellWorldPositions[index];
    }

    public Quaternion TableRotation => transform.rotation;

    // ────────────────── Gizmos ──────────────────

    void OnDrawGizmos()
    {
        if (cellWorldPositions == null || cellWorldPositions.Length == 0)
        {
            // Draw preview in editor even before Awake
            RebuildGrid();
        }

        for (int i = 0; i < cellWorldPositions.Length; i++)
        {
            bool occupied = cellOccupants != null && i < cellOccupants.Length && cellOccupants[i] != null;
            Gizmos.color = occupied
                ? new Color(1f, 0.3f, 0.3f, 0.7f)
                : new Color(0f, 1f, 0.5f, 0.5f);

            Gizmos.DrawWireCube(cellWorldPositions[i],
                new Vector3(cellSize * 0.85f, 0.01f, cellSize * 0.85f));
        }
    }
}
