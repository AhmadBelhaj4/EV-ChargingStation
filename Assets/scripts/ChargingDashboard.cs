using System.Collections;
using TMPro;
using UnityEngine;

public class ChargingDashboard : MonoBehaviour
{
    [Header("UI")]
    public GameObject dashboardPanel;
    public TMP_Text titleText;
    public TMP_Text batteryText;
    public TMP_Text statusText;

    [Header("Battery Bar")]
    public BatteryBarUI batteryBarUI;

    [Header("Charging Settings")]
    public float chargeStep = 5f;
    public float chargeInterval = 5f;
    public float maxBattery = 100f;

    [Header("Audio")]
    public AudioSource chargingCompleteAudio;

    private bool isCharging = false;
    private float currentBattery = 0f;
    private Coroutine chargingCoroutine;
    private bool hasInitializedBattery = false;

    void Start()
    {
        if (dashboardPanel != null)
            dashboardPanel.SetActive(false);
    }

    public void StartCharging()
    {
        if (isCharging) return;

        // Initialize only once from the user input
        if (!hasInitializedBattery)
        {
            currentBattery = UserInputUI.SavedBattery;
            hasInitializedBattery = true;
        }

        if (dashboardPanel == null) return;

        dashboardPanel.SetActive(true);

        if (titleText != null)
            titleText.text = "Charging Dashboard";

        if (batteryText != null)
            batteryText.text = "Battery: " + currentBattery.ToString("0") + "%";

        if (statusText != null)
            statusText.text = "Charging in progress...";

        if (batteryBarUI != null)
            batteryBarUI.UpdateBatteryBar(currentBattery);

        chargingCoroutine = StartCoroutine(ChargingRoutine());
    }

    IEnumerator ChargingRoutine()
    {
        isCharging = true;

        while (currentBattery < maxBattery)
        {
            yield return new WaitForSeconds(chargeInterval);

            currentBattery += chargeStep;

            if (currentBattery > maxBattery)
                currentBattery = maxBattery;

            if (batteryText != null)
                batteryText.text = "Battery: " + currentBattery.ToString("0") + "%";

            if (batteryBarUI != null)
                batteryBarUI.UpdateBatteryBar(currentBattery);
        }

        // Reached 100%
        UserInputUI.SavedBattery = currentBattery;

        if (statusText != null)
            statusText.text = "Charging complete";

        if (chargingCompleteAudio != null)
            chargingCompleteAudio.Play();

        isCharging = false;
        chargingCoroutine = null;
    }

    public void StopCharging()
    {
        if (chargingCoroutine != null)
        {
            StopCoroutine(chargingCoroutine);
            chargingCoroutine = null;
        }

        isCharging = false;

        // Save the current reached value
        UserInputUI.SavedBattery = currentBattery;

        if (batteryText != null)
            batteryText.text = "Battery: " + currentBattery.ToString("0") + "%";

        if (batteryBarUI != null)
            batteryBarUI.UpdateBatteryBar(currentBattery);

        if (statusText != null)
            statusText.text = "Charging stopped";
    }
}