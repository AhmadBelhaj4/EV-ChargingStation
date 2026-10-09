using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SnapToPlace : MonoBehaviour
{
    [Header("Snap Zone")]
    public ChargingDashboard chargingDashboard;
    public Transform snapPoint;
    public Transform plugTip;
    public float snapRange = 0.03f;
    public GameObject objectToSnap;
    public string grabbableTag = "Grabbable";

    [Header("Highlight")]
    public Renderer zoneRenderer;
    public Color highlightColor = Color.green;
    public float emissionIntensity = 3.0f;

    [Header("After Snap")]
    public bool disableGravityAfterSnap = true;
    public bool lockObjectAfterSnap = true;

    private bool isObjectGrabbed = false;
    private bool candidateInside = false;
    private GameObject candidate;

    private MaterialPropertyBlock mpb;
    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (snapPoint == null)
            snapPoint = transform;

        if (zoneRenderer == null)
            zoneRenderer = GetComponent<Renderer>();

        mpb = new MaterialPropertyBlock();

        if (zoneRenderer != null && zoneRenderer.sharedMaterial != null)
            zoneRenderer.sharedMaterial.EnableKeyword("_EMISSION");

        SetHighlight(false);
    }

    public void OnObjectGrabbed(bool state)
    {
        isObjectGrabbed = state;

        if (!isObjectGrabbed)
        {
            TrySnapIfPossible();
            SetHighlight(false);
        }
        else
        {
            UpdateHighlight();
        }
    }

    public void TrySnapObject(GameObject obj)
    {
        if (obj == null) return;
        if (objectToSnap != null && obj != objectToSnap) return;

        candidate = obj;
        candidateInside = true;

        TrySnapIfPossible();
        SetHighlight(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsValidCandidate(other)) return;

        candidateInside = true;
        candidate = other.gameObject;

        UpdateHighlight();
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsValidCandidate(other)) return;

        candidateInside = true;
        candidate = other.gameObject;

        UpdateHighlight();
    }

    void OnTriggerExit(Collider other)
    {
        if (candidate != null && other.gameObject == candidate)
        {
            candidateInside = false;
            candidate = null;
        }

        SetHighlight(false);
    }

    bool IsValidCandidate(Collider other)
    {
        if (other == null) return false;

        if (objectToSnap != null && other.gameObject != objectToSnap)
            return false;

        return true;
    }

    void UpdateHighlight()
    {
        if (!isObjectGrabbed || candidate == null || plugTip == null)
        {
            SetHighlight(false);
            return;
        }

        // 🔥 on ignore le trigger !
        float d = Vector3.Distance(plugTip.position, snapPoint.position);

        // 🔥 allumer seulement si très proche
        if (d <= snapRange)
        {
            SetHighlight(true);
        }
        else
        {
            SetHighlight(false);
        }
    }

    void TrySnapIfPossible()
    {
        if (candidate == null || plugTip == null) return;

        float d = Vector3.Distance(plugTip.position, snapPoint.position);
        UnityEngine.Debug.Log("Distance = " + d);

        if (d > snapRange)
        {
            UnityEngine.Debug.Log("Too far");
            return;
        }

        Rigidbody rb = candidate.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = snapPoint.position;
            rb.rotation = snapPoint.rotation;

            if (disableGravityAfterSnap)
                rb.useGravity = false;

            if (lockObjectAfterSnap)
            {
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
        else
        {
            candidate.transform.position = snapPoint.position;
            candidate.transform.rotation = snapPoint.rotation;
        }

        UnityEngine.Debug.Log("SNAP SUCCESS");

        if (chargingDashboard != null)
        {
            UnityEngine.Debug.Log("START CHARGING");
            chargingDashboard.StartCharging();
        }
        else
        {
            UnityEngine.Debug.LogError("chargingDashboard NULL");
        }
    }

    void SetHighlight(bool on)
    {
        if (zoneRenderer == null) return;

        zoneRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(EmissionId, on ? (highlightColor * emissionIntensity) : Color.black);
        zoneRenderer.SetPropertyBlock(mpb);
    }
}