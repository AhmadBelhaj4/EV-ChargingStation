using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Singleton timer that runs between cutscene end and last quest completion.
/// Displays a live timer HUD and a full-screen score panel when finished.
/// Attach to any persistent GameObject (e.g. QuestSystemGO).
/// </summary>
public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance;

    [Header("Font (optional)")]
    [SerializeField] private TMP_FontAsset customFont;

    [Header("Timer Display")]
    [SerializeField] private Color timerColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private int timerFontSize = 28;

    [Header("Score Panel")]
    [SerializeField] private Color panelColor = new Color(0.02f, 0.02f, 0.08f, 0.95f);
    [SerializeField] private Color accentColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color goldColor = new Color(1f, 0.85f, 0.1f);
    [SerializeField] private Color buttonColor = new Color(0.15f, 0.65f, 0.35f);

    [Header("Scene Navigation")]
    [Tooltip("Scene name to load when clicking Return to Menu")]
    [SerializeField] private string returnSceneName = "MainMenu";

    // State
    private bool _isRunning = false;
    private bool _isFinished = false;
    private float _elapsedTime = 0f;

    // UI references (built at runtime)
    private Canvas _canvas;
    private GameObject _timerHUD;
    private TextMeshProUGUI _timerText;
    private GameObject _scorePanel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        BuildTimerHUD();
        BuildScorePanel();
    }

    void Update()
    {
        if (_isRunning && !_isFinished)
        {
            _elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    // ─────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────

    /// <summary>
    /// Call this when the cutscene ends to start the timer.
    /// </summary>
    public void StartTimer()
    {
        if (_isRunning || _isFinished) return;

        _isRunning = true;
        _elapsedTime = 0f;

        if (_timerHUD != null)
            _timerHUD.SetActive(true);

        Debug.Log("⏱ Game timer started!");
    }

    /// <summary>
    /// Call this when all quests are completed to stop the timer and show the score.
    /// </summary>
    public void StopTimer()
    {
        if (!_isRunning || _isFinished) return;

        _isRunning = false;
        _isFinished = true;

        Debug.Log($"⏱ Game timer stopped! Final time: {FormatTime(_elapsedTime)}");

        // Hide the HUD timer
        if (_timerHUD != null)
            _timerHUD.SetActive(false);

        // Show the score panel
        ShowScorePanel();
    }

    public float GetElapsedTime() => _elapsedTime;
    public bool IsRunning() => _isRunning;

    // ─────────────────────────────────────────────
    //  TIMER HUD (top-left corner)
    // ─────────────────────────────────────────────

    void BuildTimerHUD()
    {
        // Create a shared canvas
        GameObject canvasGO = new GameObject("TimerCanvas");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 20; // above quest UI
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Timer background
        _timerHUD = new GameObject("TimerHUD");
        _timerHUD.transform.SetParent(canvasGO.transform, false);
        Image bg = _timerHUD.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.6f);
        RectTransform bgRT = _timerHUD.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 1);
        bgRT.anchorMax = new Vector2(0, 1);
        bgRT.pivot = new Vector2(0, 1);
        bgRT.anchoredPosition = new Vector2(20, -20);
        bgRT.sizeDelta = new Vector2(200, 50);

        // Timer text
        GameObject textGO = new GameObject("TimerText");
        textGO.transform.SetParent(_timerHUD.transform, false);
        _timerText = textGO.AddComponent<TextMeshProUGUI>();
        _timerText.text = "00:00";
        _timerText.fontSize = timerFontSize;
        _timerText.fontStyle = FontStyles.Bold;
        _timerText.color = timerColor;
        _timerText.alignment = TextAlignmentOptions.Center;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        _timerText.text = "00:00";

        _timerHUD.SetActive(false); // hidden until cutscene ends
    }

    void UpdateTimerDisplay()
    {
        if (_timerText != null)
            _timerText.text = FormatTime(_elapsedTime);
    }

    // ─────────────────────────────────────────────
    //  SCORE PANEL (full-screen overlay)
    // ─────────────────────────────────────────────

    void BuildScorePanel()
    {
        _scorePanel = new GameObject("ScorePanel");
        _scorePanel.transform.SetParent(_canvas.transform, false);
        Image panelImg = _scorePanel.AddComponent<Image>();
        panelImg.color = panelColor;

        // Full screen
        RectTransform panelRT = _scorePanel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // ── Inner container (centered, vertical layout) ──
        GameObject container = new GameObject("Container");
        container.transform.SetParent(_scorePanel.transform, false);
        RectTransform contRT = container.AddComponent<RectTransform>();
        contRT.anchorMin = new Vector2(0.5f, 0.5f);
        contRT.anchorMax = new Vector2(0.5f, 0.5f);
        contRT.pivot = new Vector2(0.5f, 0.5f);
        contRT.sizeDelta = new Vector2(600, 500);
        contRT.anchoredPosition = Vector2.zero;
        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;

        // ── "MISSION COMPLETE" Title ──
        GameObject titleGO = MakeTMPText(container.transform, "MISSION COMPLETE",
            42, FontStyles.Bold, goldColor, TextAlignmentOptions.Center);
        titleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);

        // ── Divider ──
        GameObject divider = new GameObject("Divider");
        divider.transform.SetParent(container.transform, false);
        Image divImg = divider.AddComponent<Image>();
        divImg.color = new Color(1f, 1f, 1f, 0.15f);
        RectTransform divRT = divider.GetComponent<RectTransform>();
        divRT.sizeDelta = new Vector2(0, 2);

        // ── "Your Time" label ──
        GameObject timeLabel = MakeTMPText(container.transform, "YOUR TIME",
            18, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.Center);
        timeLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30);

        // ── Time value (placeholder, updated in ShowScorePanel) ──
        GameObject timeValue = MakeTMPText(container.transform, "00:00",
            56, FontStyles.Bold, accentColor, TextAlignmentOptions.Center);
        timeValue.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 70);
        timeValue.name = "TimeValue";

        // ── Rating (placeholder) ──
        GameObject ratingGO = MakeTMPText(container.transform, "★★★",
            36, FontStyles.Bold, goldColor, TextAlignmentOptions.Center);
        ratingGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
        ratingGO.name = "RatingText";

        // ── Rating description ──
        GameObject ratingDesc = MakeTMPText(container.transform, "",
            16, FontStyles.Italic, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.Center);
        ratingDesc.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30);
        ratingDesc.name = "RatingDesc";

        // ── Spacer ──
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(container.transform, false);
        spacer.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 20);

        // ── Button Row (horizontal layout for side-by-side buttons) ──
        GameObject buttonRow = new GameObject("ButtonRow");
        buttonRow.transform.SetParent(container.transform, false);
        RectTransform rowRT = buttonRow.AddComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 60);
        HorizontalLayoutGroup hlg = buttonRow.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        // ── Restart Button ──
        CreateButton(buttonRow.transform, "RestartButton", "RESTART", buttonColor, OnRestartClicked);

        // ── Return to Menu Button ──
        Color menuBtnColor = new Color(0.2f, 0.45f, 0.7f);
        CreateButton(buttonRow.transform, "MenuButton", "GO TO THE STATION", menuBtnColor, OnMenuClicked);

        _scorePanel.SetActive(false);
    }

    void CreateButton(Transform parent, string name, string label, Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = bgColor;
        btnGO.AddComponent<RectTransform>();
        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        ColorBlock colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.1f);
        colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
        btn.colors = colors;

        GameObject btnTextGO = MakeTMPText(btnGO.transform, label,
            20, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        btn.onClick.AddListener(onClick);
    }

    void ShowScorePanel()
    {
        // Unlock cursor so player can click restart
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Update time display
        Transform container = _scorePanel.transform.GetChild(0);

        TextMeshProUGUI timeText = container.Find("TimeValue")?.GetComponent<TextMeshProUGUI>();
        if (timeText != null)
            timeText.text = FormatTime(_elapsedTime);

        // Calculate rating
        string stars;
        string desc;
        GetRating(_elapsedTime, out stars, out desc);

        TextMeshProUGUI ratingText = container.Find("RatingText")?.GetComponent<TextMeshProUGUI>();
        if (ratingText != null)
            ratingText.text = stars;

        TextMeshProUGUI ratingDesc = container.Find("RatingDesc")?.GetComponent<TextMeshProUGUI>();
        if (ratingDesc != null)
            ratingDesc.text = desc;

        _scorePanel.SetActive(true);

        // Pause the game (optional — player can only click restart)
        Time.timeScale = 0f;
    }

    void GetRating(float time, out string stars, out string description)
    {
        // Rating thresholds (adjust as needed)
        if (time < 120f)       // Under 2 minutes
        {
            stars = "★★★★★";
            description = "Speed run champion!";
        }
        else if (time < 240f)  // Under 4 minutes
        {
            stars = "★★★★☆";
            description = "Excellent work!";
        }
        else if (time < 360f)  // Under 6 minutes
        {
            stars = "★★★☆☆";
            description = "Good job! Room for improvement.";
        }
        else if (time < 600f)  // Under 10 minutes
        {
            stars = "★★☆☆☆";
            description = "Completed, but could be faster.";
        }
        else
        {
            stars = "★☆☆☆☆";
            description = "You made it... eventually!";
        }
    }

    void OnRestartClicked()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnMenuClicked()
    {
        Time.timeScale = 1f;

        // ── Destroy ALL runtime UI canvases from the cooling scene ──

        // 1. Timer / Score panel canvas
        if (_canvas != null)
            Destroy(_canvas.gameObject);

        // 2. Quest UI canvas
        if (QuestUI.Instance != null)
        {
            // The QuestCanvas is the parent of the quest panel
            Canvas questCanvas = QuestUI.Instance.GetComponentInChildren<Canvas>();
            if (questCanvas != null)
                Destroy(questCanvas.gameObject);
            QuestUI.Instance = null;
        }

        // 3. Reset QuestManager singleton + static flags
        if (QuestManager.Instance != null)
            QuestManager.Instance = null;

        // 4. Reset GameTimer singleton
        if (Instance == this)
            Instance = null;

        // ── Re-lock cursor so the main scene player controller works ──
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ── Unload cooling scene & re-activate main scene ──
        Maintocharge loader = FindObjectOfType<Maintocharge>();
        if (loader != null)
        {
            loader.BackFromCooling();
        }
        else
        {
            Debug.LogWarning("Maintocharge not found – falling back to full scene load.");
            SceneManager.LoadScene("0");
        }
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────

    string FormatTime(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    GameObject MakeTMPText(Transform parent, string text, int fontSize,
        FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        tmp.richText = true;

        // Apply custom font if assigned
        if (customFont != null)
            tmp.font = customFont;

        return go;
    }
}
