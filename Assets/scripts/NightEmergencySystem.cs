using UnityEngine;
using TMPro;
using System.Collections;

public class NightEmergencySystem : MonoBehaviour
{
    private bool emergencyActive = false;

    [Header("UI")]
    public GameObject emergencyPanel;
    public TMP_Text emergencyText;
    public CanvasGroup canvasGroup;

    [Header("Street Lights")]
    public GameObject[] streetLights;

    [Header("Directional Light")]
    public Light directionalLight;

    [Header("Light Settings")]
    public float dayIntensity = 1.2f;
    public float nightIntensity = 0.1f;

    public Color dayColor = new Color(1f, 0.95f, 0.8f);
    public Color nightColor = new Color(0.25f, 0.3f, 0.6f);

    void Start()
    {
        emergencyPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        // PRESS 4 → NIGHT
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ActivateEmergency();
        }

        // PRESS L → DAY
        if (emergencyActive && Input.GetKeyDown(KeyCode.L))
        {
            RestoreLights();
        }

        // Pulse UI
        if (emergencyActive)
        {
            float scale = 1f + Mathf.Sin(Time.time * 4f) * 0.05f;
            emergencyPanel.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    void ActivateEmergency()
    {
        emergencyActive = true;

        emergencyPanel.SetActive(true);

        emergencyText.text =
            "<b><color=#FF4C4C> EMERGENCY </color></b>\n\n" +
            "<color=#FFFFFF>Night mode activated</color>\n\n" +
            "<color=#00FFAA>Press [ L ] to restore lighting</color>";

        StartCoroutine(FadeInUI());
        StartCoroutine(SmoothLightTransition(nightIntensity, nightColor));

        // Turn OFF street lights
        foreach (GameObject obj in streetLights)
        {
            if (obj == null) continue;

            Light l = obj.GetComponentInChildren<Light>();
            if (l != null) l.enabled = false;
        }
    }

    void RestoreLights()
    {
        emergencyActive = false;

        StartCoroutine(FadeOutUI());
        StartCoroutine(SmoothLightTransition(dayIntensity, dayColor));

        // Turn ON street lights
        foreach (GameObject obj in streetLights)
        {
            if (obj == null) continue;

            Light l = obj.GetComponentInChildren<Light>();
            if (l != null) l.enabled = true;
        }
    }

    IEnumerator SmoothLightTransition(float targetIntensity, Color targetColor)
    {
        float duration = 2f;
        float time = 0f;

        float startIntensity = directionalLight.intensity;
        Color startColor = directionalLight.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            directionalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            directionalLight.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        directionalLight.intensity = targetIntensity;
        directionalLight.color = targetColor;
    }

    IEnumerator FadeInUI()
    {
        float t = 0f;
        emergencyPanel.transform.localScale = Vector3.zero;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;

            canvasGroup.alpha = t;
            emergencyPanel.transform.localScale =
                Vector3.Lerp(Vector3.zero, Vector3.one, t);

            yield return null;
        }
    }

    IEnumerator FadeOutUI()
    {
        float t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime * 3f;
            canvasGroup.alpha = t;
            yield return null;
        }

        emergencyPanel.SetActive(false);
    }
}