using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Required for the timer Coroutine

public class InventoryManager : MonoBehaviour
{
    [Header("Shared Emergency Items")]
    public GameObject flashlightLight; 
    public GameObject bagFlashlightButton; 
    public GameObject hudFlashlightButton; 
    public bool playerOwnsFlashlight = false;
    private bool hasFlashlightOn = false;

    public GameObject bagFirstAidButton; 
    public bool hasUsedFirstAid = false; 

    [Header("Flood Specific")]
    public bool hasLadder = false;

    [Header("UI Prompts")]
    public GameObject warningTextObject; 
    public float warningDuration = 2.5f; 

    void Start()
    {
        if (flashlightLight != null) flashlightLight.SetActive(false);
        if (hudFlashlightButton != null) hudFlashlightButton.SetActive(false);
        if (warningTextObject != null) warningTextObject.SetActive(false); 
    }

    public void TakeFlashlight()
    {
        if (playerOwnsFlashlight) return;
        playerOwnsFlashlight = true;
        if (bagFlashlightButton != null) bagFlashlightButton.SetActive(false);
        if (hudFlashlightButton != null) hudFlashlightButton.SetActive(true);
    }

    public void ToggleFlashlight()
    {
        if (!playerOwnsFlashlight) return;
        hasFlashlightOn = !hasFlashlightOn;
        if (flashlightLight != null) flashlightLight.SetActive(hasFlashlightOn);
    }

    // --- STRICT USE: FIRST AID KIT ---
    
    public void TakeAndUseFirstAidKit()
    {
        if (hasUsedFirstAid) return; 

        EarthquakeTimer timer = FindFirstObjectByType<EarthquakeTimer>();
        
        if (timer != null)
        {
            // EARTHQUAKE SCENE: Check if they actually need it
            if (timer.HasTakenDamage())
            {
                timer.ApplyFirstAid(); 
                hasUsedFirstAid = true;
                if (bagFirstAidButton != null) bagFirstAidButton.SetActive(false);
            }
            else
            {
                // DENY USAGE: Player is unharmed. Show the UI Warning.
                Debug.Log("Maybe I should use this when I need it.");
                if (warningTextObject != null)
                {
                    StartCoroutine(ShowWarningRoutine());
                }
            }
        }
        else
        {
            // FLOOD SCENE: Automatically pack it for the grading evaluation
            hasUsedFirstAid = true;
            if (bagFirstAidButton != null) bagFirstAidButton.SetActive(false);
            Debug.Log("First aid kit packed for the emergency go-bag.");
        }
    }

    // Timer sequence to show and hide the text
    private IEnumerator ShowWarningRoutine()
    {
        warningTextObject.SetActive(true);
        yield return new WaitForSeconds(warningDuration);
        warningTextObject.SetActive(false);
    }
}