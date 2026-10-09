using UnityEngine; 
using UnityEngine.SceneManagement; 
public class testbutton: MonoBehaviour 
{ 
    [Header("Main scene name")] 
    public string mainSceneName = "0"; 
    public void GoBack() 
    { 
Cursor.lockState = CursorLockMode.None; Cursor.visible = true; 
SceneManager.LoadScene(mainSceneName); 
    } 
}