using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SolarUIManager : MonoBehaviour
{
    [Header("Night Emergency System")]
    public NightEmergencySystem nightSystem;
    [Header("Dashboard Left")]
    public TMP_Text weatherText;
    public TMP_Text modeText;
    public TMP_Text carsText;
    public TMP_Text statusText;
    public TMP_Text weatherEmojiText;

    [Header("Dashboard Right")]
    public TMP_Text productionText;
    public TMP_Text consumptionText;
    public TMP_Text speedText;
    public TMP_Text temperatureText;
    public TMP_Text sourceText;

    [Header("Bars Fill Only")]
    public Image productionBarFill;
    public Image consumptionBarFill;
    public Image speedBarFill;
    public Image temperatureBarFill;

    [Header("Panels")]
    public Image leftDashboard;
    public Image rightDashboard;
    public Image leftHeader;
    public Image rightHeader;

    [Header("Directional Light")]
    public Light directionalLight;

    public Color sunnyLightColor = new Color(1f, 0.95f, 0.8f);
    public Color cloudyLightColor = new Color(0.82f, 0.87f, 0.96f);
    public Color rainyLightColor = new Color(0.65f, 0.75f, 0.9f);
    public Color nightLightColor = new Color(0.25f, 0.3f, 0.6f);

    public float sunnyIntensity = 1.2f;
    public float cloudyIntensity = 0.8f;
    public float rainyIntensity = 0.55f;
    public float nightIntensity = 0.2f;

    [Header("Skybox")]
    public Material skySunny;
    public Material skyCloudy;
    public Material skyRainy;
    public Material skyNight;

    [Header("Auto Update Values")]
    public float updateInterval = 1f;

    private string currentWeather = "Sunny";
    private bool isNight = false;

    private int cars = 1;
    private float production = 0f;
    private float consumption = 0f;
    private float speed = 0f;
    private float temperature = 0f;
    private string source = "Solar Only";

    private float timer = 0f;

    void Start()
    {
        ApplySunnyTheme();
        GenerateValues();
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ApplySunnyTheme();
            GenerateValues();
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ApplyCloudyTheme();
            GenerateValues();
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ApplyRainyTheme();
            GenerateValues();
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ApplyNightTheme();
            GenerateValues();
            UpdateUI();
        }

        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer = 0f;
            GenerateValues();
            UpdateUI();
        }
        
        
    }

    public void SetSunny()
    {
        ApplySunnyTheme();
        GenerateValues();
        UpdateUI();
    }

    public void SetCloudy()
    {
        ApplyCloudyTheme();
        GenerateValues();
        UpdateUI();
    }

    public void SetRainy()
    {
        ApplyRainyTheme();
        GenerateValues();
        UpdateUI();
    }

    public void SetNight()
    {
        ApplyNightTheme();
        GenerateValues();
        UpdateUI();
    }

    void ApplySunnyTheme()
    {
        currentWeather = "Sunny";
        isNight = false;

        ApplyTheme(
            new Color32(20, 26, 40, 235),
            new Color32(18, 28, 36, 235),
            new Color32(55, 45, 20, 255),
            new Color32(20, 55, 40, 255),
            new Color32(255, 210, 70, 255)
        );

        UpdateDirectionalLight();
        UpdateSkybox();
    }

    void ApplyCloudyTheme()
    {
        currentWeather = "Cloudy";
        isNight = false;

        ApplyTheme(
            new Color32(18, 28, 40, 235),
            new Color32(20, 30, 42, 235),
            new Color32(40, 55, 70, 255),
            new Color32(30, 55, 60, 255),
            new Color32(180, 220, 255, 255)
        );

        UpdateDirectionalLight();
        UpdateSkybox();
    }

    void ApplyRainyTheme()
    {
        currentWeather = "Rainy";
        isNight = false;

        ApplyTheme(
            new Color32(10, 22, 40, 235),
            new Color32(12, 24, 45, 235),
            new Color32(20, 45, 70, 255),
            new Color32(20, 55, 70, 255),
            new Color32(80, 170, 255, 255)
        );

        UpdateDirectionalLight();
        UpdateSkybox();
    }

    void ApplyNightTheme()
    {
        currentWeather = "Night";
        isNight = true;

        ApplyTheme(
            new Color32(18, 12, 35, 235),
            new Color32(16, 10, 30, 235),
            new Color32(55, 25, 75, 255),
            new Color32(35, 20, 60, 255),
            new Color32(190, 120, 255, 255)
        );

        UpdateDirectionalLight();
        UpdateSkybox();
    }

    void GenerateValues()
    {
        cars = Random.Range(5, 51);

        if (isNight)
        {
            production = 0f;
        }
        else if (currentWeather == "Sunny")
        {
            production = Random.Range(85f, 101f);
        }
        else if (currentWeather == "Cloudy")
        {
            production = Random.Range(40f, 61f);
        }
        else if (currentWeather == "Rainy")
        {
            production = Random.Range(10f, 26f);
        }

        consumption = cars * Random.Range(1.8f, 2.4f);
        speed = (100f / cars) * Random.Range(0.9f, 1.1f);
        temperature = 25f + (cars * 0.4f) + Random.Range(-1.5f, 1.5f);

        if (currentWeather == "Sunny")
            temperature += 5f;

        if (isNight)
            temperature -= 3f;

        source = production >= consumption ? "Solar Only" : "Solar + Grid";
    }

    void UpdateUI()
    {
        if (weatherText != null)
            weatherText.text = "Weather: " + currentWeather;

        if (modeText != null)
            modeText.text = "Mode: " + (isNight ? "Night" : "Day");

        if (carsText != null)
            carsText.text = "Cars: " + cars;

        if (statusText != null)
            statusText.text = "Status: " + (isNight ? "Grid Mode Active" : "Solar System Online");

        if (weatherEmojiText != null)
            weatherEmojiText.text = currentWeather.ToUpper();

        if (productionText != null)
            productionText.text = "Production: " + production.ToString("F0") + " kW";

        if (consumptionText != null)
            consumptionText.text = "Consumption: " + consumption.ToString("F0") + " kWh";

        if (speedText != null)
            speedText.text = "Charging Speed: " + speed.ToString("F1") + " kW/car";

        if (temperatureText != null)
            temperatureText.text = "Temperature: " + temperature.ToString("F1") + " °C";

        if (sourceText != null)
            sourceText.text = "Source: " + source;

        if (productionBarFill != null)
            productionBarFill.fillAmount = Mathf.Clamp01(production / 100f);

        if (consumptionBarFill != null)
            consumptionBarFill.fillAmount = Mathf.Clamp01(consumption / 100f);

        if (speedBarFill != null)
            speedBarFill.fillAmount = Mathf.Clamp01(speed / 20f);

        if (temperatureBarFill != null)
            temperatureBarFill.fillAmount = Mathf.Clamp01((temperature - 20f) / 50f);

        if (sourceText != null)
        {
            sourceText.color = source == "Solar Only"
                ? new Color32(80, 255, 140, 255)
                : new Color32(255, 170, 80, 255);
        }
    }

    void ApplyTheme(Color leftBg, Color rightBg, Color leftHead, Color rightHead, Color weatherColor)
    {
        if (leftDashboard != null)
            leftDashboard.color = leftBg;

        if (rightDashboard != null)
            rightDashboard.color = rightBg;

        if (leftHeader != null)
            leftHeader.color = leftHead;

        if (rightHeader != null)
            rightHeader.color = rightHead;

        if (weatherEmojiText != null)
            weatherEmojiText.color = weatherColor;

        if (productionBarFill != null)
            productionBarFill.color = weatherColor;

        if (consumptionBarFill != null)
            consumptionBarFill.color = new Color32(80, 180, 255, 255);

        if (speedBarFill != null)
            speedBarFill.color = new Color32(80, 255, 140, 255);

        if (temperatureBarFill != null)
            temperatureBarFill.color = new Color32(255, 150, 80, 255);
    }

    void UpdateDirectionalLight()
    {
        if (directionalLight == null)
            return;

        if (currentWeather == "Sunny")
        {
            directionalLight.color = sunnyLightColor;
            directionalLight.intensity = sunnyIntensity;
            directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
        else if (currentWeather == "Cloudy")
        {
            directionalLight.color = cloudyLightColor;
            directionalLight.intensity = cloudyIntensity;
            directionalLight.transform.rotation = Quaternion.Euler(35f, -20f, 0f);
        }
        else if (currentWeather == "Rainy")
        {
            directionalLight.color = rainyLightColor;
            directionalLight.intensity = rainyIntensity;
            directionalLight.transform.rotation = Quaternion.Euler(25f, -15f, 0f);
        }
        else if (currentWeather == "Night")
        {
            directionalLight.color = nightLightColor;
            directionalLight.intensity = nightIntensity;
            directionalLight.transform.rotation = Quaternion.Euler(10f, -10f, 0f);
        }
    }

    void UpdateSkybox()
    {
        if (currentWeather == "Sunny" && skySunny != null)
            RenderSettings.skybox = skySunny;
        else if (currentWeather == "Cloudy" && skyCloudy != null)
            RenderSettings.skybox = skyCloudy;
        else if (currentWeather == "Rainy" && skyRainy != null)
            RenderSettings.skybox = skyRainy;
        else if (currentWeather == "Night" && skyNight != null)
            RenderSettings.skybox = skyNight;

        DynamicGI.UpdateEnvironment();
    }
}