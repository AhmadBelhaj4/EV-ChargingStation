
using TMPro;
using UnityEngine;

public class UserInputUI : MonoBehaviour
{
    public TMP_InputField temperatureInput;
    public TMP_InputField batteryInput;

    public GameObject temperatureFieldObject;
    public GameObject batteryFieldObject;
    public GameObject validateButtonObject;

    public TMP_Text resultMessage;

    [Header("Audio")]
    public AudioSource chargingAudio;
    public AudioSource coolingAudio;

    public static float SavedTemperature = 0f;
    public static float SavedBattery = 0f;

    public void ValidateInputs()
    {
        float temperature = 0f;
        float battery = 0f;

        float.TryParse(temperatureInput.text, out temperature);
        float.TryParse(batteryInput.text, out battery);

        SavedTemperature = temperature;
        SavedBattery = battery;

        Debug.Log("SavedTemperature = " + SavedTemperature);
        Debug.Log("SavedBattery = " + SavedBattery);

        if (temperatureFieldObject != null)
            temperatureFieldObject.SetActive(false);

        if (batteryFieldObject != null)
            batteryFieldObject.SetActive(false);

        if (validateButtonObject != null)
            validateButtonObject.SetActive(false);

        if (resultMessage != null)
        {
            resultMessage.gameObject.SetActive(true);

            if (temperature > 30f)
            {
                resultMessage.text = "Please proceed to the cooling station.";

                if (coolingAudio != null)
                    coolingAudio.Play();
            }
            else
            {
                resultMessage.text = "Please recharge your battery.";

                if (chargingAudio != null)
                    chargingAudio.Play();
            }
        }
    }
}