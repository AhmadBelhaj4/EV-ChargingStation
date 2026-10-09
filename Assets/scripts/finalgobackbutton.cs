using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class finalgobackbutton : MonoBehaviour, IPointerClickHandler
{
    [Header("Main scene name")]
    public string mainSceneName = "0";

    private Button button;

    void Awake()
    {
        Time.timeScale = 1f;

        FixCursor();
        FixEventSystem();

        button = GetComponent<Button>();

        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(GoBackToMain);

        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = true;
            button.targetGraphic = img;
        }

        Debug.Log("[UniversalGoBackButton] Ready on: " + gameObject.name);
    }

    void Start()
    {
        FixCursor();
    }

    void Update()
    {
        // Press Escape to show mouse
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FixCursor();
            Debug.Log("[UniversalGoBackButton] Cursor unlocked.");
        }

        // Backup: press B to go back
        if (Input.GetKeyDown(KeyCode.B))
        {
            GoBackToMain();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GoBackToMain();
    }

    public void GoBackToMain()
    {
        Debug.Log("[UniversalGoBackButton] Going back to main scene: " + mainSceneName);

        Time.timeScale = 1f;
        FixCursor();

        SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);
    }

    private void FixCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void FixEventSystem()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>();

        if (systems.Length == 0)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Debug.Log("[UniversalGoBackButton] EventSystem created.");
        }
        else
        {
            for (int i = 1; i < systems.Length; i++)
            {
                Destroy(systems[i].gameObject);
            }
        }
    }
}