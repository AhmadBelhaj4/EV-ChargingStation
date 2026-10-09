using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneCotrol : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
