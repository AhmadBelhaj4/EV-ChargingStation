using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attache ce script sur chaque bouton météo.
/// Régle weatherType dans l'Inspector : Sunny / Cloudy / Rainy / Night
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ButtonWeatherConnector : MonoBehaviour
{
    [Header("Météo de ce bouton")]
    [Tooltip("Sunny | Cloudy | Rainy | Night")]
    public string weatherType = "Sunny";

    [Header("Style visuel")]
    public bool applyCustomStyle = true;

    private Button _btn;
    private Image _btnImage;
    private TextMeshProUGUI _btnText;

    static readonly Color ColSunny = new Color(1f, 0.78f, 0.15f);
    static readonly Color ColCloudy = new Color(0.65f, 0.80f, 1f);
    static readonly Color ColRainy = new Color(0.22f, 0.60f, 1f);
    static readonly Color ColNight = new Color(0.55f, 0.28f, 1f);

    void Awake()
    {
        _btn = GetComponent<Button>();
        _btnImage = GetComponent<Image>();
        _btnText = GetComponentInChildren<TextMeshProUGUI>();

        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(OnButtonClicked);

        if (applyCustomStyle)
            ApplyStyle();
    }

    void OnButtonClicked()
    {
        if (SolarDataManager.Instance != null)
            SolarDataManager.Instance.SetWeather(weatherType);
        else
            Debug.LogWarning("[ButtonConnector] SolarDataManager introuvable dans la scène.");
    }

    void ApplyStyle()
    {
        Color col = weatherType.ToLower() switch
        {
            "sunny" => ColSunny,
            "cloudy" => ColCloudy,
            "rainy" => ColRainy,
            "night" => ColNight,
            _ => Color.white
        };

        string label = weatherType.ToLower() switch
        {
            "sunny" => "☀ SUNNY",
            "cloudy" => "☁ CLOUDY",
            "rainy" => "⛈ RAINY",
            "night" => "★ NIGHT",
            _ => weatherType.ToUpper()
        };

        if (_btnImage != null)
            _btnImage.color = new Color(0.12f, 0.16f, 0.22f, 0.96f);

        if (_btnText != null)
        {
            _btnText.text = label;
            _btnText.color = col;
            _btnText.fontStyle = FontStyles.Bold;
            _btnText.fontSize = 18;
            _btnText.alignment = TextAlignmentOptions.Center;
        }

        ColorBlock colors = _btn.colors;
        colors.normalColor = new Color(0.12f, 0.16f, 0.22f, 0.96f);
        colors.highlightedColor = new Color(0.20f, 0.24f, 0.32f, 1f);
        colors.pressedColor = new Color(col.r * 0.35f, col.g * 0.35f, col.b * 0.35f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        _btn.colors = colors;

        Outline outline = GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(col.r, col.g, col.b, 0.45f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }
}