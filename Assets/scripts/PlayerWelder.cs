using UnityEngine;

public class PlayerWelder : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private WeldingTool weldingTool;

    [Header("Settings")]
    [SerializeField] private KeyCode weldKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode returnKey = KeyCode.E;

    [Header("Welding Sound")]
    public AudioClip weldSound;
    [Range(0f, 1f)] public float weldVolume = 0.6f;

    private WeldableHole _currentHole = null;
    private bool _hasTool = false;
    private AudioSource _weldAudio;

    // Reference to the pickup script (set when tool is equipped)
    private PickupableWeldingTool _pickupSource;

    void Start()
    {
        if (weldingTool != null)
            weldingTool.gameObject.SetActive(false);

        // Create AudioSource on the PLAYER (always active, no enable issues)
        _weldAudio = gameObject.AddComponent<AudioSource>();
        _weldAudio.clip = weldSound;
        _weldAudio.loop = true;
        _weldAudio.playOnAwake = false;
        _weldAudio.volume = weldVolume;
        _weldAudio.spatialBlend = 0f; // 2D so it's always audible

        if (weldSound == null)
            Debug.LogWarning("[PlayerWelder] weldSound is not assigned! Assign it in the Inspector.");
    }

    void Update()
    {
        if (!_hasTool) return;

        // ── Return tool on E press ──
        if (Input.GetKeyDown(returnKey))
        {
            ReturnToolToOrigin();
            return;
        }

        // ── Weld on left click ──
        if (Input.GetKey(weldKey))
        {
            WeldingTool.WeldHitResult hitResult = weldingTool.Fire();

            if (hitResult.didHit && hitResult.hole != null)
            {
                // Cool down previous hole if we switched targets
                if (_currentHole != null && _currentHole != hitResult.hole)
                    _currentHole.CoolDown();

                _currentHole = hitResult.hole;
                _currentHole.WeldThisFrame(Time.deltaTime, hitResult.point, hitResult.normal);
            }
            else
            {
                if (_currentHole != null)
                {
                    _currentHole.CoolDown();
                    _currentHole = null;
                }
            }

            // Play welding sound while firing
            if (_weldAudio != null && weldSound != null && !_weldAudio.isPlaying)
                _weldAudio.Play();
        }
        else
        {
            weldingTool.StopFiring();
            if (_currentHole != null)
            {
                _currentHole.CoolDown();
                _currentHole = null;
            }

            // Stop welding sound
            if (_weldAudio != null && _weldAudio.isPlaying)
                _weldAudio.Stop();
        }
    }

    public void EquipTool()
    {
        _hasTool = true;
        if (weldingTool != null)
            weldingTool.gameObject.SetActive(true);
        Debug.Log("Tool equipped!");
    }

    /// <summary>
    /// Called by PickupableWeldingTool so we know who to notify on return.
    /// </summary>
    public void EquipTool(PickupableWeldingTool source)
    {
        _pickupSource = source;
        EquipTool();
    }

    public void UnequipTool()
    {
        _hasTool = false;
        if (weldingTool != null)
            weldingTool.gameObject.SetActive(false);

        // Stop sound and cool down any active hole
        if (_weldAudio != null && _weldAudio.isPlaying)
            _weldAudio.Stop();

        if (_currentHole != null)
        {
            _currentHole.CoolDown();
            _currentHole = null;
        }

        Debug.Log("Tool unequipped!");
    }

    void ReturnToolToOrigin()
    {
        if (_pickupSource != null)
        {
            _pickupSource.ReturnFromPlayer();
        }
        else
        {
            // Fallback: just unequip
            UnequipTool();
        }
    }

    public bool HasTool => _hasTool;

    void OnGUI()
    {
        if (!_hasTool) return;

        string key = PromptUI.KeyName(returnKey);
        PromptUI.Draw(key, "Return Welding Tool",
            Color.white, new Color(0.4f, 0.75f, 1f));
    }
}