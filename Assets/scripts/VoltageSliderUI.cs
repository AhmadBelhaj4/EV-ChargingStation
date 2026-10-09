using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Attach this to the same GameObject as CoolingSystem.
/// It programmatically builds a premium-looking voltage slider panel
/// on the Canvas assigned in the Inspector (or auto-finds one).
/// 
/// It replaces the need for manually styling the slider panel —
/// just assign the references and it handles layout, colors, animations.
/// </summary>
public class VoltageSliderUI : MonoBehaviour
{
    [Header("Canvas (auto-found if empty)")]
    public Canvas targetCanvas;

    [Header("Panel Settings")]
    public Vector2 panelSize = new Vector2(420f, 260f);
    public Vector2 panelAnchorPosition = new Vector2(0f, -80f); // offset from center-top

    [Header("Colors")]
    public Color panelBackground = new Color(0.08f, 0.08f, 0.12f, 0.92f);
    public Color accentColor = new Color(0.2f, 0.6f, 1f);
    public Color warningColor = new Color(1f, 0.3f, 0.15f);
    public Color sliderTrackColor = new Color(0.15f, 0.15f, 0.22f);
    public Color textColor = new Color(0.9f, 0.92f, 0.95f);

    [Header("References (auto-wired to CoolingSystem)")]
    public CoolingSystem coolingSystem;

    // Built UI references
    private GameObject _panel;
    private Slider _slider;
    private TextMeshProUGUI _voltageText;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _statusText;
    private Image _panelBg;
    private Image _fillImage;
    private Image _handleImage;
    private Image _glowBar;
    private RectTransform _panelRect;

    // Animation
    private Coroutine _showHideRoutine;
    private CanvasGroup _canvasGroup;
    private bool _isVisible;

    void Start()
    {
        if (coolingSystem == null)
            coolingSystem = GetComponent<CoolingSystem>();

        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (targetCanvas == null || coolingSystem == null)
        {
            Debug.LogError("[VoltageSliderUI] Missing Canvas or CoolingSystem reference!");
            return;
        }

        BuildUI();
        WireToCoolingSystem();
        HideImmediate();
    }

    // ── Build the entire UI hierarchy ──────────────────────────────────────

    void BuildUI()
    {
        // ── Root Panel ──
        _panel = CreateUIObject("VoltagePanel", targetCanvas.transform);
        _panelRect = _panel.GetComponent<RectTransform>();
        _panelRect.sizeDelta = panelSize;
        _panelRect.anchorMin = new Vector2(0f, 1f);
        _panelRect.anchorMax = new Vector2(0f, 1f);
        _panelRect.pivot = new Vector2(0f, 1f);
        _panelRect.anchoredPosition = new Vector2(20f, -150f);

        _panelBg = _panel.AddComponent<Image>();
        _panelBg.color = panelBackground;

        // Rounded look — use a soft sprite if available, otherwise solid
        _panelBg.type = Image.Type.Sliced;

        _canvasGroup = _panel.AddComponent<CanvasGroup>();

        // Add vertical layout
        var layout = _panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 20, 20);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // ── Title Row ──
        var titleRow = CreateUIObject("TitleRow", _panel.transform);
        var titleRowRect = titleRow.GetComponent<RectTransform>();
        titleRowRect.sizeDelta = new Vector2(0f, 32f);
        var titleRowLayout = titleRow.AddComponent<HorizontalLayoutGroup>();
        titleRowLayout.childAlignment = TextAnchor.MiddleLeft;
        titleRowLayout.childControlWidth = true;
        titleRowLayout.childControlHeight = true;
        titleRowLayout.childForceExpandWidth = false;
        titleRowLayout.spacing = 8f;

        // Icon (⚡)
        var iconObj = CreateUIObject("Icon", titleRow.transform);
        _titleText = CreateTMP(iconObj, "⚡  VOLTAGE CONTROL", 18, FontStyles.Bold, accentColor);
        var titleLayout = iconObj.AddComponent<LayoutElement>();
        titleLayout.flexibleWidth = 1f;

        // ── Divider ──
        var divider = CreateUIObject("Divider", _panel.transform);
        var divRect = divider.GetComponent<RectTransform>();
        divRect.sizeDelta = new Vector2(0f, 2f);
        var divImg = divider.AddComponent<Image>();
        divImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.3f);
        var divLayout = divider.AddComponent<LayoutElement>();
        divLayout.preferredHeight = 2f;

        // ── Voltage Display ──
        var voltageRow = CreateUIObject("VoltageRow", _panel.transform);
        var voltageRowRect = voltageRow.GetComponent<RectTransform>();
        voltageRowRect.sizeDelta = new Vector2(0f, 50f);
        var voltageRowLayout = voltageRow.AddComponent<LayoutElement>();
        voltageRowLayout.preferredHeight = 50f;

        _voltageText = CreateTMP(voltageRow, "0 V", 42, FontStyles.Bold, textColor);
        _voltageText.alignment = TextAlignmentOptions.Center;

        // ── Slider Area ──
        var sliderArea = CreateUIObject("SliderArea", _panel.transform);
        var sliderAreaRect = sliderArea.GetComponent<RectTransform>();
        sliderAreaRect.sizeDelta = new Vector2(0f, 40f);
        var sliderAreaLayout = sliderArea.AddComponent<LayoutElement>();
        sliderAreaLayout.preferredHeight = 40f;

        BuildSlider(sliderArea.transform);

        // ── Glow bar (color preview strip under slider) ──
        var glowBarObj = CreateUIObject("GlowBar", _panel.transform);
        var glowBarRect = glowBarObj.GetComponent<RectTransform>();
        glowBarRect.sizeDelta = new Vector2(0f, 6f);
        _glowBar = glowBarObj.AddComponent<Image>();
        _glowBar.color = accentColor;
        var glowLayout = glowBarObj.AddComponent<LayoutElement>();
        glowLayout.preferredHeight = 6f;

        // ── Status Text ──
        var statusRow = CreateUIObject("StatusRow", _panel.transform);
        var statusRowRect = statusRow.GetComponent<RectTransform>();
        statusRowRect.sizeDelta = new Vector2(0f, 24f);
        var statusRowLayout = statusRow.AddComponent<LayoutElement>();
        statusRowLayout.preferredHeight = 24f;

        _statusText = CreateTMP(statusRow, "STATUS: IDLE", 13, FontStyles.Normal,
            new Color(textColor.r, textColor.g, textColor.b, 0.6f));
        _statusText.alignment = TextAlignmentOptions.Center;

        // ── Min/Max labels ──
        var labelsRow = CreateUIObject("LabelsRow", _panel.transform);
        var labelsRect = labelsRow.GetComponent<RectTransform>();
        labelsRect.sizeDelta = new Vector2(0f, 18f);
        var labelsLayout = labelsRow.AddComponent<LayoutElement>();
        labelsLayout.preferredHeight = 18f;

        var labelsHLayout = labelsRow.AddComponent<HorizontalLayoutGroup>();
        labelsHLayout.childAlignment = TextAnchor.MiddleCenter;
        labelsHLayout.childControlWidth = true;
        labelsHLayout.childForceExpandWidth = true;

        var minObj = CreateUIObject("MinLabel", labelsRow.transform);
        var minTMP = CreateTMP(minObj, "0V", 11, FontStyles.Normal, new Color(0.5f, 0.5f, 0.6f));
        minTMP.alignment = TextAlignmentOptions.Left;

        var maxObj = CreateUIObject("MaxLabel", labelsRow.transform);
        var maxTMP = CreateTMP(maxObj, "100V", 11, FontStyles.Normal, new Color(0.5f, 0.5f, 0.6f));
        maxTMP.alignment = TextAlignmentOptions.Right;
    }

    void BuildSlider(Transform parent)
    {
        // ── Slider root ──
        var sliderObj = CreateUIObject("VoltageSlider", parent);
        var sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = new Vector2(8f, 8f);
        sliderRect.offsetMax = new Vector2(-8f, -8f);

        _slider = sliderObj.AddComponent<Slider>();
        _slider.minValue = 0f;
        _slider.maxValue = 100f;
        _slider.wholeNumbers = false;
        _slider.direction = Slider.Direction.LeftToRight;

        // ── Background track ──
        var bgTrack = CreateUIObject("Background", sliderObj.transform);
        var bgRect = bgTrack.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.35f);
        bgRect.anchorMax = new Vector2(1f, 0.65f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImg = bgTrack.AddComponent<Image>();
        bgImg.color = sliderTrackColor;

        // ── Fill area ──
        var fillArea = CreateUIObject("Fill Area", sliderObj.transform);
        var fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.35f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.65f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        var fill = CreateUIObject("Fill", fillArea.transform);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0f, 1f); // slider controls max.x
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        _fillImage = fill.AddComponent<Image>();
        _fillImage.color = accentColor;

        // ── Handle area ──
        var handleArea = CreateUIObject("Handle Slide Area", sliderObj.transform);
        var handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0f, 0f);
        handleAreaRect.anchorMax = new Vector2(1f, 1f);
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        var handle = CreateUIObject("Handle", handleArea.transform);
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(26f, 26f);

        _handleImage = handle.AddComponent<Image>();
        _handleImage.color = Color.white;

        // Wire slider references
        _slider.fillRect = fillRect;
        _slider.handleRect = handleRect;
        _slider.targetGraphic = _handleImage;

        // Slider color block
        var colors = _slider.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.85f, 0.9f, 1f);
        colors.pressedColor = accentColor;
        colors.selectedColor = new Color(0.85f, 0.9f, 1f);
        _slider.colors = colors;
    }

    // ── Wire to CoolingSystem ──────────────────────────────────────────────

    void WireToCoolingSystem()
    {
        // Replace CoolingSystem's references with our new UI
        coolingSystem.voltageSlider = _slider;
        coolingSystem.voltageLabel = _voltageText;
        coolingSystem.sliderPanel = _panel;

        // Re-setup slider in CoolingSystem
        _slider.minValue = 0f;
        _slider.maxValue = 100f;
        _slider.value = coolingSystem.voltage;

        // Re-bind CoolingSystem's listener to the NEW slider
        coolingSystem.RebindSlider();

        // Listen for value changes to update our enhanced visuals
        _slider.onValueChanged.AddListener(OnVoltageChanged);
    }

    void OnVoltageChanged(float value)
    {
        // Update voltage text with formatting
        _voltageText.text = $"{value:0} V";

        // Compute liquid color for the glow bar preview
        Color liquidColor;
        if (value < 50f)
            liquidColor = Color.Lerp(
                new Color(0.2f, 0.6f, 1f),   // cold blue
                new Color(1f, 0.6f, 0.1f),    // warm orange
                value / 50f);
        else
            liquidColor = Color.Lerp(
                new Color(1f, 0.6f, 0.1f),    // warm orange
                new Color(1f, 0.15f, 0.05f),  // hot red
                (value - 50f) / 50f);

        // Update fill color to match liquid
        if (_fillImage != null)
            _fillImage.color = liquidColor;

        // Update glow bar
        if (_glowBar != null)
            _glowBar.color = liquidColor;

        // Update status text
        if (_statusText != null)
        {
            if (value <= 0f)
            {
                _statusText.text = "STATUS: IDLE";
                _statusText.color = new Color(textColor.r, textColor.g, textColor.b, 0.5f);
            }
            else if (value < coolingSystem.warningThreshold)
            {
                _statusText.text = "STATUS: OPERATING";
                _statusText.color = new Color(0.3f, 0.9f, 0.4f);
            }
            else
            {
                _statusText.text = "⚠ WARNING: HIGH VOLTAGE";
                _statusText.color = warningColor;
            }
        }

        // Tint title on warning
        if (_titleText != null)
        {
            // The title icon/text is inside the child
        }
    }

    // ── Show / Hide with fade animation ────────────────────────────────────

    public void Show()
    {
        if (_panel == null) return;
        _panel.SetActive(true);

        if (_showHideRoutine != null)
            StopCoroutine(_showHideRoutine);
        _showHideRoutine = StartCoroutine(AnimatePanel(true));
        _isVisible = true;
    }

    public void Hide()
    {
        if (_panel == null) return;

        if (_showHideRoutine != null)
            StopCoroutine(_showHideRoutine);
        _showHideRoutine = StartCoroutine(AnimatePanel(false));
        _isVisible = false;
    }

    void HideImmediate()
    {
        if (_panel == null) return;
        _canvasGroup.alpha = 0f;
        _panelRect.anchoredPosition = panelAnchorPosition + new Vector2(0f, 30f);
        _panel.SetActive(false);
        _isVisible = false;
    }

    IEnumerator AnimatePanel(bool show)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        float startAlpha = _canvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;

         Vector2 hiddenPos = new Vector2(-panelSize.x, -150f);  // slides out to the left
        Vector2 shownPos  = new Vector2(20f, -150f);
/*
        Vector2 hiddenPos = panelAnchorPosition + new Vector2(0f, 30f);
        Vector2 shownPos = panelAnchorPosition;
        */

        Vector2 startPos = _panelRect.anchoredPosition;
        Vector2 endPos = show ? shownPos : hiddenPos;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            _panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        _canvasGroup.alpha = endAlpha;
        _panelRect.anchoredPosition = endPos;

        if (!show)
            _panel.SetActive(false);

        _showHideRoutine = null;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static TextMeshProUGUI CreateTMP(GameObject parent, string text, float fontSize,
        FontStyles style, Color color)
    {
        var tmp = parent.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.alignment = TextAlignmentOptions.Left;
        return tmp;
    }
}
