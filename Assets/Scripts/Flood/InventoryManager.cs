using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Flashlight References")]
    public GameObject flashlightLight; 
    public GameObject bagFlashlightButton; 
    public GameObject hudFlashlightButton; 

    [Header("Ladder Tracking")]
    public bool hasLadder = false;

    private bool hasFlashlightOn = false;
    private bool playerOwnsFlashlight = false;

    void Start()
    {
        if (flashlightLight != null) flashlightLight.SetActive(false);
        if (hudFlashlightButton != null) hudFlashlightButton.SetActive(false);
    }

    public void TakeFlashlight()
    {
        if (playerOwnsFlashlight) return;

        playerOwnsFlashlight = true;
        Debug.Log("Flashlight taken! Added to HUD.");

        if (bagFlashlightButton != null) bagFlashlightButton.SetActive(false);

        if (hudFlashlightButton != null) hudFlashlightButton.SetActive(true);
    }

    public void ToggleFlashlight()
    {
        if (!playerOwnsFlashlight) return;

        hasFlashlightOn = !hasFlashlightOn;
        
        if (flashlightLight != null) 
        {
            flashlightLight.SetActive(hasFlashlightOn);
        }
    }
}