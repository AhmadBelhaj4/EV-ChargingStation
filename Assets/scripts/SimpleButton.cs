using UnityEngine;

public class SimpleButton : MonoBehaviour
{
    public GameObject solarSystem;
    private bool isOn = true;

    public void ToggleSystem()
    {
        isOn = !isOn;
        solarSystem.SetActive(isOn);

        Debug.Log("System is " + (isOn ? "ON" : "OFF"));
    }
}