using UnityEngine;
using System;

/// <summary>
/// Script central : gère les données énergétiques et notifie les dashboards.
/// Crée un GameObject vide appelé "SolarManager" et attache ce script dessus.
/// </summary>
public class SolarDataManager : MonoBehaviour
{
    public static SolarDataManager Instance { get; private set; }

    public WeatherTheme CurrentTheme { get; private set; }
    public int Cars { get; private set; }
    public float SolarProduction { get; private set; }
    public float EnergyConsumption { get; private set; }
    public float ChargingSpeed { get; private set; }
    public float SystemTemperature { get; private set; }
    public string EnergySource { get; private set; }

    public static event Action OnDataUpdated;

    [Header("Météo de démarrage")]
    [Tooltip("Sunny / Cloudy / Rainy / Night")]
    public string startWeather = "Sunny";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        SetWeather(startWeather);
    }

    public void SetWeather(string weather)
    {
        switch (weather.ToLower())
        {
            case "sunny":
                CurrentTheme = WeatherTheme.Sunny();
                break;
            case "cloudy":
                CurrentTheme = WeatherTheme.Cloudy();
                break;
            case "rainy":
                CurrentTheme = WeatherTheme.Rainy();
                break;
            case "night":
                CurrentTheme = WeatherTheme.Night();
                break;
            default:
                CurrentTheme = WeatherTheme.Sunny();
                break;
        }

        Cars = UnityEngine.Random.Range(1, 51);
        SolarProduction = CurrentTheme.solarProduction;
        EnergyConsumption = Mathf.Round(Cars * 2f);
        ChargingSpeed = Mathf.Round((100f / Cars) * 10f) / 10f;
        SystemTemperature = Mathf.Round((25f + Cars * 0.5f + CurrentTheme.temperatureBonus) * 10f) / 10f;
        EnergySource = SolarProduction >= EnergyConsumption ? "Solar Only" : "Solar + STEG";

        OnDataUpdated?.Invoke();

        Debug.Log($"[SolarManager] Weather={CurrentTheme.weatherName} | Cars={Cars} | Prod={SolarProduction} | Conso={EnergyConsumption} | Source={EnergySource}");
    }

    public void SetSunny() => SetWeather("Sunny");
    public void SetCloudy() => SetWeather("Cloudy");
    public void SetRainy() => SetWeather("Rainy");
    public void SetNight() => SetWeather("Night");
}