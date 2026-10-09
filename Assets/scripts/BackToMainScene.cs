using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BackToMainScene : MonoBehaviour
{
    void Start()
    {
        // Only show the button if we are NOT in the main scene
        if (SceneManager.GetActiveScene().name == "0") return;

        // Create Canvas
        GameObject canvasGO = new GameObject("BackButtonCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Button — left-center, matching old "Go Back" position
        GameObject btnGO = new GameObject("GoBackButton");
        btnGO.transform.SetParent(canvasGO.transform, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.32f, 0.99f, 0.60f, 1f);
        btnImg.raycastTarget = true;

        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0, 0.5f);
        btnRT.anchorMax = new Vector2(0, 0.5f);
        btnRT.pivot = new Vector2(0, 0.5f);
        btnRT.anchoredPosition = new Vector2(20, 120);
        btnRT.sizeDelta = new Vector2(200, 50);

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(LoadMainScene);

        // Button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Go Back";
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // Disable the old broken button
        GameObject oldButton = GameObject.Find("Button");
        if (oldButton != null && oldButton.GetComponentInParent<Canvas>() != canvas)
        {
            oldButton.SetActive(false);
        }
    }

    void Update()
    {
        // Skip if we're in the main scene
        if (SceneManager.GetActiveScene().name == "0") return;

        // Press Escape to unlock cursor (so you can click the Go Back button)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Press B to go back to main scene directly (no need to click)
        if (Input.GetKeyDown(KeyCode.B))
        {
            LoadMainScene();
        }
    }

    public void LoadMainScene()
    {
        Debug.Log("[BackToMainScene] Loading main scene...");
        SceneManager.LoadScene("0");
        /*
        Time.timeScale = 1f;

        // Re-lock cursor so the main scene player controller works
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Destroy runtime UI elements specific to SolarTwinScene
        // 1. Destroy this button's Canvas
        Canvas myCanvas = GetComponentInChildren<Canvas>();
        if (myCanvas != null)
            Destroy(myCanvas.gameObject);

        // 2. Destroy SolarUIManager canvas if it exists
        SolarUIManager solarUI = FindObjectOfType<SolarUIManager>();
        if (solarUI != null)
        {
            Canvas solarCanvas = solarUI.GetComponentInChildren<Canvas>();
            if (solarCanvas != null) Destroy(solarCanvas.gameObject);
        }

        // Delegate to Maintocharge if available (to prevent singleton duplication issues)
        Maintocharge loader = FindObjectOfType<Maintocharge>();
        if (loader != null)
        {
            loader.BackFromSolar();
        }
        else
        {
            Debug.LogWarning("Maintocharge not found — falling back to full scene load.");
            
        }
        */
    }
}