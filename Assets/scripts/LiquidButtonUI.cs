using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Attach to the same GameObject as PipeLiquidController.
/// Programmatically builds a premium liquid-control panel with
/// animated show/hide, a fill progress bar, and styled buttons.
/// </summary>
public class LiquidButtonUI : MonoBehaviour
{
    [Header("Canvas (auto-found if empty)")]
    public Canvas targetCanvas;

    [Header("Panel Settings")]
    public Vector2 panelSize = new Vector2(380f, 300f);
    public Vector2 panelAnchorPosition = new Vector2(0f, -80f);

    [Header("Colors")]
    public Color panelBackground = new Color(0.08f, 0.08f, 0.12f, 0.92f);
    public Color accentColor = new Color(0.2f, 0.65f, 1f);
    public Color removeColor = new Color(0.9f, 0.25f, 0.2f);
    public Color addColor = new Color(0.15f, 0.75f, 0.4f);
    public Color trackColor = new Color(0.15f, 0.15f, 0.22f);
    public Color textColor = new Color(0.9f, 0.92f, 0.95f);

    [Header("References")]
    public PipeLiquidController pipeLiquidController;

    // Built UI
    private GameObject _panel;
    private Button _removeBtn;
    private Button _addBtn;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _statusText;
    private TextMeshProUGUI _fillPercentText;
    private Image _fillBar;
    private Image _fillTrack;
    private RectTransform _fillBarRect;
    private RectTransform _panelRect;
    private CanvasGroup _canvasGroup;
    private Coroutine _showHideRoutine;
    private bool _isVisible;
    private bool _playerIsNear;

    void Start()
    {
        if (pipeLiquidController == null)
            pipeLiquidController = GetComponent<PipeLiquidController>();

        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (targetCanvas == null || pipeLiquidController == null)
        {
            Debug.LogError("[LiquidButtonUI] Missing Canvas or PipeLiquidController!");
            return;
        }

        BuildUI();
        HideImmediate();
    }

    void Update()
    {
        if (_playerIsNear && pipeLiquidController != null)
        {
            float fill = pipeLiquidController.GetCurrentFill();
            UpdateVisuals(fill);
        }
    }

    // ── Build UI ───────────────────────────────────────────────────────────

    void BuildUI()
    {
        // ── Root Panel ──
        _panel = CreateObj("LiquidPanel", targetCanvas.transform);
        _panelRect = _panel.GetComponent<RectTransform>();
        _panelRect.sizeDelta = panelSize;
        _panelRect.anchorMin = new Vector2(0f, 1f);
        _panelRect.anchorMax = new Vector2(0f, 1f);
        _panelRect.pivot = new Vector2(0f, 1f);
        _panelRect.anchoredPosition = new Vector2(20f, -150f);

        var bg = _panel.AddComponent<Image>();
        bg.color = panelBackground;
        bg.type = Image.Type.Sliced;

        _canvasGroup = _panel.AddComponent<CanvasGroup>();

        var layout = _panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 20, 20);
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // ── Title ──
        var titleRow = CreateObj("TitleRow", _panel.transform);
        titleRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);
        var titleLE = titleRow.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 30f;
        _titleText = MakeTMP(titleRow, "   LIQUID CONTROL", 18, FontStyles.Bold, accentColor);

        /*
        // ── Divider (thin line) ──
        var divider = CreateObj("Divider", _panel.transform);
        var divImg = divider.AddComponent<Image>();
        divImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.3f);
        var divLE = divider.AddComponent<LayoutElement>();
        divLE.preferredHeight = 1f;
        divLE.minHeight = 1f;
        divLE.flexibleHeight = 0f;
        */

        // ── Fill percentage ──
        var fillRow = CreateObj("FillRow", _panel.transform);
        fillRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 44f);
        var fillRowLE = fillRow.AddComponent<LayoutElement>();
        fillRowLE.preferredHeight = 44f;
        _fillPercentText = MakeTMP(fillRow, "100%", 38, FontStyles.Bold, textColor);
        _fillPercentText.alignment = TextAlignmentOptions.Center;

        // ── Progress bar (standard track + fill) ──
        var barContainer = CreateObj("BarContainer", _panel.transform);
        barContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 14f);
        var barLE = barContainer.AddComponent<LayoutElement>();
        barLE.preferredHeight = 14f;
        barLE.minHeight = 14f;
        barLE.flexibleHeight = 0f;

        // Track background
        _fillTrack = barContainer.AddComponent<Image>();
        _fillTrack.color = trackColor;

        // Fill bar (anchored — right anchor X controls the fill level)
        var fillBarObj = CreateObj("FillBar", barContainer.transform);
        _fillBarRect = fillBarObj.GetComponent<RectTransform>();
        _fillBarRect.anchorMin = Vector2.zero;
        _fillBarRect.anchorMax = Vector2.one;  // will be adjusted in UpdateVisuals
        _fillBarRect.offsetMin = new Vector2(2f, 2f);
        _fillBarRect.offsetMax = new Vector2(-2f, -2f);

        _fillBar = fillBarObj.AddComponent<Image>();
        _fillBar.color = accentColor;

        // ── Min / Max labels ──
        var labelsRow = CreateObj("Labels", _panel.transform);
        labelsRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 16f);
        var labelsLE = labelsRow.AddComponent<LayoutElement>();
        labelsLE.preferredHeight = 16f;
        var labelsHL = labelsRow.AddComponent<HorizontalLayoutGroup>();
        labelsHL.childControlWidth = true;
        labelsHL.childForceExpandWidth = true;

        var emptyLabel = CreateObj("Empty", labelsRow.transform);
        var emptyTMP = MakeTMP(emptyLabel, "EMPTY", 11, FontStyles.Normal, new Color(0.5f, 0.5f, 0.6f));
        emptyTMP.alignment = TextAlignmentOptions.Left;

        var fullLabel = CreateObj("Full", labelsRow.transform);
        var fullTMP = MakeTMP(fullLabel, "FULL", 11, FontStyles.Normal, new Color(0.5f, 0.5f, 0.6f));
        fullTMP.alignment = TextAlignmentOptions.Right;

        // ── Buttons row ──
        var btnRow = CreateObj("ButtonRow", _panel.transform);
        btnRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 48f);
        var btnRowLE = btnRow.AddComponent<LayoutElement>();
        btnRowLE.preferredHeight = 48f;

        var btnHL = btnRow.AddComponent<HorizontalLayoutGroup>();
        btnHL.spacing = 16f;
        btnHL.childAlignment = TextAnchor.MiddleCenter;
        btnHL.childControlWidth = true;
        btnHL.childControlHeight = true;
        btnHL.childForceExpandWidth = true;
        btnHL.childForceExpandHeight = true;

        _removeBtn = BuildButton(btnRow.transform, "RemoveBtn", "DRAIN", removeColor);
        _addBtn = BuildButton(btnRow.transform, "AddBtn", "FILL", addColor);

        // Wire button clicks
        _removeBtn.onClick.AddListener(() =>
        {
            pipeLiquidController?.RemoveLiquid();
        });
        _addBtn.onClick.AddListener(() =>
        {
            pipeLiquidController?.AddLiquid();
        });

        // ── Status ──
        var statusRow = CreateObj("StatusRow", _panel.transform);
        statusRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);
        var statusLE = statusRow.AddComponent<LayoutElement>();
        statusLE.preferredHeight = 22f;
        _statusText = MakeTMP(statusRow, "STATUS: FULL", 12, FontStyles.Normal,
            new Color(textColor.r, textColor.g, textColor.b, 0.6f));
        _statusText.alignment = TextAlignmentOptions.Center;
    }

    Button BuildButton(Transform parent, string name, string label, Color color)
    {
        var btnObj = CreateObj(name, parent);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = color;

        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        colors.selectedColor = Color.white;
        btn.colors = colors;
        btn.targetGraphic = btnImg;

        // Label must be on a CHILD object — Image and TMP are both Graphic and can't share a GameObject
        var labelObj = CreateObj(name + "_Label", btnObj.transform);
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var tmp = MakeTMP(labelObj, label, 16, FontStyles.Bold, Color.white);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false; // let clicks pass through to the button

        return btn;
    }

    // ── Update visuals each frame ──────────────────────────────────────────

    void UpdateVisuals(float fill)
    {
        bool isFull = fill >= 0.99f;
        bool isEmpty = fill <= 0.01f;
        bool animating = !isFull && !isEmpty;

        // Progress bar — scale fill via anchor
        if (_fillBarRect != null)
        {
            Vector2 max = _fillBarRect.anchorMax;
            max.x = Mathf.Clamp01(fill);
            _fillBarRect.anchorMax = max;
            _fillBarRect.offsetMax = new Vector2(-2f, -2f);
        }

        // Fill percentage
        if (_fillPercentText != null)
            _fillPercentText.text = $"{Mathf.RoundToInt(fill * 100f)}%";

        // Bar color shifts with fill level
        if (_fillBar != null)
        {
            Color barColor = Color.Lerp(
                new Color(0.4f, 0.4f, 0.5f),  // empty grey
                accentColor,                     // full blue
                fill);
            _fillBar.color = barColor;
        }

        // Button visibility
        if (_removeBtn != null)
            _removeBtn.gameObject.SetActive(isFull);
        if (_addBtn != null)
            _addBtn.gameObject.SetActive(isEmpty && QuestManager.IsReturnPipeDone);

        // Status text
        if (_statusText != null)
        {
            if (animating)
            {
                _statusText.text = fill > 0.5f ? "STATUS: DRAINING..." : "STATUS: FILLING...";
                _statusText.color = new Color(1f, 0.8f, 0.2f);
            }
            else if (isFull)
            {
                _statusText.text = "STATUS: FULL";
                _statusText.color = new Color(0.3f, 0.9f, 0.5f);
            }
            else
            {
                _statusText.text = "STATUS: EMPTY";
                _statusText.color = new Color(0.6f, 0.6f, 0.7f);
            }
        }
    }

    // ── Show / Hide ────────────────────────────────────────────────────────

    public void Show()
    {
        if (_panel == null) return;
        _panel.SetActive(true);
        _playerIsNear = true;

        if (_showHideRoutine != null) StopCoroutine(_showHideRoutine);
        _showHideRoutine = StartCoroutine(AnimatePanel(true));
        _isVisible = true;
    }

    public void Hide()
    {
        if (_panel == null) return;
        _playerIsNear = false;

        if (_showHideRoutine != null) StopCoroutine(_showHideRoutine);
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
        _playerIsNear = false;
    }

    IEnumerator AnimatePanel(bool show)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        float startAlpha = _canvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;

        Vector2 hiddenPos = new Vector2(-panelSize.x, -150f);  // slides out to the left
        Vector2 shownPos  = new Vector2(20f, -150f);
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

        if (!show) _panel.SetActive(false);
        _showHideRoutine = null;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    static GameObject CreateObj(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static TextMeshProUGUI MakeTMP(GameObject parent, string text, float size,
        FontStyles style, Color color)
    {
        var tmp = parent.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.alignment = TextAlignmentOptions.Left;
        return tmp;
    }
}
