using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach to the welding table. Creates a visible grid on the table surface.
/// Pipes snap to the nearest free grid cell when dropped close enough.
/// The grid is defined in the table's local XZ plane.
/// </summary>
public class WeldingTableGrid : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [Tooltip("Number of columns (along local X)")]
    [SerializeField] private int columns = 5;
    [Tooltip("Number of rows (along local Z)")]
    [SerializeField] private int rows = 3;
    [Tooltip("Size of each grid cell in world units")]
    [SerializeField] private float cellSize = 0.3f;

    [Header("Grid Positioning")]
    [Tooltip("Offset from the table's pivot to the grid center")]
    [SerializeField] private Vector3 gridOffset = new Vector3(0f, 0.05f, 0f);

    [Header("Snap Settings")]
    [Tooltip("How close the pipe must be (world units) to snap to a cell")]
    [SerializeField] private float snapProximity = 1.5f;
    [Tooltip("Speed of the snap animation (higher = faster)")]
    [SerializeField] private float snapLerpSpeed = 8f;

    [Header("Visual Settings")]
    [Tooltip("Color of the grid lines")]
    [SerializeField] private Color gridLineColor = new Color(0f, 1f, 0.6f, 0.5f);
    [Tooltip("Width of the grid lines")]
    [SerializeField] private float lineWidth = 0.005f;
    [Tooltip("Color when a cell is highlighted (pipe nearby)")]
    [SerializeField] private Color highlightColor = new Color(0f, 1f, 0.3f, 0.9f);
    [Tooltip("Color of an occupied cell indicator")]
    [SerializeField] private Color occupiedColor = new Color(1f, 0.3f, 0.3f, 0.4f);

    // Internal data
    private Dictionary<Vector2Int, GrabbablePipe> _occupiedCells = new Dictionary<Vector2Int, GrabbablePipe>();
    private GameObject _gridVisualRoot;
    private GameObject _highlightIndicator;
    private Renderer _highlightRenderer;
    private Material _highlightMat;
    private Vector2Int _currentHighlightCell = new Vector2Int(-9999, -9999);
    private bool _highlightActive = false;

    // Singleton-like access (supports multiple grids via FindObjectsByType)
    private static WeldingTableGrid[] _allGrids;

    /// <summary>Returns all active WeldingTableGrid instances.</summary>
    public static WeldingTableGrid[] AllGrids
    {
        get
        {
            if (_allGrids == null || _allGrids.Length == 0)
                _allGrids = FindObjectsByType<WeldingTableGrid>(FindObjectsSortMode.None);
            return _allGrids;
        }
    }

    void Awake()
    {
        _allGrids = null; // Force refresh
    }

    void Start()
    {
        CreateGridVisuals();
        CreateHighlightIndicator();
    }

    void OnDestroy()
    {
        _allGrids = null;
    }

    // ===================== PUBLIC API =====================

    /// <summary>
    /// Given a world position, returns the nearest free grid cell's world position.
    /// Returns false if no free cell is within snapProximity.
    /// </summary>
    public bool TryGetNearestFreeCell(Vector3 worldPos, out Vector3 snapWorldPos, out Vector2Int cellIndex)
    {
        snapWorldPos = Vector3.zero;
        cellIndex = Vector2Int.zero;

        float bestDist = float.MaxValue;
        bool found = false;

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector2Int idx = new Vector2Int(x, z);
                if (_occupiedCells.ContainsKey(idx)) continue;

                Vector3 cellWorld = GetCellWorldPosition(x, z);
                float dist = Vector3.Distance(worldPos, cellWorld);

                if (dist < bestDist && dist <= snapProximity)
                {
                    bestDist = dist;
                    snapWorldPos = cellWorld;
                    cellIndex = idx;
                    found = true;
                }
            }
        }

        return found;
    }

    /// <summary>
    /// Marks a cell as occupied by a pipe.
    /// </summary>
    public void OccupyCell(Vector2Int cellIndex, GrabbablePipe pipe)
    {
        _occupiedCells[cellIndex] = pipe;
        Debug.Log($"[WeldingTableGrid] Cell ({cellIndex.x},{cellIndex.y}) occupied by {pipe.gameObject.name}");
    }

    /// <summary>
    /// Frees a cell.
    /// </summary>
    public void FreeCell(Vector2Int cellIndex)
    {
        if (_occupiedCells.ContainsKey(cellIndex))
        {
            Debug.Log($"[WeldingTableGrid] Cell ({cellIndex.x},{cellIndex.y}) freed.");
            _occupiedCells.Remove(cellIndex);
        }
    }

    /// <summary>
    /// Frees whichever cell this pipe occupies.
    /// </summary>
    public void FreeCellByPipe(GrabbablePipe pipe)
    {
        Vector2Int? found = null;
        foreach (var kvp in _occupiedCells)
        {
            if (kvp.Value == pipe)
            {
                found = kvp.Key;
                break;
            }
        }
        if (found.HasValue)
            FreeCell(found.Value);
    }

    /// <summary>
    /// Shows a highlight indicator at the nearest free cell if the given position is close enough.
    /// Call every frame while carrying a pipe.
    /// </summary>
    public void UpdateProximityHighlight(Vector3 pipeWorldPos)
    {
        if (TryGetNearestFreeCell(pipeWorldPos, out Vector3 snapPos, out Vector2Int cellIdx))
        {
            ShowHighlight(snapPos, cellIdx);
        }
        else
        {
            HideHighlight();
        }
    }

    /// <summary>
    /// Hides the highlight indicator.
    /// </summary>
    public void HideHighlight()
    {
        if (_highlightIndicator != null && _highlightActive)
        {
            _highlightIndicator.SetActive(false);
            _highlightActive = false;
            _currentHighlightCell = new Vector2Int(-9999, -9999);
        }
    }

    public float SnapLerpSpeed => snapLerpSpeed;

    /// <summary>
    /// The rotation to apply to a pipe when it snaps to this grid (matches the table's Y rotation).
    /// </summary>
    public Quaternion SnapRotation => transform.rotation;

    // ===================== GRID MATH =====================

    /// <summary>
    /// Converts a cell index (col, row) to a world-space position on the grid.
    /// The grid is centered on (transform.position + gridOffset).
    /// </summary>
    public Vector3 GetCellWorldPosition(int col, int row)
    {
        // Grid spans from -halfWidth to +halfWidth in local X,
        // and from -halfDepth to +halfDepth in local Z.
        float halfWidth = (columns - 1) * cellSize * 0.5f;
        float halfDepth = (rows - 1) * cellSize * 0.5f;

        Vector3 localPos = new Vector3(
            col * cellSize - halfWidth,
            0f,
            row * cellSize - halfDepth
        );

        // Transform to world space using the table's transform
        return transform.TransformPoint(localPos + gridOffset);
    }

    // ===================== VISUALS =====================

    void CreateGridVisuals()
    {
        // Clean up old visuals
        if (_gridVisualRoot != null) Destroy(_gridVisualRoot);

        _gridVisualRoot = new GameObject("GridVisuals");
        _gridVisualRoot.transform.SetParent(transform);
        _gridVisualRoot.transform.localPosition = Vector3.zero;
        _gridVisualRoot.transform.localRotation = Quaternion.identity;

        // Use Unlit/Color shader for visible grid lines
        Material lineMat = new Material(Shader.Find("Sprites/Default"));
        lineMat.color = gridLineColor;

        float halfWidth = (columns - 1) * cellSize * 0.5f;
        float halfDepth = (rows - 1) * cellSize * 0.5f;

        // Extra padding around outer cells
        float pad = cellSize * 0.5f;

        // Draw vertical lines (along Z) — columns + 1 lines
        for (int x = 0; x <= columns - 1; x++)
        {
            float localX = x * cellSize - halfWidth;
            Vector3 start = new Vector3(localX, 0f, -halfDepth - pad) + gridOffset;
            Vector3 end   = new Vector3(localX, 0f,  halfDepth + pad) + gridOffset;

            // Also draw half-step lines for cell boundaries
            if (x < columns - 1)
            {
                float midX = localX + cellSize * 0.5f;
                CreateGridLine($"GridLine_VBound_{x}", lineMat,
                    new Vector3(midX, 0f, -halfDepth - pad) + gridOffset,
                    new Vector3(midX, 0f,  halfDepth + pad) + gridOffset,
                    lineWidth * 0.5f);
            }

            CreateGridLine($"GridLine_V_{x}", lineMat, start, end, lineWidth);
        }

        // Draw horizontal lines (along X) — rows + 1 lines
        for (int z = 0; z <= rows - 1; z++)
        {
            float localZ = z * cellSize - halfDepth;
            Vector3 start = new Vector3(-halfWidth - pad, 0f, localZ) + gridOffset;
            Vector3 end   = new Vector3( halfWidth + pad, 0f, localZ) + gridOffset;

            if (z < rows - 1)
            {
                float midZ = localZ + cellSize * 0.5f;
                CreateGridLine($"GridLine_HBound_{z}", lineMat,
                    new Vector3(-halfWidth - pad, 0f, midZ) + gridOffset,
                    new Vector3( halfWidth + pad, 0f, midZ) + gridOffset,
                    lineWidth * 0.5f);
            }

            CreateGridLine($"GridLine_H_{z}", lineMat, start, end, lineWidth);
        }

        // Draw cell boundary lines (the outer rectangle edges)
        // Left edge
        CreateGridLine("GridLine_BoundL", lineMat,
            new Vector3(-halfWidth - pad, 0f, -halfDepth - pad) + gridOffset,
            new Vector3(-halfWidth - pad, 0f,  halfDepth + pad) + gridOffset, lineWidth);
        // Right edge
        CreateGridLine("GridLine_BoundR", lineMat,
            new Vector3(halfWidth + pad, 0f, -halfDepth - pad) + gridOffset,
            new Vector3(halfWidth + pad, 0f,  halfDepth + pad) + gridOffset, lineWidth);
        // Bottom edge
        CreateGridLine("GridLine_BoundB", lineMat,
            new Vector3(-halfWidth - pad, 0f, -halfDepth - pad) + gridOffset,
            new Vector3( halfWidth + pad, 0f, -halfDepth - pad) + gridOffset, lineWidth);
        // Top edge
        CreateGridLine("GridLine_BoundT", lineMat,
            new Vector3(-halfWidth - pad, 0f, halfDepth + pad) + gridOffset,
            new Vector3( halfWidth + pad, 0f, halfDepth + pad) + gridOffset, lineWidth);
    }

    void CreateGridLine(string name, Material mat, Vector3 localStart, Vector3 localEnd, float width)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(_gridVisualRoot.transform);
        lineObj.transform.localPosition = Vector3.zero;
        lineObj.transform.localRotation = Quaternion.identity;
        lineObj.transform.localScale = Vector3.one;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = mat;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.positionCount = 2;
        lr.useWorldSpace = false;
        lr.SetPosition(0, localStart);
        lr.SetPosition(1, localEnd);
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
    }

    void CreateHighlightIndicator()
    {
        _highlightIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _highlightIndicator.name = "GridCellHighlight";
        _highlightIndicator.transform.SetParent(transform);
        _highlightIndicator.transform.localScale = new Vector3(cellSize * 0.9f, 0.02f, cellSize * 0.9f);

        // Remove collider so it doesn't interfere
        Collider col = _highlightIndicator.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Create transparent material
        _highlightRenderer = _highlightIndicator.GetComponent<Renderer>();
        _highlightMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (_highlightMat == null)
            _highlightMat = new Material(Shader.Find("Standard"));

        _highlightMat.SetFloat("_Surface", 1f);
        _highlightMat.SetFloat("_Blend", 0f);
        _highlightMat.SetOverrideTag("RenderType", "Transparent");
        _highlightMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        _highlightMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        _highlightMat.renderQueue = 3000;
        _highlightMat.color = highlightColor;

        _highlightRenderer.material = _highlightMat;
        _highlightIndicator.SetActive(false);
    }

    void ShowHighlight(Vector3 worldPos, Vector2Int cellIdx)
    {
        if (_highlightIndicator == null) return;

        _highlightIndicator.SetActive(true);
        _highlightIndicator.transform.position = worldPos;
        _highlightIndicator.transform.rotation = transform.rotation;
        _highlightActive = true;
        _currentHighlightCell = cellIdx;

        // Pulsing effect
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.1f;
        _highlightIndicator.transform.localScale = new Vector3(
            cellSize * 0.9f * pulse, 0.02f, cellSize * 0.9f * pulse);

        _highlightMat.color = highlightColor;
    }

    // ===================== GIZMOS =====================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.6f);

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 pos = GetCellWorldPosition(x, z);
                Gizmos.DrawWireCube(pos, new Vector3(cellSize * 0.9f, 0.02f, cellSize * 0.9f));
            }
        }

        // Draw snap proximity sphere
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Vector3 center = transform.TransformPoint(gridOffset);
        Gizmos.DrawWireSphere(center, snapProximity);
    }
}
