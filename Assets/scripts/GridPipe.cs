using UnityEngine;

/// <summary>
/// Attach to each pipe that can be grabbed and placed on the welding table grid.
/// This is a minimal state-tracker — all logic lives in PipeGrabSystem.
/// WeldableHole checks GridPipe.IsOnGrid to gate welding.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GridPipe : MonoBehaviour
{
    [Header("Snap Rotation (euler offset when placed on grid)")]
    public Vector3 snapRotationOffset = Vector3.zero;

    [Header("Height above grid cell centre")]
    public float heightOffset = 0f;

    // ── State (set by PipeGrabSystem) ──
    [HideInInspector] public bool isHeld   = false;
    [HideInInspector] public bool isOnGrid = false;

    // Where the pipe lived before being grabbed (for return)
    [HideInInspector] public Vector3    savedPos;
    [HideInInspector] public Quaternion savedRot;
    [HideInInspector] public Transform  savedParent;
    [HideInInspector] public bool       hasSavedPoint = false;

    // Original Start() position — for returning pipe to battery area
    [HideInInspector] public Vector3    startPos;
    [HideInInspector] public Quaternion startRot;
    [HideInInspector] public Transform  startParent;

    // Which grid / cell it's snapped to
    [HideInInspector] public TableGrid  snappedGrid;
    [HideInInspector] public int        snappedCell = -1;

    // Cached components
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Collider  col;

    /// <summary>Read-only: is this pipe currently placed on a grid?</summary>
    public bool IsOnGrid => isOnGrid;

    void Awake()
    {
        rb  = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Start()
    {
        // Wait for the cutscene to finish (car moves for ~20s) before saving
        Invoke(nameof(SaveStartPosition), 21f);
    }

    void SaveStartPosition()
    {
        startPos    = transform.position;
        startRot    = transform.rotation;
        startParent = transform.parent;
        Debug.Log($"{gameObject.name}: Saved start position after cutscene.");
    }

    /// <summary>
    /// Freeze the rigidbody so the pipe stays still.
    /// </summary>
    public void Freeze()
    {
        rb.isKinematic    = true;
        rb.useGravity     = false;
        rb.linearVelocity     = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints     = RigidbodyConstraints.FreezeAll;
    }

    /// <summary>
    /// Unfreeze so the pipe can be moved by the grab system.
    /// </summary>
    public void Unfreeze()
    {
        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = true;   // still kinematic — PipeGrabSystem moves it directly
        rb.useGravity  = false;
    }

    /// <summary>
    /// Save the current position so we can return here later.
    /// </summary>
    public void SavePosition()
    {
        savedPos     = transform.position;
        savedRot     = transform.rotation;
        savedParent  = transform.parent;
        hasSavedPoint = true;
    }

    /// <summary>
    /// Return the pipe to its saved position.
    /// </summary>
    public void ReturnToSaved()
    {
        if (!hasSavedPoint) return;

        if (snappedGrid != null)
        {
            snappedGrid.FreeByPipe(this);
            snappedGrid = null;
            snappedCell = -1;
        }

        isOnGrid = false;
        isHeld   = false;

        transform.SetParent(savedParent);
        transform.position = savedPos;
        transform.rotation = savedRot;
        Freeze();
    }

    /// <summary>
    /// Return the pipe to its original Start() position (battery area).
    /// </summary>
    public void ReturnToStart()
    {
        if (snappedGrid != null)
        {
            snappedGrid.FreeByPipe(this);
            snappedGrid = null;
            snappedCell = -1;
        }

        isOnGrid = false;
        isHeld   = false;

        transform.SetParent(startParent);
        transform.position = startPos;
        transform.rotation = startRot;
        Freeze();
    }
}
