using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FuseBoxButtonSequence : MonoBehaviour
{
    // =========================================================
    // SWITCH BUTTONS
    // =========================================================

    [Header("6 UI Switch Buttons")]
    public Button[] switchButtons = new Button[6];

    [Header("6 Switch Images")]
    public Image[] switchImages = new Image[6];


    // =========================================================
    // INDICATOR
    // =========================================================

    [Header("Indicator Light")]
    public Image indicatorLight;


    // =========================================================
    // STATUS TEXT
    // =========================================================

    [Header("Status Text")]
    public TMP_Text statusText;


    // =========================================================
    // CORRECT SEQUENCE
    // =========================================================

    [Header("Correct Sequence")]
    
    // 3,1,5,0,4,2 means:
    //
    // Switch 4
    // Switch 2
    // Switch 6
    // Switch 1
    // Switch 5
    // Switch 3

    public int[] correctSequence =
    {
        3,
        1,
        5,
        0,
        4,
        2
    };


    // =========================================================
    // ACTUAL UNITY LIGHTS
    // =========================================================

    [Header("Actual House Lights")]
    
    // Drag the actual Light components here.
    //
    // Example:
    // Living Room Point Light
    // Kitchen Point Light
    // Bedroom Point Light

    public Light[] gameLights;


    // =========================================================
    // LIGHT ON MODELS
    // =========================================================

    [Header("Light ON Models")]
    
    // Drag the Light_ON GameObjects here.

    public GameObject[] lightsOn;


    // =========================================================
    // LIGHT OFF MODELS
    // =========================================================

    [Header("Light OFF Models")]
    
    // Drag the Light_OFF GameObjects here.

    public GameObject[] lightsOff;


    // =========================================================
    // COLORS
    // =========================================================

    [Header("Colors")]

    public Color offColor = Color.white;

    public Color onColor = Color.green;

    public Color correctColor = Color.green;

    public Color wrongColor = Color.red;

    public Color readyColor = Color.yellow;


    // =========================================================
    // RESET SETTINGS
    // =========================================================

    [Header("Wrong Sequence Settings")]

    public float wrongResetDelay = 1f;


    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private int currentStep = 0;

    private bool puzzleCompleted = false;

    private bool isResetting = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Connect all six buttons
        for (int i = 0; i < switchButtons.Length; i++)
        {
            int index = i;

            if (switchButtons[i] != null)
            {
                switchButtons[i].onClick.AddListener(
                    () => PressSwitch(index)
                );
            }
        }

        // Reset switches
        ResetPuzzle();

        // Set indicator to yellow
        if (indicatorLight != null)
        {
            indicatorLight.color = readyColor;
        }

        // Set starting status
        if (statusText != null)
        {
            statusText.text = "ACTIVATE THE SWITCHES";
        }

        // Make sure house lights start ON
        TurnHouseLightsOn();
    }


    // =========================================================
    // PRESS SWITCH
    // =========================================================

    public void PressSwitch(int index)
    {
        // Don't allow input after puzzle is completed
        if (puzzleCompleted)
            return;

        // Don't allow input while resetting
        if (isResetting)
            return;

        // Make sure index is valid
        if (index < 0 || index >= switchButtons.Length)
            return;


        // =====================================================
        // CORRECT SWITCH
        // =====================================================

        if (index == correctSequence[currentStep])
        {
            // Turn this switch green
            if (switchImages[index] != null)
            {
                switchImages[index].color = onColor;
            }

            // Move to next step
            currentStep++;

            // Green indicator
            if (indicatorLight != null)
            {
                indicatorLight.color = correctColor;
            }

            // Status
            if (statusText != null)
            {
                statusText.text = "CORRECT!";
            }

            Debug.Log(
                "Correct switch! Step " + currentStep +
                " / " + correctSequence.Length
            );


            // =================================================
            // CHECK IF COMPLETE
            // =================================================

            if (currentStep >= correctSequence.Length)
            {
                CompletePuzzle();
            }
        }


        // =====================================================
        // WRONG SWITCH
        // =====================================================

        else
        {
            // Red indicator
            if (indicatorLight != null)
            {
                indicatorLight.color = wrongColor;
            }

            // Wrong status
            if (statusText != null)
            {
                statusText.text = "WRONG SEQUENCE!";
            }

            Debug.Log("Wrong switch!");

            // Reset after delay
            StartCoroutine(ResetAfterDelay());
        }
    }


    // =========================================================
    // RESET AFTER WRONG BUTTON
    // =========================================================

    private IEnumerator ResetAfterDelay()
    {
        isResetting = true;

        // Wait
        yield return new WaitForSeconds(wrongResetDelay);

        // Reset switches
        ResetPuzzle();

        // Indicator goes back to yellow
        if (indicatorLight != null)
        {
            indicatorLight.color = readyColor;
        }

        // Status
        if (statusText != null)
        {
            statusText.text = "TRY AGAIN";
        }

        isResetting = false;
    }


    // =========================================================
    // RESET PUZZLE
    // =========================================================

    public void ResetPuzzle()
    {
        currentStep = 0;

        // Reset all six switches
        for (int i = 0; i < switchImages.Length; i++)
        {
            if (switchImages[i] != null)
            {
                switchImages[i].color = offColor;
            }
        }

        Debug.Log("Switch sequence reset.");
    }


    // =========================================================
    // PUZZLE COMPLETED
    // =========================================================

    private void CompletePuzzle()
    {
        puzzleCompleted = true;

        Debug.Log("=================================");
        Debug.Log("FUSE BOX PUZZLE COMPLETED!");
        Debug.Log("POWERING OFF HOUSE...");
        Debug.Log("=================================");


        // =====================================================
        // GREEN INDICATOR
        // =====================================================

        if (indicatorLight != null)
        {
            indicatorLight.color = correctColor;
        }


        // =====================================================
        // STATUS
        // =====================================================

        if (statusText != null)
        {
            statusText.text = "POWER OFF — COMPLETED!";
        }


        // =====================================================
        // TURN OFF HOUSE LIGHTS
        // =====================================================

        TurnHouseLightsOff();
    }


    // =========================================================
    // TURN HOUSE LIGHTS OFF
    // =========================================================

    private void TurnHouseLightsOff()
    {
        Debug.Log("Turning OFF all house lights...");


        // -----------------------------------------------------
        // Disable actual Unity Lights
        // -----------------------------------------------------

        foreach (Light lightSource in gameLights)
        {
            if (lightSource != null)
            {
                lightSource.enabled = false;
            }
        }


        // -----------------------------------------------------
        // Hide ON light models
        // -----------------------------------------------------

        foreach (GameObject lightOn in lightsOn)
        {
            if (lightOn != null)
            {
                lightOn.SetActive(false);
            }
        }


        // -----------------------------------------------------
        // Show OFF light models
        // -----------------------------------------------------

        foreach (GameObject lightOff in lightsOff)
        {
            if (lightOff != null)
            {
                lightOff.SetActive(true);
            }
        }


        Debug.Log("All house lights are OFF.");
    }


    // =========================================================
    // TURN HOUSE LIGHTS ON
    // =========================================================

    private void TurnHouseLightsOn()
    {
        Debug.Log("Turning ON all house lights...");


        // -----------------------------------------------------
        // Enable actual Unity Lights
        // -----------------------------------------------------

        foreach (Light lightSource in gameLights)
        {
            if (lightSource != null)
            {
                lightSource.enabled = true;
            }
        }


        // -----------------------------------------------------
        // Show ON models
        // -----------------------------------------------------

        foreach (GameObject lightOn in lightsOn)
        {
            if (lightOn != null)
            {
                lightOn.SetActive(true);
            }
        }


        // -----------------------------------------------------
        // Hide OFF models
        // -----------------------------------------------------

        foreach (GameObject lightOff in lightsOff)
        {
            if (lightOff != null)
            {
                lightOff.SetActive(false);
            }
        }
    }


    // =========================================================
    // MANUAL RESTART
    // =========================================================

    public void RestartPuzzle()
    {
        // Stop any reset coroutine
        StopAllCoroutines();

        puzzleCompleted = false;

        isResetting = false;

        // Reset switches
        ResetPuzzle();

        // Reset indicator
        if (indicatorLight != null)
        {
            indicatorLight.color = readyColor;
        }

        // Reset status
        if (statusText != null)
        {
            statusText.text = "ACTIVATE THE SWITCHES";
        }

        // Turn house lights back on
        TurnHouseLightsOn();

        Debug.Log("Fuse box puzzle restarted.");
    }
}