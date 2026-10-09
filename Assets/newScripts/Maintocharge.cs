using UnityEngine;
using UnityEngine.SceneManagement;

public class Maintocharge : MonoBehaviour
{
    public static Maintocharge Instance;

    [Header("Scene Names")]
    public string mainSceneName = "0";
    public string coolingSceneName = "cooling scene";
    public string solarSceneName = "SolarTwinScene";

    [Header("Player")]
    public Transform player;

    private static Vector3 savedPlayerPosition;
    private static Quaternion savedPlayerRotation;
    private static bool hasSavedPosition = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When we return to Main Scene, restore player position
        if (scene.name == mainSceneName && hasSavedPosition)
        {
            player = null; // force re-find since old reference is destroyed
            FindPlayerIfNeeded();

            if (player != null)
            {
                player.position = savedPlayerPosition;
                player.rotation = savedPlayerRotation;
            }
        }
    }

    private void SavePlayerPosition()
    {
        FindPlayerIfNeeded();

        if (player != null)
        {
            savedPlayerPosition = player.position;
            savedPlayerRotation = player.rotation;
            hasSavedPosition = true;
        }
        else
        {
            Debug.LogWarning("Player not found. Make sure your player has the tag Player.");
        }
    }

    private void FindPlayerIfNeeded()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }
    }

    private void Update()
    {
        // Press B to go back to the main scene from solar or cooling
        if (Input.GetKeyDown(KeyCode.B))
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == solarSceneName || currentScene == coolingSceneName)
            {
                SceneManager.LoadScene(mainSceneName);
            }
        }
    }

    public void LoadCoolingScene()
    {
        SavePlayerPosition();
        SceneManager.LoadScene(coolingSceneName);
    }

    public void LoadSolarScene()
    {
        SavePlayerPosition();
        SceneManager.LoadScene(solarSceneName);
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    /// <summary>
    /// Called by GameTimer when the player clicks "Go to Station" on the score panel.
    /// Loads back to the main scene.
    /// </summary>
    public void BackFromCooling()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    /// <summary>
    /// Called by the "Go Back to Station" button in the solar scene.
    /// Loads back to the main scene.
    /// </summary>
    public void BackFromSolar()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}