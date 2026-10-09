using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public bool hasScrewdriver = false;

    [Header("HUD (optional)")]
    public GameObject screwdriverHUDIcon;  // small icon shown when carrying the tool

    public void PickUpScrewdriver()
    {
        hasScrewdriver = true;
        if (screwdriverHUDIcon) screwdriverHUDIcon.SetActive(true);
        Debug.Log("Screwdriver added to inventory.");
    }

    public void RemoveScrewdriver()
    {
        hasScrewdriver = false;
        if (screwdriverHUDIcon) screwdriverHUDIcon.SetActive(false);
        Debug.Log("Screwdriver removed from inventory.");
    }
}