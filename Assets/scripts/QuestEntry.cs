using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image statusIcon;          // green = done, yellow = active

    [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color completedColor = new Color(0.2f, 1f, 0.4f);

    public void Setup(Quest q)
    {
        if (titleText) titleText.text = q.title;
        if (descriptionText) descriptionText.text = q.description;
        if (progressText) progressText.text = q.ProgressText;

        if (statusIcon)
            statusIcon.color = q.status == QuestStatus.Completed
                ? completedColor
                : activeColor;
    }
}