using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class WeatherButtonStyle : MonoBehaviour
{
    public string weatherType = "Sunny";

    void Start()
    {
        Button btn = GetComponent<Button>();
        Image img = GetComponent<Image>();
        TMP_Text txt = GetComponentInChildren<TMP_Text>();

        Color textColor = Color.white;
        Color bgColor = new Color(0.08f, 0.10f, 0.16f, 0.96f);
        Color outlineColor = Color.white;

        switch (weatherType)
        {
            case "Sunny":
                textColor = new Color(1.0f, 0.85f, 0.20f);
                outlineColor = textColor;
                break;
            case "Cloudy":
                textColor = new Color(0.75f, 0.88f, 1.0f);
                outlineColor = textColor;
                break;
            case "Rainy":
                textColor = new Color(0.35f, 0.75f, 1.0f);
                outlineColor = textColor;
                break;
            case "Night":
                textColor = new Color(0.75f, 0.50f, 1.0f);
                outlineColor = textColor;
                break;
        }

        img.color = bgColor;

        if (txt != null)
        {
            txt.color = textColor;
            txt.fontSize = 20;
            txt.fontStyle = FontStyles.Bold;
        }

        ColorBlock cb = btn.colors;
        cb.normalColor = bgColor;
        cb.highlightedColor = new Color(0.16f, 0.18f, 0.28f, 1f);
        cb.pressedColor = new Color(0.25f, 0.28f, 0.38f, 1f);
        cb.selectedColor = cb.highlightedColor;
        btn.colors = cb;

        Outline outline = GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(outlineColor.r, outlineColor.g, outlineColor.b, 0.45f);
        outline.effectDistance = new Vector2(2f, -2f);
    }
}