using UnityEngine;
using TMPro;

public class ElementInfoUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI textUI;

    public void ShowInfo(string description)
    {
        panel.SetActive(true);
        textUI.text = description;
    }

    public void HideInfo()
    {
        panel.SetActive(false);
    }
}