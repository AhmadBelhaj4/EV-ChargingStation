using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public static QuestUI Instance;

    [Header("Press Tab to show/hide")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    // Built automatically — no prefab needed
    private GameObject _questPanel;
    private Transform _questListParent;
    private GameObject _completionBanner;
    private TextMeshProUGUI _bannerText;
    private bool _panelVisible = false;  // starts hidden until cutscene ends

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        BuildUI();
        QuestManager.Instance?.GetQuests();
        RefreshUI(QuestManager.Instance?.GetQuests() ?? new List<Quest>());

        // Hide quest panel until cutscene ends
        _questPanel.SetActive(false);
    }

    void Update()
    {
        // Only allow toggle after panel has been shown at least once
        if (_panelVisible && Input.GetKeyDown(toggleKey))
        {
            bool active = !_questPanel.activeSelf;
            _questPanel.SetActive(active);
        }
    }

    /// <summary>
    /// Call this when the cutscene ends to reveal the quest panel.
    /// Example: QuestUI.Instance.ShowPanel();
    /// </summary>
    public void ShowPanel()
    {
        _panelVisible = true;
        _questPanel.SetActive(true);
        RefreshUI(QuestManager.Instance?.GetQuests() ?? new List<Quest>());
    }

    void BuildUI()
    {
        // ── Canvas ──
        GameObject canvasGO = new GameObject("QuestCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Quest Panel ──
        _questPanel = MakePanel(canvasGO.transform,
            new Vector2(300, 400),
            new Vector2(-160, -210),   // top-right anchor offset
            new Color(0f, 0f, 0f, 0.85f),
            TextAnchor.UpperRight
        );

        // Anchor to top-right
        RectTransform panelRT = _questPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1, 1);
        panelRT.anchorMax = new Vector2(1, 1);
        panelRT.pivot = new Vector2(1, 1);
        panelRT.anchoredPosition = new Vector2(-20, -20);
        panelRT.sizeDelta = new Vector2(300, 400);

        // Panel Title
        GameObject titleGO = MakeText(_questPanel.transform, "   QUESTS",
            18, FontStyles.Bold, Color.white);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -10);
        titleRT.sizeDelta = new Vector2(0, 40);

        // Divider line
        GameObject divider = new GameObject("Divider");
        divider.transform.SetParent(_questPanel.transform, false);
        Image divImg = divider.AddComponent<Image>();
        divImg.color = new Color(1f, 1f, 1f, 0.2f);
        RectTransform divRT = divider.GetComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0, 1);
        divRT.anchorMax = new Vector2(1, 1);
        divRT.pivot = new Vector2(0.5f, 1);
        divRT.anchoredPosition = new Vector2(0, -52);
        divRT.sizeDelta = new Vector2(-20, 2);

        // ── Scroll list parent ──
        GameObject listGO = new GameObject("QuestListParent");
        listGO.transform.SetParent(_questPanel.transform, false);
        RectTransform listRT = listGO.AddComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0, 0);
        listRT.anchorMax = new Vector2(1, 1);
        listRT.offsetMin = new Vector2(10, 10);
        listRT.offsetMax = new Vector2(-10, -60);
        VerticalLayoutGroup vlg = listGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = false;
        vlg.padding = new RectOffset(0, 0, 5, 5);
        _questListParent = listGO.transform;

        // ── Completion Banner ──
        _completionBanner = MakePanel(canvasGO.transform,
            new Vector2(420, 80),
            Vector2.zero,
            new Color(0.05f, 0.5f, 0.1f, 0.95f)
        );
        RectTransform bannerRT = _completionBanner.GetComponent<RectTransform>();
        bannerRT.anchorMin = new Vector2(0.5f, 0);
        bannerRT.anchorMax = new Vector2(0.5f, 0);
        bannerRT.pivot = new Vector2(0.5f, 0);
        bannerRT.anchoredPosition = new Vector2(0, 80);
        bannerRT.sizeDelta = new Vector2(420, 70);

        // Banner text
        GameObject bannerTextGO = MakeText(_completionBanner.transform,
            "✓ Quest Complete!", 20, FontStyles.Bold, Color.white);
        RectTransform btRT = bannerTextGO.GetComponent<RectTransform>();
        btRT.anchorMin = Vector2.zero;
        btRT.anchorMax = Vector2.one;
        btRT.offsetMin = Vector2.zero;
        btRT.offsetMax = Vector2.zero;
        _bannerText = bannerTextGO.GetComponent<TextMeshProUGUI>();
        _bannerText.alignment = TextAlignmentOptions.Center;

        _completionBanner.SetActive(false);
    }

    public void RefreshUI(List<Quest> quests)
    {
        if (_questListParent == null) return;

        // Clear old entries
        foreach (Transform child in _questListParent)
            Destroy(child.gameObject);

        // Create one row per visible quest
        foreach (Quest q in quests)
        {
            if (q.status == QuestStatus.Locked) continue;
            CreateQuestEntry(q);
        }
    }

    void CreateQuestEntry(Quest q)
    {
        // Row background
        GameObject row = new GameObject($"Quest_{q.id}");
        row.transform.SetParent(_questListParent, false);
        Image rowImg = row.AddComponent<Image>();
        rowImg.color = q.status == QuestStatus.Completed
            ? new Color(0.1f, 0.4f, 0.1f, 0.6f)   // green tint
            : new Color(0.15f, 0.15f, 0.15f, 0.8f); // dark grey
        RectTransform rowRT = row.GetComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 75);

        // Status dot
        GameObject dot = new GameObject("Dot");
        dot.transform.SetParent(row.transform, false);
        Image dotImg = dot.AddComponent<Image>();
        dotImg.color = q.status == QuestStatus.Completed
            ? new Color(0.2f, 1f, 0.4f)    // green
            : new Color(1f, 0.85f, 0.1f);  // yellow
        RectTransform dotRT = dot.GetComponent<RectTransform>();
        dotRT.anchorMin = new Vector2(0, 0.5f);
        dotRT.anchorMax = new Vector2(0, 0.5f);
        dotRT.pivot = new Vector2(0, 0.5f);
        dotRT.anchoredPosition = new Vector2(10, 0);
        dotRT.sizeDelta = new Vector2(12, 12);

        // Title
        GameObject titleGO = MakeText(row.transform, q.title,
            15, FontStyles.Bold, Color.white);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0, 1);
        titleRT.anchoredPosition = new Vector2(30, -8);
        titleRT.sizeDelta = new Vector2(-40, 22);

        // Description
        GameObject descGO = MakeText(row.transform, q.description,
            11, FontStyles.Normal, new Color(0.8f, 0.8f, 0.8f));
        RectTransform descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0, 0.5f);
        descRT.anchorMax = new Vector2(1, 0.5f);
        descRT.pivot = new Vector2(0, 0.5f);
        descRT.anchoredPosition = new Vector2(30, 0);
        descRT.sizeDelta = new Vector2(-40, 20);

        // Progress
        string progressStr = q.status == QuestStatus.Completed
            ? "✓ Done"
            : q.ProgressText;
        Color progressColor = q.status == QuestStatus.Completed
            ? new Color(0.2f, 1f, 0.4f)
            : new Color(1f, 0.85f, 0.1f);
        GameObject progGO = MakeText(row.transform, progressStr,
            13, FontStyles.Bold, progressColor);
        TextMeshProUGUI progTMP = progGO.GetComponent<TextMeshProUGUI>();
        progTMP.alignment = TextAlignmentOptions.BottomRight;
        RectTransform progRT = progGO.GetComponent<RectTransform>();
        progRT.anchorMin = new Vector2(0, 0);
        progRT.anchorMax = new Vector2(1, 0);
        progRT.pivot = new Vector2(1, 0);
        progRT.anchoredPosition = new Vector2(-10, 8);
        progRT.sizeDelta = new Vector2(-10, 20);
    }

    public void ShowCompletionBanner(string questTitle)
    {
        StartCoroutine(BannerRoutine(questTitle));
    }

    IEnumerator BannerRoutine(string questTitle)
    {
        _bannerText.text = $"✓  Quest Complete!\n<size=14>{questTitle}</size>";
        _completionBanner.SetActive(true);
        yield return new WaitForSeconds(3f);
        _completionBanner.SetActive(false);
    }

    // ── Helpers ──

    GameObject MakePanel(Transform parent, Vector2 size, Vector2 pos,
        Color color, TextAnchor anchor = TextAnchor.UpperLeft)
    {
        GameObject go = new GameObject("Panel");
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return go;
    }

    GameObject MakeText(Transform parent, string text, int fontSize,
        FontStyles style, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        go.AddComponent<RectTransform>();
        return go;
    }
}