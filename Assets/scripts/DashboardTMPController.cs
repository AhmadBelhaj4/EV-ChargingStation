using UnityEngine;
using TMPro;

public class DashboardTMPController : MonoBehaviour
{
    public EnergySystem energySystem;

    [Header("LEFT DASHBOARD")]
    public TextMeshProUGUI weatherEmojiText;
    public TextMeshProUGUI weatherText;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI carsText;
    public TextMeshProUGUI statusText;

    [Header("RIGHT DASHBOARD")]
    public TextMeshProUGUI productionText;
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI consumptionText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI sourceText;

    void Update()
    {
        if (energySystem == null) return;

        UpdateLeftDashboard();
        UpdateRightDashboard();
    }

    void UpdateLeftDashboard()
    {
        if (weatherEmojiText != null)
        {
            switch (energySystem.currentWeather)
            {
                case EnergySystem.WeatherType.Sunny:
                    weatherEmojiText.text = "SUNNY";
                    break;
                case EnergySystem.WeatherType.Cloudy:
                    weatherEmojiText.text = "CLOUDY";
                    break;
                case EnergySystem.WeatherType.Rainy:
                    weatherEmojiText.text = "RAINY";
                    break;
                case EnergySystem.WeatherType.Night:
                    weatherEmojiText.text = "NIGHT";
                    break;
            }
        }

        if (weatherText != null)
            weatherText.text = "Weather: " + energySystem.currentWeather;

        if (modeText != null)
            modeText.text = energySystem.inverterOn ? "Mode: ON" : "Mode: OFF";

        if (carsText != null)
            carsText.text = energySystem.chargerOn ? "Cars: " + energySystem.connectedCars : "Cars: 0";

        if (statusText != null)
            statusText.text = "Status: " + energySystem.systemStatus;
    }

    void UpdateRightDashboard()
    {
        if (temperatureText != null)
            temperatureText.text = "Temperature: " + energySystem.currentTemperature.ToString("F1") + "°C";

        if (productionText != null)
            productionText.text = "Production: " + energySystem.currentInputPower.ToString("F1") + " kW";

        if (consumptionText != null)
            consumptionText.text = "Consumption: " + energySystem.currentOutputPower.ToString("F1") + " kW";

        if (speedText != null)
        {
            float displayedSpeed = energySystem.chargerOn ? energySystem.chargeSpeedKW : 0f;
            speedText.text = "Charging Speed: " + displayedSpeed.ToString("F1") + " kW";
        }

        if (sourceText != null)
        {
            if (!energySystem.inverterOn)
            {
                sourceText.text = "Source: OFF";
            }
            else if (energySystem.currentWeather == EnergySystem.WeatherType.Night)
            {
                sourceText.text = "Source: Battery";
            }
            else if (energySystem.currentWeather == EnergySystem.WeatherType.Rainy)
            {
                sourceText.text = "Source: Solar + Battery";
            }
            else
            {
                sourceText.text = "Source: Solar";
            }
        }
    }
}