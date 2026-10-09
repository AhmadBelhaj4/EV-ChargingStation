using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbablePipe : MonoBehaviour
{
    [Header("Grab Settings")]
    [SerializeField] private float grabDistance = 10f;
    [SerializeField] private float holdDistance = 1.5f;
    [SerializeField] private float holdSmoothing = 12f;
    [SerializeField] private KeyCode grabKey = KeyCode.E;

    [Header("Hold Position (offset from camera center)")]
    [Tooltip("X = right, Y = down from center, Z = forward")]
    [SerializeField] private Vector3 holdOffset = new Vector3(0.3f, -0.4f, 1.5f);
    [Tooltip("Rotation of the pipe while held (euler angles relative to camera)")]
    [SerializeField] private Vector3 holdRotationOffset = new Vector3(0f, 0f, 0f);

    [Header("Grid Snap Settings")]
    [Tooltip("Rotation offset applied when snapping to grid (euler angles)")]
    [SerializeField] private Vector3 snapRotationOffset = Vector3.zero;
    [Tooltip("Height offset above the grid cell position")]
    [SerializeField] private float snapHeightOffset = 0f;

    [Header("UI Prompt")]
    [SerializeField] private string grabPrompt = "Press E to grab pipe";
    [SerializeField] private string dropPrompt = "Press E to drop pipe";

    [Header("Return Settings")]
    [Tooltip("How close the player must be to the pipe's original position to return it")]
    [SerializeField] private float returnProximity = 3f;

    [Header("State (read-only)")]
    [SerializeField] private bool isGrabbed = false;
    [SerializeField] private bool isOnGrid = false;
    [SerializeField] private bool isSnapping = false;

    private Rigidbody _rb;
    private Camera _cam;
    private Transform _originalParent;
    private bool _showPrompt = false;
    private string _currentPrompt = "";

    // Return position — captured at grab time (not Start), so it uses current car position
    private Vector3 _returnPosition;
    private Quaternion _returnRotation;
    private Transform _returnParent;
    private bool _hasReturnPoint = false;

    // Original Start() position — used for returning the pipe to the battery area
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Transform _startParent;

    // Grid snap animation
    private WeldingTableGrid _snapGrid;
    private Vector2Int _occupiedCell;
    private Vector3 _snapStartPos;
    private Quaternion _snapStartRot;
    private Vector3 _snapTargetPos;
    private Quaternion _snapTargetRot;
    private float _snapProgress;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main;
        _originalParent = transform.parent;

        // Save original position for return-to-battery
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        _startParent = transform.parent;

        // Add a big box collider if none exists (so raycast can hit the pipe)
        if (GetComponent<Collider>() == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            // Make it bigger than the mesh for easier clicking
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                box.center = transform.InverseTransformPoint(rend.bounds.center);
                box.size = transform.InverseTransformVector(rend.bounds.size) * 1.3f;
            }
            Debug.Log($"{gameObject.name}: Auto-added BoxCollider for grab detection.");
        }

        FreezeRigidbody();
    }

    void Update()
    {
        // Handle snap animation
        if (isSnapping)
        {
            AnimateSnap();
            return;
        }

        HandleInteraction();

        if (isGrabbed)
        {
            FollowCamera();
            UpdateGridHighlights();
        }
    }

    void HandleInteraction()
    {
        // Block all pipe interaction until pipes are drained
        if (!QuestManager.IsDrainDone) return;
        if (!Input.GetKeyDown(grabKey)) return;

        // If already holding — return to original spot or drop
        if (isGrabbed)
        {
            if (IsNearReturnPosition())
                ReturnToOriginalPosition();
            else
                Drop();
            return;
        }

        // If on grid, pick it back up
        if (isOnGrid)
        {
            GrabFromGrid();
            return;
        }

        // Try to grab via raycast from screen center
        Ray ray = _cam.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            if (hit.collider.gameObject == gameObject ||
                hit.collider.transform.IsChildOf(transform) ||
                transform.IsChildOf(hit.collider.transform))
            {
                Grab();
            }
        }
    }

    void LateUpdate()
    {
        _showPrompt = false;

        // Hide all prompts until pipes are drained
        if (!QuestManager.IsDrainDone) return;

        if (isSnapping) return;

        if (isGrabbed)
        {
            _showPrompt = true;

            // Check if near original position (battery area) — show return prompt
            if (IsNearReturnPosition())
            {
                _currentPrompt = "return_pipe";
                return;
            }

            // Check if near any grid
            bool nearGrid = false;
            foreach (var grid in WeldingTableGrid.AllGrids)
            {
                if (grid.TryGetNearestFreeCell(transform.position, out _, out _))
                {
                    nearGrid = true;
                    break;
                }
            }
            _currentPrompt = nearGrid
                ? $"{dropPrompt} (will snap to grid)"
                : dropPrompt;
            return;
        }

        if (isOnGrid)
        {
            // Show "pick up from grid" prompt if player is looking at it
            Ray ray = _cam.ScreenPointToRay(
                new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
            {
                if (hit.collider.gameObject == gameObject ||
                    hit.collider.transform.IsChildOf(transform) ||
                    transform.IsChildOf(hit.collider.transform))
                {
                    _showPrompt = true;
                    _currentPrompt = "Press E to pick up pipe";
                }
            }
            return;
        }

        // Check if player is looking at pipe — use a generous check
        Ray lookRay = _cam.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(lookRay, out RaycastHit lookHit, grabDistance))
        {
            if (lookHit.collider.gameObject == gameObject ||
                lookHit.collider.transform.IsChildOf(transform) ||
                transform.IsChildOf(lookHit.collider.transform))
            {
                _showPrompt = true;
                _currentPrompt = grabPrompt;
            }
        }
    }

    // ==================== GRAB / DROP ====================

    void Grab()
    {
        // Save return point NOW (at current world position, after car has moved)
        _returnPosition = transform.position;
        _returnRotation = transform.rotation;
        _returnParent = transform.parent;
        _hasReturnPoint = true;

        isGrabbed = true;
        isOnGrid = false;

        FreezeRigidbody();
        transform.SetParent(null);

        // Immediately place pipe in front of player so it's visible
        Vector3 targetPos = GetHoldPosition();
        Quaternion targetRot = GetHoldRotation();
        transform.position = targetPos;
        transform.rotation = targetRot;

        Debug.Log($"{gameObject.name} grabbed! Return pos saved at {_returnPosition}");
    }

    void GrabFromGrid()
    {
        // Free the grid cell first
        if (_snapGrid != null)
        {
            _snapGrid.FreeCellByPipe(this);
            _snapGrid = null;
        }

        // Save return point as the grid position
        _returnPosition = transform.position;
        _returnRotation = transform.rotation;
        _returnParent = transform.parent;
        _hasReturnPoint = true;

        isGrabbed = true;
        isOnGrid = false;

        FreezeRigidbody();
        transform.SetParent(null);

        // Immediately place in front of player
        transform.position = GetHoldPosition();
        transform.rotation = GetHoldRotation();

        Debug.Log($"{gameObject.name} picked up from grid!");
    }

    void Drop()
    {
        isGrabbed = false;

        // Hide all grid highlights
        foreach (var grid in WeldingTableGrid.AllGrids)
            grid.HideHighlight();

        // Try to snap to nearest grid cell
        WeldingTableGrid bestGrid = null;
        Vector3 bestSnapPos = Vector3.zero;
        Vector2Int bestCell = Vector2Int.zero;
        float bestDist = float.MaxValue;

        foreach (var grid in WeldingTableGrid.AllGrids)
        {
            if (grid.TryGetNearestFreeCell(transform.position, out Vector3 snapPos, out Vector2Int cell))
            {
                float dist = Vector3.Distance(transform.position, snapPos);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestGrid = grid;
                    bestSnapPos = snapPos;
                    bestCell = cell;
                }
            }
        }

        if (bestGrid != null)
        {
            StartSnapAnimation(bestGrid, bestSnapPos, bestCell);
        }
        else
        {
            // No grid nearby — just drop in place
            FreezeRigidbody();
            Debug.Log($"{gameObject.name} dropped (no grid cell nearby).");
        }
    }

    // ==================== SNAP ANIMATION ====================

    void StartSnapAnimation(WeldingTableGrid grid, Vector3 targetPos, Vector2Int cell)
    {
        isSnapping = true;
        _snapGrid = grid;
        _occupiedCell = cell;
        _snapStartPos = transform.position;
        _snapStartRot = transform.rotation;
        _snapTargetPos = targetPos + Vector3.up * snapHeightOffset;
        _snapTargetRot = grid.SnapRotation * Quaternion.Euler(snapRotationOffset);
        _snapProgress = 0f;
        Debug.Log($"{gameObject.name} snapping to grid cell ({cell.x},{cell.y})...");
    }

    void AnimateSnap()
    {
        if (_snapGrid == null)
        {
            isSnapping = false;
            return;
        }

        _snapProgress += Time.deltaTime * _snapGrid.SnapLerpSpeed;
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_snapProgress));

        transform.position = Vector3.Lerp(_snapStartPos, _snapTargetPos, t);
        transform.rotation = Quaternion.Slerp(_snapStartRot, _snapTargetRot, t);

        if (_snapProgress >= 1f)
            FinishSnap();
    }

    void FinishSnap()
    {
        isSnapping = false;
        isOnGrid = true;

        // Final exact position
        transform.position = _snapTargetPos;
        transform.rotation = _snapTargetRot;

        FreezeRigidbody();
        _snapGrid.OccupyCell(_occupiedCell, this);

        QuestManager.Instance?.UpdateQuest("place_pipe");

        Debug.Log($"{gameObject.name} snapped to grid!");
    }

    // ==================== GRID HIGHLIGHT ====================

    void UpdateGridHighlights()
    {
        foreach (var grid in WeldingTableGrid.AllGrids)
        {
            grid.UpdateProximityHighlight(transform.position);
        }
    }

    // ==================== RETURN ====================

    /// <summary>
    /// Returns pipe to where it was grabbed from (not Start position).
    /// If the car has moved, it returns to the car's current child position.
    /// </summary>
    public void ReturnToOriginalPosition()
    {
        // Free grid cell if on grid
        if (isOnGrid && _snapGrid != null)
        {
            _snapGrid.FreeCellByPipe(this);
            _snapGrid = null;
        }

        isOnGrid = false;
        isGrabbed = false;
        isSnapping = false;

        // Always return to original Start() position (near battery)
        transform.SetParent(_startParent);
        transform.position = _startPosition;
        transform.rotation = _startRotation;

        FreezeRigidbody();
        QuestManager.Instance?.UpdateQuest("return_pipe");
        Debug.Log($"{gameObject.name} returned to original position!");
    }

    // ==================== HELPERS ====================

    Vector3 GetHoldPosition()
    {
        Transform camT = _cam.transform;
        return camT.position
             + camT.forward  * holdOffset.z
             + camT.right    * holdOffset.x
             + camT.up       * holdOffset.y;
    }

    Quaternion GetHoldRotation()
    {
        return _cam.transform.rotation * Quaternion.Euler(holdRotationOffset);
    }

    void FollowCamera()
    {
        Vector3 targetPos = GetHoldPosition();
        Quaternion targetRot = GetHoldRotation();

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * holdSmoothing);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * holdSmoothing);
    }

    void FreezeRigidbody()
    {
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    bool IsNearReturnPosition()
    {
        if (_cam == null) return false;
        float dist = Vector3.Distance(_cam.transform.position, _startPosition);
        return dist <= returnProximity;
    }

    void OnGUI()
    {
        if (!_showPrompt || string.IsNullOrEmpty(_currentPrompt)) return;

        string key = PromptUI.KeyName(grabKey);

        if (_currentPrompt == "return_pipe")
            PromptUI.Draw(key, "Return pipe to battery",
                Color.white, new Color(0.4f, 0.75f, 1f));
        else
            PromptUI.Draw(key, _currentPrompt);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabDistance);
    }

    public bool IsOnGrid => isOnGrid;
    public bool IsGrabbed => isGrabbed;
}