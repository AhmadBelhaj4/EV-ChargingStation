using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    public enum WeatherType
    {
        Sunny,
        Cloudy,
        Rainy,
        Night
    }

    [Header("Main States")]
    public bool inverterOn = true;
    public bool chargerOn = false;

    [Header("Weather")]
    public WeatherType currentWeather = WeatherType.Sunny;

    [Header("Cars")]
    [Range(0, 10)] public int connectedCars = 1;

    [Header("Battery")]
    [Range(0f, 100f)] public float batteryLevel = 78f;
    public float batteryChargeRate = 0.7f;
    public float batteryDischargeRate = 1.0f;

    [Header("Base Power By Weather")]
    public float sunnyProduction = 100f;
    public float cloudyProduction = 65f;
    public float rainyProduction = 30f;
    public float nightProduction = 5f;

    [Header("Charging")]
    public float chargerConsumption = 18f;
    public float chargeSpeedKW = 4.5f;
    [Range(0f, 100f)] public float carChargeProgress = 20f;

    [Header("Live Outputs")]
    public float currentInputPower = 0f;
    public float currentOutputPower = 0f;
    public float currentTemperature = 25f;
    public float currentEfficiency = 92f;
    public string systemStatus = "System Ready";

    [Header("Smoothing")]
    public float productionSmooth = 1.2f;
    public float temperatureSmooth = 0.8f;
    public float outputSmooth = 1.5f;
    public float efficiencySmooth = 1.0f;

    [Header("Update Intervals")]
    public float productionInterval = 3f;
    public float temperatureInterval = 4f;
    public float efficiencyInterval = 5f;

    private float targetInputPower;
    private float targetTemperature;
    private float targetOutputPower;
    private float targetEfficiency;

    private float productionTimer;
    private float temperatureTimer;
    private float efficiencyTimer;

    void Start()
    {
        targetInputPower = GetBaseProduction();
        currentInputPower = targetInputPower;

        targetTemperature = GetBaseTemperature();
        currentTemperature = targetTemperature;

        targetOutputPower = 0f;
        currentOutputPower = 0f;

        targetEfficiency = GetBaseEfficiency();
        currentEfficiency = targetEfficiency;
    }

    void Update()
    {
        UpdateProductionTarget();
        UpdateTemperatureTarget();
        UpdateEfficiencyTarget();
        UpdateOutputTarget();

        currentInputPower = Mathf.Lerp(currentInputPower, targetInputPower, Time.deltaTime * productionSmooth);
        currentTemperature = Mathf.Lerp(currentTemperature, targetTemperature, Time.deltaTime * temperatureSmooth);
        currentOutputPower = Mathf.Lerp(currentOutputPower, targetOutputPower, Time.deltaTime * outputSmooth);
        currentEfficiency = Mathf.Lerp(currentEfficiency, targetEfficiency, Time.deltaTime * efficiencySmooth);

        UpdateBattery();
        UpdateCarCharge();
        UpdateStatus();
    }

    float GetBaseProduction()
    {
        if (!inverterOn) return 0f;

        switch (currentWeather)
        {
            case WeatherType.Sunny: return sunnyProduction;
            case WeatherType.Cloudy: return cloudyProduction;
            case WeatherType.Rainy: return rainyProduction;
            case WeatherType.Night: return nightProduction;
        }

        return 0f;
    }

    float GetBaseTemperature()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return 31f;
            case WeatherType.Cloudy: return 24f;
            case WeatherType.Rainy: return 19f;
            case WeatherType.Night: return 15f;
        }

        return 25f;
    }

    float GetBaseEfficiency()
    {
        if (!inverterOn) return 0f;

        switch (currentWeather)
        {
            case WeatherType.Sunny: return 96f;
            case WeatherType.Cloudy: return 89f;
            case WeatherType.Rainy: return 82f;
            case WeatherType.Night: return 70f;
        }

        return 90f;
    }

    void UpdateProductionTarget()
    {
        if (!inverterOn)
        {
            targetInputPower = 0f;
            return;
        }

        productionTimer += Time.deltaTime;

        if (productionTimer >= productionInterval)
        {
            productionTimer = 0f;

            float baseValue = GetBaseProduction();
            float variation = 0f;

            switch (currentWeather)
            {
                case WeatherType.Sunny:
                    variation = Random.Range(-5f, 5f);
                    break;
                case WeatherType.Cloudy:
                    variation = Random.Range(-8f, 8f);
                    break;
                case WeatherType.Rainy:
                    variation = Random.Range(-4f, 4f);
                    break;
                case WeatherType.Night:
                    variation = Random.Range(-1f, 1f);
                    break;
            }

            targetInputPower = Mathf.Max(0f, baseValue + variation);
        }
    }

    void UpdateTemperatureTarget()
    {
        temperatureTimer += Time.deltaTime;

        if (temperatureTimer >= temperatureInterval)
        {
            temperatureTimer = 0f;

            float baseTemp = GetBaseTemperature();
            float variation = Random.Range(-1.2f, 1.2f);
            targetTemperature = baseTemp + variation;
        }
    }

    void UpdateEfficiencyTarget()
    {
        efficiencyTimer += Time.deltaTime;

        if (efficiencyTimer >= efficiencyInterval)
        {
            efficiencyTimer = 0f;

            float baseEff = GetBaseEfficiency();
            float variation = Random.Range(-2f, 2f);
            targetEfficiency = Mathf.Clamp(baseEff + variation, 0f, 100f);
        }
    }

    void UpdateOutputTarget()
    {
        if (!inverterOn)
        {
            targetOutputPower = 0f;
            return;
        }

        if (chargerOn)
        {
            float variation = Random.Range(-1.5f, 1.5f);
            targetOutputPower = Mathf.Max(0f, chargerConsumption + variation);
        }
        else
        {
            targetOutputPower = Random.Range(0f, 0.5f);
        }
    }

    void UpdateBattery()
    {
        if (!inverterOn)
            return;

        if (chargerOn)
        {
            batteryLevel -= batteryDischargeRate * Time.deltaTime;
        }
        else
        {
            batteryLevel += batteryChargeRate * Time.deltaTime;
        }

        batteryLevel = Mathf.Clamp(batteryLevel, 0f, 100f);

        if (batteryLevel <= 0f)
        {
            batteryLevel = 0f;
            chargerOn = false;
        }
    }

    void UpdateCarCharge()
    {
        if (!inverterOn || !chargerOn)
            return;

        if (batteryLevel <= 0f)
        {
            chargerOn = false;
            return;
        }

        carChargeProgress += 0.18f * Time.deltaTime;
        carChargeProgress = Mathf.Clamp(carChargeProgress, 0f, 100f);

        if (carChargeProgress >= 100f)
        {
            carChargeProgress = 100f;
            chargerOn = false;
        }
    }

    void UpdateStatus()
    {
        if (!inverterOn)
        {
            systemStatus = "Inverter OFF";
        }
        else if (chargerOn)
        {
            systemStatus = "Charging Vehicle";
        }
        else if (batteryLevel >= 99f)
        {
            systemStatus = "Battery Full";
        }
        else if (currentWeather == WeatherType.Night)
        {
            systemStatus = "Night Monitoring";
        }
        else
        {
            systemStatus = "System Ready";
        }
    }

    public void ToggleInverter()
    {
        inverterOn = !inverterOn;

        if (!inverterOn)
        {
            chargerOn = false;
            targetInputPower = 0f;
            targetOutputPower = 0f;
            targetEfficiency = 0f;
        }
        else
        {
            targetInputPower = GetBaseProduction();
            targetEfficiency = GetBaseEfficiency();
        }
    }

    public void ToggleCharger()
    {
        if (!inverterOn) return;
        if (batteryLevel <= 0f) return;
        if (carChargeProgress >= 100f) return;

        chargerOn = !chargerOn;
    }

    public void ResetCarCharge()
    {
        carChargeProgress = 0f;
        chargerOn = false;
    }

    public void SetSunny() { currentWeather = WeatherType.Sunny; }
    public void SetCloudy() { currentWeather = WeatherType.Cloudy; }
    public void SetRainy() { currentWeather = WeatherType.Rainy; }
    public void SetNight() { currentWeather = WeatherType.Night; }
}