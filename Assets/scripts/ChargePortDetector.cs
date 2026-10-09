using UnityEngine;

public class ChargePortDetector : MonoBehaviour
{
    [Header("Plug Detection")]
    public GameObject plugObject;

    [Header("Visual Indicator")]
    public Renderer indicatorRenderer;
    public Color offColor = Color.black;
    public Color onColor = Color.green;

    [Header("Charging")]
    public ChargingDashboard chargingDashboard;

    [Header("Audio")]
    public AudioSource chargingStartAudio;

    private bool chargingStarted = false;

    void Start()
    {
        SetIndicator(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (plugObject == null) return;

        if (other.gameObject == plugObject || other.transform.IsChildOf(plugObject.transform))
        {
            SetIndicator(true);

            if (!chargingStarted)
            {
                if (chargingDashboard != null)
                    chargingDashboard.StartCharging();

                if (chargingStartAudio != null)
                    chargingStartAudio.Play();

                chargingStarted = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (plugObject == null) return;

        if (other.gameObject == plugObject || other.transform.IsChildOf(plugObject.transform))
        {
            SetIndicator(false);

            if (chargingDashboard != null)
                chargingDashboard.StopCharging();

            chargingStarted = false;
        }
    }

    void SetIndicator(bool state)
    {
        if (indicatorRenderer == null) return;

        indicatorRenderer.material.color = state ? onColor : offColor;
    }
}