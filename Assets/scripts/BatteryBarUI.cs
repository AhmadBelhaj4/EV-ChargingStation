using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BatteryBarUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform fillBar;
    public TMP_Text percentText;

    [Header("Bar Settings")]
    public float minWidth = 0f;
    public float maxWidth = 300f;

    [Header("Colors")]
    public Color lowColor = Color.red;
    public Color mediumColor = Color.yellow;
    public Color highColor = Color.green;

    private Image fillImage;

    void Awake()
    {
        if (fillBar != null)
            fillImage = fillBar.GetComponent<Image>();
    }

    public void UpdateBatteryBar(float batteryPercent)
    {
        batteryPercent = Mathf.Clamp(batteryPercent, 0f, 100f);

        float width = Mathf.Lerp(minWidth, maxWidth, batteryPercent / 100f);

        if (fillBar != null)
            fillBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        if (percentText != null)
            percentText.text = batteryPercent.ToString("0") + "%";

        if (fillImage != null)
        {
            if (batteryPercent <= 20f)
                fillImage.color = lowColor;
            else if (batteryPercent <= 50f)
                fillImage.color = mediumColor;
            else
                fillImage.color = highColor;
        }
    }
}