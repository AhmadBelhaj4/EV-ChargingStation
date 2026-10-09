using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Génère automatiquement 2 dashboards futuristes au démarrage.
/// Crée un GameObject vide "DashboardBuilder" et attache ce script dessus.
/// </summary>
public class DashboardBuilder : MonoBehaviour
{
    private Canvas _canvas;
    private GameObject _leftPanel;
    private GameObject _rightPanel;

    private TextMeshProUGUI _txtWeather;
    private TextMeshProUGUI _txtMode;
    private TextMeshProUGUI _txtCars;
    private TextMeshProUGUI _txtWeatherDesc;
    private TextMeshProUGUI _txtWeatherEmoji;

    private Image _leftBorder;
    private Image _leftBg;
    private Image _glowDecor;
    private Image _leftHeaderBg;

    private TextMeshProUGUI _txtProd;
    private TextMeshProUGUI _txtConso;
    private TextMeshProUGUI _txtSpeed;
    private TextMeshProUGUI _txtTemp;
    private TextMeshProUGUI _txtSource;

    private Image _barProd;
    private Image _barConso;
    private Image _barSpeed;
    private Image _barTemp;

    private Image _rightBorder;
    private Image _rightBg;
    private Image _rightHeaderBg;

    private float _targetProd;
    private float _targetConso;
    private float _targetSpeed;
    private float _targetTemp;

    private float _animSpeed = 4f;

    const float PANEL_W = 300f;
    const float PANEL_H = 420f;
    const float MARGIN = 40f;
    const float HEADER_H = 60f;

    void Start()
    {
        BuildCanvas();
        BuildLeftPanel();
        BuildRightPanel();

        SolarDataManager.OnDataUpdated += RefreshDashboards;
        StartCoroutine(InitialRefresh());
    }

    IEnumerator InitialRefresh()
    {
        yield return null;
        RefreshDashboards();
    }

    void OnDestroy()
    {
        SolarDataManager.OnDataUpdated -= RefreshDashboards;
    }

    void Update()
    {
        if (_barProd == null) return;

        AnimateBar(_barProd, _targetProd);
        AnimateBar(_barConso, _targetConso);
        AnimateBar(_barSpeed, _targetSpeed);
        AnimateBar(_barTemp, _targetTemp);

        float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.005f;
        if (_leftPanel != null) _leftPanel.transform.localScale = Vector3.one * pulse;
        if (_rightPanel != null) _rightPanel.transform.localScale = Vector3.one * pulse;
    }

    void BuildCanvas()
    {
        GameObject canvasGO = new GameObject("FuturisticHUD");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10;

        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
    }

    void BuildLeftPanel()
    {
        _leftPanel = CreatePanel(
            "SolarCommandPanel",
            _canvas.transform,
            new Vector2(PANEL_W, PANEL_H),
            new Vector2(0f, 0.5f),
            new Vector2(MARGIN, 0f)
        );

        _leftBg = _leftPanel.GetComponent<Image>();
        _leftBg.color = new Color(0.05f, 0.06f, 0.10f, 0.94f);
        _leftBorder = CreateBorderGlow(_leftPanel.transform, new Color(0.3f, 0.8f, 1f));

        GameObject header = CreateChild("Header", _leftPanel.transform);
        SetRect(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -HEADER_H), new Vector2(0, 0));
        _leftHeaderBg = header.AddComponent<Image>();
        _leftHeaderBg.color = new Color(0.10f, 0.28f, 0.45f, 0.90f);

        GameObject titleGO = CreateChild("Title", header.transform);
        SetRect(titleGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = "◈  SOLAR COMMAND PANEL";
        title.fontSize = 24;
        title.fontStyle = FontStyles.Bold;
        title.color = Color.white;
        title.alignment = TextAlignmentOptions.MidlineLeft;
        title.margin = new Vector4(16, 0, 0, 0);

        GameObject emojiGO = CreateChild("WeatherEmoji", _leftPanel.transform);
        SetRect(emojiGO, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-60, -135), new Vector2(60, -25));
        _txtWeatherEmoji = emojiGO.AddComponent<TextMeshProUGUI>();
        _txtWeatherEmoji.text = "☀";
        _txtWeatherEmoji.fontSize = 60;
        _txtWeatherEmoji.alignment = TextAlignmentOptions.Center;
        _txtWeatherEmoji.color = new Color(1f, 0.9f, 0.2f);

        _glowDecor = CreateGlowCircle(_leftPanel.transform, new Vector2(0, -82), 72f);

        float yStart = -160f;
        float yStep = -50f;

        _txtWeather = CreateDataRow(_leftPanel.transform, "WEATHER", "SUNNY", yStart + yStep * 0);
        _txtMode = CreateDataRow(_leftPanel.transform, "MODE", "DAY", yStart + yStep * 1);
        _txtCars = CreateDataRow(_leftPanel.transform, "CARS CHARGING", "—", yStart + yStep * 2);

        CreateSeparator(_leftPanel.transform, -315f);

        GameObject descGO = CreateChild("WeatherDesc", _leftPanel.transform);
        SetRect(descGO, new Vector2(0, 1), new Vector2(1, 1), new Vector2(MARGIN, -380), new Vector2(-MARGIN, -330));
        _txtWeatherDesc = descGO.AddComponent<TextMeshProUGUI>();
        _txtWeatherDesc.text = "PEAK SOLAR OUTPUT";
        _txtWeatherDesc.fontSize = 14;
        _txtWeatherDesc.fontStyle = FontStyles.Bold;
        _txtWeatherDesc.color = new Color(0.7f, 1f, 0.9f);
        _txtWeatherDesc.alignment = TextAlignmentOptions.Center;

        CreateStatusBadge(_leftPanel.transform, -392f);
    }

    void BuildRightPanel()
    {
        _rightPanel = CreatePanel(
            "EnergyFlowMonitor",
            _canvas.transform,
            new Vector2(PANEL_W, PANEL_H),
            new Vector2(1f, 0.5f),
            new Vector2(-MARGIN, 0f)
        );

        _rightBg = _rightPanel.GetComponent<Image>();
        _rightBg.color = new Color(0.05f, 0.08f, 0.10f, 0.94f);
        _rightBorder = CreateBorderGlow(_rightPanel.transform, new Color(0.1f, 1f, 0.5f));

        GameObject header = CreateChild("Header", _rightPanel.transform);
        SetRect(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -HEADER_H), new Vector2(0, 0));
        _rightHeaderBg = header.AddComponent<Image>();
        _rightHeaderBg.color = new Color(0.08f, 0.24f, 0.18f, 0.90f);

        GameObject titleGO = CreateChild("Title", header.transform);
        SetRect(titleGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = "⚡  ENERGY FLOW MONITOR";
        title.fontSize = 24;
        title.fontStyle = FontStyles.Bold;
        title.color = Color.white;
        title.alignment = TextAlignmentOptions.MidlineLeft;
        title.margin = new Vector4(16, 0, 0, 0);

        float yStart = -76f;
        float yStep = -74f;

        (_barProd, _txtProd) = CreateEnergyBar(_rightPanel.transform, "SOLAR PRODUCTION", "100 kW", yStart + yStep * 0, new Color(1f, 0.85f, 0.1f));
        (_barConso, _txtConso) = CreateEnergyBar(_rightPanel.transform, "CONSUMPTION", "50 kW", yStart + yStep * 1, new Color(0.2f, 0.7f, 1f));
        (_barSpeed, _txtSpeed) = CreateEnergyBar(_rightPanel.transform, "CHARGING SPEED", "5.0 kW", yStart + yStep * 2, new Color(0.1f, 1f, 0.5f));
        (_barTemp, _txtTemp) = CreateEnergyBar(_rightPanel.transform, "SYSTEM TEMP", "50 °C", yStart + yStep * 3, new Color(1f, 0.4f, 0.1f));

        float sourceY = -396f;

        GameObject srcContainer = CreateChild("SourceContainer", _rightPanel.transform);
        SetRect(srcContainer, new Vector2(0, 1), new Vector2(1, 1), new Vector2(MARGIN, sourceY - 40f), new Vector2(-MARGIN, sourceY));
        Image srcBg = srcContainer.AddComponent<Image>();
        srcBg.color = new Color(0.06f, 0.15f, 0.11f, 0.85f);

        GameObject srcLabelGO = CreateChild("SourceLabel", srcContainer.transform);
        SetRect(srcLabelGO, new Vector2(0, 0), new Vector2(0.48f, 1), Vector2.zero, Vector2.zero);
        TextMeshProUGUI srcLabel = srcLabelGO.AddComponent<TextMeshProUGUI>();
        srcLabel.text = "ENERGY SOURCE";
        srcLabel.fontSize = 12;
        srcLabel.fontStyle = FontStyles.Bold;
        srcLabel.color = new Color(0.55f, 0.9f, 0.7f);
        srcLabel.alignment = TextAlignmentOptions.MidlineLeft;
        srcLabel.margin = new Vector4(10, 0, 0, 0);

        GameObject srcValueGO = CreateChild("SourceValue", srcContainer.transform);
        SetRect(srcValueGO, new Vector2(0.48f, 0), new Vector2(1f, 1), Vector2.zero, Vector2.zero);
        _txtSource = srcValueGO.AddComponent<TextMeshProUGUI>();
        _txtSource.text = "Solar Only";
        _txtSource.fontSize = 15;
        _txtSource.fontStyle = FontStyles.Bold;
        _txtSource.color = new Color(0.2f, 1f, 0.6f);
        _txtSource.alignment = TextAlignmentOptions.MidlineRight;
        _txtSource.margin = new Vector4(0, 0, 10, 0);
    }

    void RefreshDashboards()
    {
        if (SolarDataManager.Instance == null) return;

        SolarDataManager d = SolarDataManager.Instance;
        WeatherTheme t = d.CurrentTheme;

        ApplyThemeColors(t);

        _txtWeatherEmoji.text = t.weatherEmoji;
        _txtWeatherEmoji.color = t.primaryColor;

        _txtWeather.text = t.weatherName;
        _txtMode.text = t.modeLabel.ToUpper();
        _txtCars.text = d.Cars.ToString();
        _txtWeatherDesc.text = t.weatherDescription;
        _txtWeatherDesc.color = t.textColor;

        _txtProd.text = $"{d.SolarProduction:0} kW";
        _txtConso.text = $"{d.EnergyConsumption:0} kW";
        _txtSpeed.text = $"{d.ChargingSpeed:0.0} kW/car";
        _txtTemp.text = $"{d.SystemTemperature:0.0} °C";

        bool solarOnly = d.EnergySource == "Solar Only";
        _txtSource.text = d.EnergySource;
        _txtSource.color = solarOnly ? new Color(0.2f, 1f, 0.5f) : new Color(1f, 0.55f, 0.15f);

        _targetProd = Mathf.Clamp01(d.SolarProduction / 100f);
        _targetConso = Mathf.Clamp01(d.EnergyConsumption / 100f);
        _targetSpeed = Mathf.Clamp01(d.ChargingSpeed / 20f);
        _targetTemp = Mathf.Clamp01((d.SystemTemperature - 20f) / 50f);
    }

    void ApplyThemeColors(WeatherTheme t)
    {
        _leftBg.color = t.backgroundColor;
        _rightBg.color = new Color(
            Mathf.Clamp01(t.backgroundColor.r * 0.85f),
            Mathf.Clamp01(t.backgroundColor.g * 1.05f),
            Mathf.Clamp01(t.backgroundColor.b * 0.95f),
            t.backgroundColor.a
        );

        _leftBorder.color = new Color(t.primaryColor.r, t.primaryColor.g, t.primaryColor.b, 0.95f);
        _rightBorder.color = new Color(t.secondaryColor.r, t.secondaryColor.g, t.secondaryColor.b, 0.95f);

        _glowDecor.color = new Color(t.accentGlow.r, t.accentGlow.g, t.accentGlow.b, 0.20f);

        _leftHeaderBg.color = new Color(t.primaryColor.r * 0.22f, t.primaryColor.g * 0.22f, t.primaryColor.b * 0.22f, 0.90f);
        _rightHeaderBg.color = new Color(t.secondaryColor.r * 0.25f, t.secondaryColor.g * 0.25f, t.secondaryColor.b * 0.25f, 0.90f);

        _barProd.color = t.primaryColor;
    }

    void AnimateBar(Image bar, float target)
    {
        bar.fillAmount = Mathf.Lerp(bar.fillAmount, target, Time.deltaTime * _animSpeed);
    }

    GameObject CreatePanel(string name, Transform parent, Vector2 size, Vector2 pivot, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.pivot = pivot;
        rt.anchorMin = pivot;
        rt.anchorMax = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.05f, 0.05f, 0.10f, 0.94f);

        return go;
    }

    Image CreateBorderGlow(Transform parent, Color color)
    {
        GameObject go = new GameObject("BorderGlow", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0);

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(color.r, color.g, color.b, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        CreateBorderLine(parent, new Vector2(0, 0), new Vector2(1, 0), 2f, color);
        CreateBorderLine(parent, new Vector2(0, 1), new Vector2(1, 1), 2f, color);
        CreateBorderLine(parent, new Vector2(0, 0), new Vector2(0, 1), 2f, color, true);
        CreateBorderLine(parent, new Vector2(1, 0), new Vector2(1, 1), 2f, color, true);

        return img;
    }

    void CreateBorderLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax, float thickness, Color color, bool vertical = false)
    {
        GameObject go = new GameObject("BorderLine", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;

        if (!vertical)
        {
            rt.offsetMin = new Vector2(0, anchorMin.y == 0 ? 0 : -thickness);
            rt.offsetMax = new Vector2(0, anchorMin.y == 0 ? thickness : 0);
        }
        else
        {
            rt.offsetMin = new Vector2(anchorMin.x == 0 ? 0 : -thickness, 0);
            rt.offsetMax = new Vector2(anchorMin.x == 0 ? thickness : 0, 0);
        }

        go.GetComponent<Image>().color = color;
    }

    Image CreateGlowCircle(Transform parent, Vector2 position, float radius)
    {
        GameObject go = new GameObject("GlowCircle", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(radius * 2f, radius * 2f);
        rt.anchoredPosition = position;

        Image img = go.GetComponent<Image>();
        img.color = new Color(1f, 0.9f, 0.2f, 0.18f);

        return img;
    }

    TextMeshProUGUI CreateDataRow(Transform parent, string label, string value, float y)
    {
        const float rowH = 42f;

        GameObject row = CreateChild($"Row_{label}", parent);
        SetRect(row, new Vector2(0, 1), new Vector2(1, 1), new Vector2(MARGIN, y - rowH), new Vector2(-MARGIN, y));

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(1f, 1f, 1f, 0.035f);

        GameObject labelGO = CreateChild("Label", row.transform);
        SetRect(labelGO, new Vector2(0, 0), new Vector2(0.52f, 1), Vector2.zero, Vector2.zero);
        TextMeshProUGUI lTmp = labelGO.AddComponent<TextMeshProUGUI>();
        lTmp.text = label;
        lTmp.fontSize = 12;
        lTmp.fontStyle = FontStyles.Bold;
        lTmp.color = new Color(0.60f, 0.78f, 0.95f, 0.95f);
        lTmp.alignment = TextAlignmentOptions.MidlineLeft;
        lTmp.margin = new Vector4(10, 0, 0, 0);

        GameObject valueGO = CreateChild("Value", row.transform);
        SetRect(valueGO, new Vector2(0.52f, 0), new Vector2(1f, 1), Vector2.zero, Vector2.zero);
        TextMeshProUGUI vTmp = valueGO.AddComponent<TextMeshProUGUI>();
        vTmp.text = value;
        vTmp.fontSize = 18;
        vTmp.fontStyle = FontStyles.Bold;
        vTmp.color = Color.white;
        vTmp.alignment = TextAlignmentOptions.MidlineRight;
        vTmp.margin = new Vector4(0, 0, 10, 0);

        return vTmp;
    }

    (Image bar, TextMeshProUGUI valueText) CreateEnergyBar(Transform parent, string label, string value, float y, Color barColor)
    {
        const float blockH = 64f;

        GameObject block = CreateChild($"EnergyBlock_{label}", parent);
        SetRect(block, new Vector2(0, 1), new Vector2(1, 1), new Vector2(MARGIN, y - blockH), new Vector2(-MARGIN, y));

        Image blockBg = block.AddComponent<Image>();
        blockBg.color = new Color(1f, 1f, 1f, 0.035f);

        GameObject labelRow = CreateChild("LabelRow", block.transform);
        SetRect(labelRow, new Vector2(0, 0.45f), new Vector2(1, 1), Vector2.zero, Vector2.zero);

        GameObject labelGO = CreateChild("Label", labelRow.transform);
        SetRect(labelGO, new Vector2(0, 0), new Vector2(0.60f, 1), Vector2.zero, Vector2.zero);
        TextMeshProUGUI lTmp = labelGO.AddComponent<TextMeshProUGUI>();
        lTmp.text = label;
        lTmp.fontSize = 11;
        lTmp.fontStyle = FontStyles.Bold;
        lTmp.color = new Color(
            Mathf.Clamp01(barColor.r * 0.7f + 0.3f),
            Mathf.Clamp01(barColor.g * 0.7f + 0.3f),
            Mathf.Clamp01(barColor.b * 0.7f + 0.3f),
            1f
        );
        lTmp.alignment = TextAlignmentOptions.MidlineLeft;
        lTmp.margin = new Vector4(10, 0, 0, 0);

        GameObject valueGO = CreateChild("Value", labelRow.transform);
        SetRect(valueGO, new Vector2(0.60f, 0), new Vector2(1f, 1), Vector2.zero, Vector2.zero);
        TextMeshProUGUI vTmp = valueGO.AddComponent<TextMeshProUGUI>();
        vTmp.text = value;
        vTmp.fontSize = 15;
        vTmp.fontStyle = FontStyles.Bold;
        vTmp.color = barColor;
        vTmp.alignment = TextAlignmentOptions.MidlineRight;
        vTmp.margin = new Vector4(0, 0, 10, 0);

        GameObject barBgGO = CreateChild("BarBg", block.transform);
        SetRect(barBgGO, new Vector2(0, 0), new Vector2(1, 0.45f), new Vector2(10, 6), new Vector2(-10, -6));
        Image barBgImg = barBgGO.AddComponent<Image>();
        barBgImg.color = new Color(0.10f, 0.12f, 0.18f, 0.95f);

        GameObject barGO = CreateChild("Bar", barBgGO.transform);
        SetRect(barGO, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));
        Image barImg = barGO.AddComponent<Image>();
        barImg.color = barColor;
        barImg.type = Image.Type.Filled;
        barImg.fillMethod = Image.FillMethod.Horizontal;
        barImg.fillAmount = 0.7f;

        return (barImg, vTmp);
    }

    void CreateSeparator(Transform parent, float y)
    {
        GameObject go = CreateChild("Separator", parent);
        SetRect(go, new Vector2(0, 1), new Vector2(1, 1), new Vector2(MARGIN, y - 1f), new Vector2(-MARGIN, y));

        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.10f);
    }

    GameObject CreateStatusBadge(Transform parent, float y)
    {
        GameObject badge = CreateChild("StatusBadge", parent);
        SetRect(badge, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-90, y - 28f), new Vector2(90, y));

        Image img = badge.AddComponent<Image>();
        img.color = new Color(0.05f, 0.35f, 0.12f, 0.82f);

        GameObject txt = CreateChild("StatusText", badge.transform);
        SetRect(txt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = "●  SYSTEM ONLINE";
        tmp.fontSize = 11;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.2f, 1f, 0.5f);
        tmp.alignment = TextAlignmentOptions.Center;

        return badge;
    }

    GameObject CreateChild(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }
}