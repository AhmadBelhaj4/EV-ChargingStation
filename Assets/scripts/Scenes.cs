/*using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenes : MonoBehaviour
{
    [Header("Scene To Load")]
    public string sceneName;

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty. Write the scene name in the Inspector.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Scene name is empty.");
            return;
        }

        SceneManager.LoadScene(name);
    }

    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}*/
using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class Scenes : MonoBehaviour
{
    public PlayableDirector director;
    public CinemachineCamera playerVCam;

    void Start()
    {
        playerVCam.gameObject.SetActive(false);
        director.stopped += OnCutsceneEnd;
    }

    void OnCutsceneEnd(PlayableDirector pd)
    {
        playerVCam.gameObject.SetActive(true);

        // Start the game timer now that gameplay begins
        GameTimer.Instance?.StartTimer();

        // Show the quest list now that the cutscene is over
        QuestUI.Instance?.ShowPanel();
    }
}