using UnityEngine;

/// <summary>
/// Attach to the PLAYER (or Main Camera).
/// Handles first-person grab, carry, grid-snap, and drop for any object with a GridPipe component.
///
/// SETUP:
///   1. Put this on your Player or Camera.
///   2. Set pipeLayer to the layer your pipes are on.
///   3. Set tableSurfaceLayer to the layer the welding table mesh is on.
///   4. Assign tableGrid (or leave null — it will auto-find the first TableGrid in the scene).
///   5. Each pipe needs: Collider + Rigidbody + GridPipe component, on the pipeLayer.
///   6. The welding table mesh must be on the tableSurfaceLayer.
/// </summary>
public class PipeGrabSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main camera. If null, uses Camera.main.")]
    public Camera cam;
    [Tooltip("The welding table grid. If null, auto-finds the first TableGrid in the scene.")]
    public TableGrid tableGrid;

    [Header("Grab")]
    [Tooltip("Max distance to grab a pipe (from camera)")]
    public float grabRange = 5f;
    [Tooltip("Layer mask for pipe objects")]
    public LayerMask pipeLayer;

    [Header("Carry")]
    [Tooltip("How far in front of the camera the pipe floats when NOT over the table")]
    public float carryDistance = 1.5f;
    [Tooltip("Smoothing speed (higher = snappier)")]
    public float carrySmooth = 15f;
    [Tooltip("Offset from camera centre while carrying (x=right, y=up, z=forward)")]
    public Vector3 carryOffset = new Vector3(0.2f, -0.3f, 1.5f);

    [Header("Table Surface")]
    [Tooltip("Layer mask for the welding table surface (for surface raycasting)")]
    public LayerMask tableSurfaceLayer;
    [Tooltip("Extra height above the table surface hit point")]
    public float tableHeightOffset = 0.05f;

    [Header("Input")]
    public KeyCode grabKey = KeyCode.E;

    [Header("Return to Battery")]
    [Tooltip("How close to the pipe's original position to show the return prompt")]
    public float returnProximity = 5f;

    // ── Runtime ──
    GridPipe heldPipe;

    // Preview highlight
    GameObject highlightCube;
    Renderer highlightRenderer;
    Material highlightMat;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (tableGrid == null)
            tableGrid = FindFirstObjectByType<TableGrid>();

        CreateHighlight();
    }

    void Update()
    {
        if (cam == null) return;

        // Block all pipe interaction until pipes are drained
        if (!QuestManager.IsDrainDone) return;

        // Toggle grab / drop / return on key press
        if (Input.GetKeyDown(grabKey))
        {
            if (heldPipe != null)
            {
                if (IsNearPipeStart())
                    ReturnPipe();
                else
                    Drop();
            }
            else
            {
                TryGrab();
            }
        }



        // While holding, move the pipe
        if (heldPipe != null)
            CarryPipe();
        else
            HideHighlight();
    }

    // ════════════════════════ GRAB ════════════════════════

    void TryGrab()
    {
        Ray ray = GetCenterRay();

        if (!Physics.Raycast(ray, out RaycastHit hit, grabRange, pipeLayer))
            return;

        // Walk up the hierarchy to find a GridPipe
        GridPipe pipe = hit.collider.GetComponent<GridPipe>();
        if (pipe == null) pipe = hit.collider.GetComponentInParent<GridPipe>();
        if (pipe == null) return;

        // If it's on the grid, free the cell first
        if (pipe.isOnGrid && pipe.snappedGrid != null)
        {
            pipe.snappedGrid.Free(pipe.snappedCell);
            pipe.snappedGrid = null;
            pipe.snappedCell = -1;
        }

        // Save position for return
        pipe.SavePosition();

        // Pick it up
        pipe.isHeld   = true;
        pipe.isOnGrid = false;
        pipe.Unfreeze();
        pipe.transform.SetParent(null);

        heldPipe = pipe;

        Debug.Log($"[PipeGrabSystem] Grabbed: {pipe.gameObject.name}");
    }

    // ════════════════════════ CARRY ════════════════════════

    void CarryPipe()
    {
        if (heldPipe == null) return;

        Ray ray = GetCenterRay();

        // Check if we're looking at the table surface
        bool onTable = false;
        Vector3 targetPos = GetFreeCarryPosition();
        Quaternion targetRot = cam.transform.rotation;

        if (tableSurfaceLayer.value != 0 &&
            Physics.Raycast(ray, out RaycastHit tableHit, 20f, tableSurfaceLayer))
        {
            // We're pointing at the table — slide the pipe on the surface
            onTable = true;
            targetPos = tableHit.point + Vector3.up * tableHeightOffset;

            // Try to snap to nearest grid cell
            if (tableGrid != null &&
                tableGrid.GetNearestFreeCell(targetPos, out int cellIdx, out Vector3 cellPos))
            {
                targetPos = cellPos + Vector3.up * heldPipe.heightOffset;
                targetRot = tableGrid.TableRotation * Quaternion.Euler(heldPipe.snapRotationOffset);
                ShowHighlight(cellPos);
            }
            else if (tableGrid != null &&
                     tableGrid.GetNearestCell(targetPos, out int anyCellIdx, out Vector3 anyCellPos))
            {
                // Cell occupied — show red highlight? For now just use the raw table position.
                targetPos = tableHit.point + Vector3.up * tableHeightOffset;
                targetRot = tableGrid.TableRotation * Quaternion.Euler(heldPipe.snapRotationOffset);
                HideHighlight();
            }
            else
            {
                // On table but outside grid area
                targetRot = tableGrid != null
                    ? tableGrid.TableRotation * Quaternion.Euler(heldPipe.snapRotationOffset)
                    : cam.transform.rotation;
                HideHighlight();
            }
        }
        else
        {
            HideHighlight();
        }

        // Smoothly move the pipe
        heldPipe.transform.position = Vector3.Lerp(
            heldPipe.transform.position, targetPos, Time.deltaTime * carrySmooth);
        heldPipe.transform.rotation = Quaternion.Slerp(
            heldPipe.transform.rotation, targetRot, Time.deltaTime * carrySmooth);
    }

    Vector3 GetFreeCarryPosition()
    {
        Transform t = cam.transform;
        return t.position
             + t.forward * carryOffset.z
             + t.right   * carryOffset.x
             + t.up      * carryOffset.y;
    }

    // ════════════════════════ DROP ════════════════════════

    void Drop()
    {
        if (heldPipe == null) return;

        HideHighlight();

        // Check if we can snap to a grid cell
        bool snapped = false;

        if (tableGrid != null)
        {
            Vector3 pipePos = heldPipe.transform.position;
            if (tableGrid.GetNearestFreeCell(pipePos, out int cellIdx, out Vector3 cellPos))
            {
                // Snap!
                Vector3 finalPos = cellPos + Vector3.up * heldPipe.heightOffset;
                Quaternion finalRot = tableGrid.TableRotation * Quaternion.Euler(heldPipe.snapRotationOffset);

                heldPipe.transform.position = finalPos;
                heldPipe.transform.rotation = finalRot;

                heldPipe.isOnGrid    = true;
                heldPipe.snappedGrid = tableGrid;
                heldPipe.snappedCell = cellIdx;
                tableGrid.Occupy(cellIdx, heldPipe);

                // Notify quest system
                if (QuestManager.Instance != null)
                    QuestManager.Instance.UpdateQuest("place_pipe");

                Debug.Log($"[PipeGrabSystem] Snapped {heldPipe.gameObject.name} to cell {cellIdx}");
                snapped = true;
            }
        }

        if (!snapped)
        {
            Debug.Log($"[PipeGrabSystem] Dropped {heldPipe.gameObject.name} freely (no grid cell nearby).");
        }

        heldPipe.isHeld = false;
        heldPipe.Freeze();
        heldPipe = null;
    }

    // ════════════════════════ RETURN TO BATTERY ════════════════════════

    bool IsNearPipeStart()
    {
        if (heldPipe == null || cam == null) return false;
        float dist = Vector3.Distance(cam.transform.position, heldPipe.startPos);
        return dist <= returnProximity;
    }

    void ReturnPipe()
    {
        if (heldPipe == null) return;

        HideHighlight();
        heldPipe.ReturnToStart();
        QuestManager.Instance?.UpdateQuest("return_pipe");
        Debug.Log($"[PipeGrabSystem] Returned {heldPipe.gameObject.name} to original position.");
        heldPipe = null;
    }

    // ════════════════════════ HIGHLIGHT ════════════════════════

    void CreateHighlight()
    {
        highlightCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlightCube.name = "GridSnapHighlight";

        // Remove collider
        Collider col = highlightCube.GetComponent<Collider>();
        if (col != null) Destroy(col);

        float cs = tableGrid != null ? tableGrid.cellSize : 0.25f;
        highlightCube.transform.localScale = new Vector3(cs * 0.9f, 0.015f, cs * 0.9f);

        highlightRenderer = highlightCube.GetComponent<Renderer>();
        highlightMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (highlightMat == null)
            highlightMat = new Material(Shader.Find("Standard"));

        // Transparent green
        highlightMat.SetFloat("_Surface", 1f);
        highlightMat.SetOverrideTag("RenderType", "Transparent");
        highlightMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        highlightMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        highlightMat.renderQueue = 3000;
        highlightMat.color = new Color(0f, 1f, 0.4f, 0.6f);

        highlightRenderer.material = highlightMat;
        highlightCube.SetActive(false);
    }

    void ShowHighlight(Vector3 worldPos)
    {
        if (highlightCube == null) return;
        highlightCube.SetActive(true);
        highlightCube.transform.position = worldPos;
        if (tableGrid != null)
            highlightCube.transform.rotation = tableGrid.TableRotation;

        // Pulse
        float cs = tableGrid != null ? tableGrid.cellSize : 0.25f;
        float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.08f;
        highlightCube.transform.localScale = new Vector3(cs * 0.9f * pulse, 0.015f, cs * 0.9f * pulse);
    }

    void HideHighlight()
    {
        if (highlightCube != null)
            highlightCube.SetActive(false);
    }

    // ════════════════════════ UI PROMPT ════════════════════════

    void OnGUI()
    {
        if (cam == null) return;

        // Hide pipe prompts until pipes are drained
        if (!QuestManager.IsDrainDone) return;

        string prompt = null;
        string key = PromptUI.KeyName(grabKey);
        bool isReturn = false;

        if (heldPipe != null)
        {
            // Holding a pipe — check if near original position (battery)
            if (IsNearPipeStart())
            {
                prompt = "Return Pipe To The Battery";
                isReturn = true;
            }
            else
            {
                bool nearGrid = tableGrid != null &&
                    tableGrid.GetNearestFreeCell(heldPipe.transform.position, out _, out _);

                prompt = nearGrid
                    ? "Place On The Table"
                    : "Drop Pipe";
            }
        }
        else
        {
            // Not holding — check if looking at a pipe
            Ray ray = GetCenterRay();
            if (Physics.Raycast(ray, out RaycastHit hit, grabRange, pipeLayer))
            {
                GridPipe pipe = hit.collider.GetComponent<GridPipe>();
                if (pipe == null) pipe = hit.collider.GetComponentInParent<GridPipe>();
                if (pipe != null)
                    prompt = "Grab Pipe";
            }
        }

        if (string.IsNullOrEmpty(prompt)) return;

        if (isReturn)
            PromptUI.Draw(key, prompt, Color.white, new Color(0.4f, 0.75f, 1f));
        else
            PromptUI.Draw(key, prompt);
    }

    // ════════════════════════ HELPERS ════════════════════════

    Ray GetCenterRay()
    {
        return cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }
}
