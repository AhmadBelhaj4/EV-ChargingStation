using UnityEngine;

/// <summary>
/// Place on workbench as a snap target.
/// One snap point per pipe slot on the table.
/// Press E while near workbench to send pipe back.
/// Auto-creates a visible indicator ring if none is assigned.
/// </summary>
public class WorkbenchSnapPoint : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float returnRange = 3f;
    [SerializeField] private GameObject snapIndicator;  // visual ring/highlight on table

    [Header("Proximity Highlight")]
    [SerializeField] private Color highlightColor = new Color(0f, 1f, 0.5f, 0.8f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Auto Indicator Settings")]
    [SerializeField] private float indicatorRadius = 0.5f;
    [SerializeField] private float indicatorHeight = 0.02f;

    [Header("State")]
    [SerializeField] private GrabbablePipe occupyingPipe = null;

    private bool _playerInRange = false;
    private bool _proximityHighlight = false;
    private Renderer _indicatorRenderer;
    private Material _indicatorMat;

    public bool IsOccupied => occupyingPipe != null;

    void Start()
    {
        // Auto-create a visual indicator if none assigned
        if (snapIndicator == null)
        {
            snapIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            snapIndicator.name = "SnapIndicator";
            snapIndicator.transform.SetParent(transform);
            snapIndicator.transform.localPosition = Vector3.zero;
            snapIndicator.transform.localScale = new Vector3(
                indicatorRadius * 2f, indicatorHeight, indicatorRadius * 2f);

            // Remove the collider so it doesn't interfere with raycasts
            Collider col = snapIndicator.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Debug.Log($"{gameObject.name}: Auto-created snap indicator.");
        }

        _indicatorRenderer = snapIndicator.GetComponent<Renderer>();

        // Create a unique transparent material for the indicator
        if (_indicatorRenderer != null)
        {
            _indicatorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (_indicatorMat == null)
                _indicatorMat = new Material(Shader.Find("Standard"));

            // Make transparent
            _indicatorMat.SetFloat("_Surface", 1f);
            _indicatorMat.SetFloat("_Blend", 0f);
            _indicatorMat.SetOverrideTag("RenderType", "Transparent");
            _indicatorMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _indicatorMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            _indicatorMat.renderQueue = 3000;

            _indicatorMat.color = normalColor;
            _indicatorRenderer.material = _indicatorMat;
        }

        snapIndicator.SetActive(true);

        // Make sure there's a trigger collider for player proximity
        Collider triggerCol = GetComponent<Collider>();
        if (triggerCol == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = returnRange;
            Debug.Log($"{gameObject.name}: Auto-added trigger SphereCollider (radius={returnRange}).");
        }
        else if (!triggerCol.isTrigger)
        {
            triggerCol.isTrigger = true;
        }
    }

    void Update()
    {
        // Animate indicator when highlighted
        if (_proximityHighlight && snapIndicator != null && snapIndicator.activeSelf)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.15f;
            snapIndicator.transform.localScale = new Vector3(
                indicatorRadius * 2f * pulse,
                indicatorHeight,
                indicatorRadius * 2f * pulse);
        }

        if (!_playerInRange || !IsOccupied) return;

        if (Input.GetKeyDown(KeyCode.E))
            ReturnPipe();
    }

    /// <summary>
    /// Called by GrabbablePipe when the carried pipe is near this snap point.
    /// Changes indicator color and starts pulsing.
    /// </summary>
    public void SetProximityHighlight(bool isNear)
    {
        _proximityHighlight = isNear;

        if (snapIndicator != null && !IsOccupied && _indicatorMat != null)
        {
            _indicatorMat.color = isNear ? highlightColor : normalColor;

            if (!isNear)
            {
                // Reset scale when not highlighted
                snapIndicator.transform.localScale = new Vector3(
                    indicatorRadius * 2f, indicatorHeight, indicatorRadius * 2f);
            }
        }
    }

    public void OnPipePlaced(GrabbablePipe pipe)
    {
        occupyingPipe = pipe;

        if (snapIndicator != null)
            snapIndicator.SetActive(false);

        _proximityHighlight = false;
        Debug.Log($"Snap point occupied by {pipe.gameObject.name}");
    }

    public void OnPipeRemoved()
    {
        occupyingPipe = null;

        if (snapIndicator != null)
            snapIndicator.SetActive(true);

        SetProximityHighlight(false);
    }

    void ReturnPipe()
    {
        if (occupyingPipe == null) return;

        occupyingPipe.ReturnToOriginalPosition();
        occupyingPipe = null;

        if (snapIndicator != null)
            snapIndicator.SetActive(true);

        SetProximityHighlight(false);
        Debug.Log("Pipe returned!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            Debug.Log("Player entered workbench range");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
        }
    }

    void OnGUI()
    {
        if (!_playerInRange || !IsOccupied) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 22;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        float w = 400f, h = 40f;
        GUI.Label(
            new Rect(Screen.width / 2f - w / 2f, Screen.height - 80f, w, h),
            "Press E to return pipe",
            style);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, returnRange);
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(indicatorRadius * 2f, 0.05f, indicatorRadius * 2f));
    }
}