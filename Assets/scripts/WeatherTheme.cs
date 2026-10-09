using UnityEngine;

/// <summary>
/// Données de thème pour chaque condition météo.
/// Pas besoin de l'attacher à un GameObject.
/// </summary>
[System.Serializable]
public class WeatherTheme
{
    public string weatherName;
    public string modeLabel;         // "Day" ou "Night"

    // Couleurs principales
    public Color primaryColor;
    public Color secondaryColor;
    public Color backgroundColor;
    public Color accentGlow;
    public Color textColor;

    // Données énergie
    public float solarProduction;
    public float temperatureBonus;

    // Décor visuel
    public string weatherEmoji;
    public string weatherDescription;

    public static WeatherTheme Sunny() => new WeatherTheme
    {
        weatherName = "SUNNY",
        modeLabel = "Day",
        primaryColor = new Color(1f, 0.80f, 0.20f),
        secondaryColor = new Color(1f, 0.55f, 0.10f),
        backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.94f),
        accentGlow = new Color(1f, 0.85f, 0.25f, 0.55f),
        textColor = new Color(1f, 0.96f, 0.82f),
        solarProduction = 100f,
        temperatureBonus = 5f,
        weatherEmoji = "☀",
        weatherDescription = "PEAK SOLAR OUTPUT"
    };

    public static WeatherTheme Cloudy() => new WeatherTheme
    {
        weatherName = "CLOUDY",
        modeLabel = "Day",
        primaryColor = new Color(0.68f, 0.82f, 1f),
        secondaryColor = new Color(0.42f, 0.58f, 0.80f),
        backgroundColor = new Color(0.05f, 0.07f, 0.11f, 0.94f),
        accentGlow = new Color(0.60f, 0.78f, 1f, 0.45f),
        textColor = new Color(0.88f, 0.94f, 1f),
        solarProduction = 50f,
        temperatureBonus = 0f,
        weatherEmoji = "☁",
        weatherDescription = "REDUCED SOLAR CAPACITY"
    };

    public static WeatherTheme Rainy() => new WeatherTheme
    {
        weatherName = "RAINY",
        modeLabel = "Day",
        primaryColor = new Color(0.22f, 0.62f, 1f),
        secondaryColor = new Color(0.10f, 0.38f, 0.85f),
        backgroundColor = new Color(0.03f, 0.06f, 0.12f, 0.95f),
        accentGlow = new Color(0.20f, 0.58f, 1f, 0.45f),
        textColor = new Color(0.80f, 0.92f, 1f),
        solarProduction = 20f,
        temperatureBonus = 0f,
        weatherEmoji = "⛈",
        weatherDescription = "LOW SOLAR — STEG BACKUP ACTIVE"
    };

    public static WeatherTheme Night() => new WeatherTheme
    {
        weatherName = "NIGHT",
        modeLabel = "Night",
        primaryColor = new Color(0.58f, 0.30f, 1f),
        secondaryColor = new Color(0.32f, 0.14f, 0.72f),
        backgroundColor = new Color(0.03f, 0.02f, 0.08f, 0.96f),
        accentGlow = new Color(0.55f, 0.28f, 1f, 0.42f),
        textColor = new Color(0.88f, 0.80f, 1f),
        solarProduction = 0f,
        temperatureBonus = -3f,
        weatherEmoji = "★",
        weatherDescription = "SOLAR OFFLINE — GRID MODE"
    };
}