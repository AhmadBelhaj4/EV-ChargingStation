using UnityEngine;

/// <summary>
/// Attach to the welding tool model (child of Camera or player hand).
/// Handles the tool's own light and sparks while firing.
/// </summary>
public class WeldingTool : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private Light toolLight;           // light at the tip of the tool
    [SerializeField] private ParticleSystem toolSparks; // sparks at the tip of the tool
    [SerializeField] private Transform raycastOrigin;   // tip of the tool (empty GO at tip)

    [Header("Tool Light Settings")]
    [SerializeField] private Color lightColor = new Color(1f, 0.7f, 0.2f);
    [SerializeField] private float lightIntensity = 5f;
    [SerializeField] private float lightRange = 1.2f;
    [SerializeField] private float flickerSpeed = 12f;
    [SerializeField] private float flickerAmount = 0.25f;

    [Header("Raycast Settings")]
    [SerializeField] private float weldRange = 5f;
    [SerializeField] private LayerMask weldableLayer;

    private bool _isFiring = false;

    /// <summary>
    /// Data returned by Fire() — includes the hit hole and surface info for effect placement.
    /// </summary>
    public struct WeldHitResult
    {
        public WeldableHole hole;
        public Vector3 point;
        public Vector3 normal;
        public bool didHit;
    }

    void Start()
    {
        // Tool light and sparks start OFF
        if (toolLight != null)
            toolLight.gameObject.SetActive(false);

        if (toolSparks != null)
            toolSparks.Stop();
    }

    void Update()
    {
        if (_isFiring)
        {
            // Flicker the tool light
            if (toolLight != null)
            {
                toolLight.gameObject.SetActive(true);
                float flicker = 1f
                    + Mathf.Sin(Time.time * flickerSpeed) * flickerAmount
                    + Mathf.Sin(Time.time * flickerSpeed * 2.3f) * flickerAmount * 0.4f;
                toolLight.color = lightColor;
                toolLight.intensity = lightIntensity * flicker;
                toolLight.range = lightRange;
            }

            // Sparks on
            if (toolSparks != null && !toolSparks.isPlaying)
                toolSparks.Play();
        }
        else
        {
            // Instant OFF
            if (toolLight != null)
                toolLight.gameObject.SetActive(false);

            if (toolSparks != null && toolSparks.isPlaying)
                toolSparks.Stop();
        }
    }

    /// <summary>
    /// Cast a ray from screen center. Returns hit info if a WeldableHole was hit.
    /// Uses weldableLayer mask first, then falls back to unfiltered raycast.
    /// </summary>
    public WeldHitResult Fire()
    {
        _isFiring = true;

        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * weldRange, Color.red);

        WeldHitResult result = default;

        // Try with layer mask first (avoids hitting the pipe collider before the hole)
        if (weldableLayer.value != 0 && Physics.Raycast(ray, out RaycastHit hit, weldRange, weldableLayer))
        {
            WeldableHole hole = hit.collider.GetComponent<WeldableHole>();
            if (hole == null) hole = hit.collider.GetComponentInParent<WeldableHole>();

            if (hole != null)
            {
                result.didHit = true;
                result.hole = hole;
                result.point = hit.point;
                result.normal = hit.normal;
                return result;
            }
        }

        // Fallback: RaycastAll so we pass through the table surface and find holes
        RaycastHit[] allHits = Physics.RaycastAll(ray, weldRange);
        if (allHits.Length > 0)
        {
            // Sort by distance so we check the closest weldable hole first
            System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit h in allHits)
            {
                WeldableHole hole = h.collider.GetComponent<WeldableHole>();
                if (hole == null) hole = h.collider.GetComponentInParent<WeldableHole>();

                if (hole != null)
                {
                    result.didHit = true;
                    result.hole   = hole;
                    result.point  = h.point;
                    result.normal = h.normal;
                    return result;
                }
            }

            Debug.Log($"RaycastAll hit {allHits.Length} objects but none had WeldableHole (first: '{allHits[0].collider.gameObject.name}')");
        }
        else
        {
            Debug.Log("Raycast hit NOTHING");
        }

        return result;
    }

    public void StopFiring()
    {
        _isFiring = false;
    }
}
