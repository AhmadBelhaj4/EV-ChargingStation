using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WeldableHole : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float weldDuration = 3f;

    [Header("Glow Colors")]
    [SerializeField] private Color coldColor = new Color(0.8f, 0.1f, 0f);
    [SerializeField] private Color hotColor = new Color(1f, 0.9f, 0.3f);
    [SerializeField] private float maxEmissionIntensity = 12f;
    [SerializeField] private float glowPulseSpeed = 6f;
    [SerializeField] private float glowPulseAmount = 0.15f;

    [Header("Weld Light (auto-created if empty)")]
    [SerializeField] private Light weldLight;
    [SerializeField] private Color weldLightColor = new Color(1f, 0.7f, 0.2f);
    [SerializeField] private float weldLightIntensity = 4f;
    [SerializeField] private float weldLightRange = 2f;
    [SerializeField] private float lightFlickerSpeed = 10f;
    [SerializeField] private float lightFlickerAmount = 0.3f;

    [Header("Weld Sparks (auto-created if empty)")]
    [SerializeField] private ParticleSystem weldSparks;
    [SerializeField] private int sparkCount = 20;
    [SerializeField] private float sparkSpeed = 2.5f;
    [SerializeField] private float sparkLifetime = 0.5f;
    [SerializeField] private float sparkSize = 0.04f;

    private Renderer _renderer;
    private Material _mat;
    private float _weldProgress = 0f;
    private bool _isDone = false;

    void Start()
    {
        Debug.Log($"{gameObject.name} is on layer: {gameObject.layer} — '{LayerMask.LayerToName(gameObject.layer)}'");

        _renderer = GetComponent<Renderer>();
        _mat = new Material(_renderer.material);
        _renderer.material = _mat;
        _mat.EnableKeyword("_EMISSION");
        _mat.SetColor("_EmissionColor", Color.black);
        SetMaterialTransparent(_mat);

        // Warn if no collider (raycast won't work)
        if (GetComponent<Collider>() == null)
            Debug.LogError($"{gameObject.name}: No Collider! Add a MeshCollider or BoxCollider so raycasts can hit this hole.");

        // Auto-create point light if not assigned
        if (weldLight == null)
        {
            GameObject go = new GameObject("WeldLight");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            weldLight = go.AddComponent<Light>();
            weldLight.type = LightType.Point;
            weldLight.shadows = LightShadows.None;
        }
        weldLight.color = weldLightColor;
        weldLight.range = weldLightRange;
        weldLight.intensity = 0f;

        // Auto-create spark particle system if not assigned
        if (weldSparks == null)
        {
            GameObject go = new GameObject("WeldSparks");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            weldSparks = go.AddComponent<ParticleSystem>();
            BuildSparkSystem(weldSparks);
        }
        weldSparks.Stop();

        Debug.Log($"{gameObject.name} WeldableHole initialized");
    }

    /// <summary>
    /// Called by PlayerWelder every frame while aiming and holding fire.
    /// Accepts hit point and normal so effects are placed at the weld contact.
    /// </summary>
    public void WeldThisFrame(float deltaTime, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_isDone) return;

    // Only allow welding if pipe is placed on the grid
    GridPipe pipe = GetComponentInParent<GridPipe>();
    if (pipe != null && !pipe.IsOnGrid)
    {
        Debug.Log("Pipe must be on the grid to weld!");
        return;
    }

        _weldProgress += deltaTime / weldDuration;
        _weldProgress = Mathf.Clamp01(_weldProgress);
        Debug.Log($"{gameObject.name} progress: {_weldProgress:F2}");

        // ── Emission glow (pulses, transitions cold → hot) ──
        float pulse = 1f + Mathf.Sin(Time.time * glowPulseSpeed) * glowPulseAmount;
        float intensity = maxEmissionIntensity * pulse;
        Color glow = Color.Lerp(coldColor, hotColor, _weldProgress);
        _mat.SetColor("_EmissionColor", glow * intensity);

        // ── Gradually fade alpha (begins at 50 % progress) ──
        if (_weldProgress > 0.5f)
        {
            float fade = (_weldProgress - 0.5f) / 0.5f;
            SetAlpha(1f - fade);
        }

        // ── Point light at weld contact ──
        if (weldLight != null)
        {
            weldLight.transform.position = hitPoint + hitNormal * 0.05f;
            float flicker = 1f
                + Mathf.Sin(Time.time * lightFlickerSpeed) * lightFlickerAmount
                + Mathf.Sin(Time.time * lightFlickerSpeed * 2.7f) * lightFlickerAmount * 0.4f;
            weldLight.intensity = weldLightIntensity * flicker;
        }

        // ── Sparks at weld contact ──
        if (weldSparks != null)
        {
            weldSparks.transform.position = hitPoint;
            weldSparks.transform.rotation = Quaternion.LookRotation(hitNormal);
            if (!weldSparks.isPlaying)
                weldSparks.Play();
        }

        // ── Finished ──
        if (_weldProgress >= 1f)
            FinishWeld();
    }

    /// <summary>
    /// Backward-compatible overload — effects placed at hole center.
    /// </summary>
    public void WeldThisFrame(float deltaTime)
    {
        WeldThisFrame(deltaTime, transform.position, transform.up);
    }

    /// <summary>
    /// Called when the player stops aiming at this hole — turn off effects.
    /// </summary>
    public void CoolDown()
    {
        if (_isDone) return;
        _mat.SetColor("_EmissionColor", Color.black);

        if (weldLight != null)
            weldLight.intensity = 0f;

        if (weldSparks != null && weldSparks.isPlaying)
            weldSparks.Stop();
    }

    void FinishWeld()
    {
        _isDone = true;
        _mat.SetColor("_EmissionColor", Color.black);
        SetAlpha(0f);
        _renderer.enabled = false;

        if (weldLight != null) weldLight.intensity = 0f;
        if (weldSparks != null) weldSparks.Stop();

        // Reports to quest system
        QuestManager.Instance?.UpdateQuest("fix_pipes");
        QuestManager.Instance?.UpdateQuest("fix_holes_3");
        QuestManager.Instance?.UpdateQuest("fix_all");

        Debug.Log($"{gameObject.name} welded!");
    }

    void SetAlpha(float alpha)
    {
        Color c = _mat.color;
        c.a = alpha;
        _mat.color = c;
    }

    void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_ZWrite", 0f);
        mat.SetFloat("_AlphaClip", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    /// <summary>
    /// Builds a spark particle system procedurally — no prefab needed.
    /// </summary>
    void BuildSparkSystem(ParticleSystem ps)
    {
        var main = ps.main;
        main.startLifetime = sparkLifetime;
        main.startSpeed = sparkSpeed;
        main.startSize = sparkSize;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.85f, 0.3f), new Color(1f, 0.4f, 0f));
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.playOnAwake = false;
        main.maxParticles = 300;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = sparkCount;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 0.01f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.85f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
    }
}