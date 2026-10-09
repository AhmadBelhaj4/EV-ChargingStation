using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CoolingSystem : MonoBehaviour
{
    [Header("Voltage")]
    [Range(0f, 100f)]
    public float voltage = 0f;
    public float warningThreshold = 75f;

    [Header("UI")]
    public Slider voltageSlider;
    public TextMeshProUGUI voltageLabel;
    public GameObject warningPanel;
    public GameObject sliderPanel;

    [Header("Color Change Delay")]
    [Tooltip("Seconds to wait before applying the new liquid color after the slider changes.")]
    public float colorChangeDelay = 0f;

    [Header("Cooling Pipe Renderers")]
    public Renderer[] coolingPipeRenderers;

    [Header("Liquid Flow Particles")]
    public ParticleSystem[] liquidFlowParticles;

private Color coldColor = new Color(0.1f, 0.5f, 1f);    // blue  = cold = 0 voltage
private Color warmColor = new Color(1f, 0.6f, 0.1f);    // orange = warm = 50 voltage
private Color hotColor  = new Color(1f, 0.1f, 0.05f); 

private static readonly int ColorID        = Shader.PropertyToID("_Color");
private static readonly int FresnelColorID = Shader.PropertyToID("_Fresnel_Color");
private static readonly int TopColorID     = Shader.PropertyToID("_Top_Color");
//private static readonly int BubblesColorID = Shader.PropertyToID("_Bubbles_Color");

private Color _currentColor;   // ← ADD THIS LINE

    // Each renderer gets ONE persistent MaterialPropertyBlock
    // This avoids creating material instances entirely — works on shared AND instanced materials
    private MaterialPropertyBlock[] blocks;
    private Coroutine _pendingColorChange;

/*    void Awake()
    {
        // Build one PropertyBlock per renderer
        blocks = new MaterialPropertyBlock[coolingPipeRenderers.Length];
        for (int i = 0; i < coolingPipeRenderers.Length; i++)
            blocks[i] = new MaterialPropertyBlock();
    }
*/
    void Awake()
    {
        blocks = new MaterialPropertyBlock[coolingPipeRenderers.Length];
        for (int i = 0; i < coolingPipeRenderers.Length; i++)
        {
            blocks[i] = new MaterialPropertyBlock();

        }
    }

    void Start()
    {
        SetSliderVisible(false);

        if (voltageSlider != null)
        {
            voltageSlider.minValue = 0f;
            voltageSlider.maxValue = 100f;
            voltageSlider.value = 0f;
            voltageSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    /// <summary>
    /// Called by VoltageSliderUI after it replaces the slider reference.
    /// Re-registers OnSliderChanged on the new slider.
    /// </summary>
    public void RebindSlider()
    {
        if (voltageSlider != null)
        {
            voltageSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    // ── Proximity ────────────────────────────────────────────────────────

    

    

    void SetSliderVisible(bool visible)
    {
        if (sliderPanel != null)
            sliderPanel.SetActive(visible);
        else if (voltageSlider != null)
            voltageSlider.gameObject.SetActive(visible);
    }

    // ── Slider ───────────────────────────────────────────────────────────

    void OnSliderChanged(float value)
    {
        voltage = value;

        Color targetColor;
        if (voltage < 50f)
            targetColor = Color.Lerp(coldColor, warmColor, voltage / 50f);
        else
            targetColor = Color.Lerp(warmColor, hotColor, (voltage - 50f) / 50f);

        // Cancel any pending color change so only the latest value applies
        if (_pendingColorChange != null)
            StopCoroutine(_pendingColorChange);

        if (colorChangeDelay > 0f)
            _pendingColorChange = StartCoroutine(DelayedApplyColor(targetColor));
        else
            ApplyColor(targetColor);

        if (voltageLabel != null)
            voltageLabel.text = $"{voltage:0} V";

        if (warningPanel != null)
            warningPanel.SetActive(voltage >= warningThreshold);

        if (voltage >= 100f)
        QuestManager.Instance?.UpdateQuest("test_voltage");
    }

    System.Collections.IEnumerator DelayedApplyColor(Color col)
    {
        yield return new WaitForSeconds(colorChangeDelay);
        ApplyColor(col);
        _pendingColorChange = null;
    }

    // ── Color via MaterialPropertyBlock — no material instances created ──

/*    void ApplyColor(Color col)
    {
        Color emissive = col * 0.8f;

        for (int i = 0; i < coolingPipeRenderers.Length; i++)
        {
            if (coolingPipeRenderers[i] == null) continue;

            // Get the current block state first (important!)
            coolingPipeRenderers[i].GetPropertyBlock(blocks[i]);

            blocks[i].SetColor(BaseColorID, col);
            blocks[i].SetColor(EmissionColorID, emissive);

            // Push it back — this is what actually changes the visual
            coolingPipeRenderers[i].SetPropertyBlock(blocks[i]);
        }
    }
*/

    void LateUpdate()
    {
        // Re-apply every frame to fight spline mesh regeneration overwriting the block
        if (_currentColor != default)
            ApplyColor(_currentColor);
    }

    void ApplyColor(Color col)
    {
        _currentColor = col;

        // The fresnel/top glow — make it brighter than the base color
        Color glowColor = col * 2.5f;

        for (int i = 0; i < coolingPipeRenderers.Length; i++)
        {
            if (coolingPipeRenderers[i] == null) continue;

            coolingPipeRenderers[i].GetPropertyBlock(blocks[i]);

            blocks[i].SetColor(ColorID,        col);        // main liquid color
            blocks[i].SetColor(FresnelColorID, glowColor);  // edge glow
            blocks[i].SetColor(TopColorID,     glowColor);  // top highlight
            // Bubbles color is optional — set it too for full effect
  

            coolingPipeRenderers[i].SetPropertyBlock(blocks[i]);
        }
    }

    // ── Called by BatterySystem after repair ─────────────────────────────

    public void ResetSystem()
    {
        voltage = 0f;
        if (voltageSlider != null) voltageSlider.value = 0f;
        foreach (var ps in liquidFlowParticles)
            ps?.Play();
        ApplyColor(coldColor);
    }
}
