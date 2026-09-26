using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FuseBoxButtonSequence : MonoBehaviour
{

    [Header("6 UI Switch Buttons")]
    public Button[] switchButtons = new Button[6];

    [Header("6 Switch Images")]
    public Image[] switchImages = new Image[6];

    [Header("Indicator Light")]
    public Image indicatorLight;

    [Header("Status Text")]
    public TMP_Text statusText;

    [Header("Simulation State")]
    public bool isPowerOff = false;

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

    [Header("Actual House Lights")]

    public Light[] gameLights;

    [Header("Light ON Models")]
    public GameObject[] lightsOn;


    [Header("Light OFF Models")]
    public GameObject[] lightsOff;

    [Header("Colors")]

    public Color offColor = Color.white;

    public Color onColor = Color.green;

    public Color correctColor = Color.green;

    public Color wrongColor = Color.red;

    public Color readyColor = Color.yellow;

    [Header("Wrong Sequence Settings")]

    public float wrongResetDelay = 1f;
    private int currentStep = 0;
    private bool puzzleCompleted = false;
    private bool isResetting = false;
    private Color originalAmbientColor;
    public GameObject vignetteUI;


    private void Start()
    {
        originalAmbientColor = RenderSettings.ambientLight;
        if (vignetteUI != null) vignetteUI.SetActive(false);

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

        ResetPuzzle();

        if (indicatorLight != null)
        {
            indicatorLight.color = readyColor;
        }

        if (statusText != null)
        {
            statusText.text = "ACTIVATE THE SWITCHES";
        }

        TurnHouseLightsOn();
    }

    public void PressSwitch(int index)
    {
        if (puzzleCompleted)
            return;

        if (isResetting)
            return;

        if (index < 0 || index >= switchButtons.Length)
            return;

        if (index == correctSequence[currentStep])
        {
            // Turn this switch green
            if (switchImages[index] != null)
            {
                switchImages[index].color = onColor;
            }

            currentStep++;

            if (indicatorLight != null)
            {
                indicatorLight.color = correctColor;
            }
        
            if (statusText != null)
            {
                statusText.text = "CORRECT!";
            }

            Debug.Log(
                "Correct switch! Step " + currentStep +
                " / " + correctSequence.Length
            );

            if (currentStep >= correctSequence.Length)
            {
                CompletePuzzle();
            }
        }

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

    private IEnumerator ResetAfterDelay()
    {
        isResetting = true;

        yield return new WaitForSeconds(wrongResetDelay);

        ResetPuzzle();
        
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

    private void CompletePuzzle()
    {
        puzzleCompleted = true;

        Debug.Log("=================================");
        Debug.Log("FUSE BOX PUZZLE COMPLETED!");
        Debug.Log("POWERING OFF HOUSE...");
        Debug.Log("=================================");

        if (indicatorLight != null)
        {
            indicatorLight.color = correctColor;
        }

        if (statusText != null)
        {
            statusText.text = "POWER OFF — COMPLETED!";
        }

        TurnHouseLightsOff();
    }

    private void TurnHouseLightsOff()
    {
        isPowerOff = true;
        
        Debug.Log("Turning OFF all house lights...");

        RenderSettings.ambientLight = Color.black;
        if (vignetteUI != null) vignetteUI.SetActive(true);

        foreach (Light lightSource in gameLights)
        {
            if (lightSource != null)
            {
                lightSource.enabled = false;
            }
        }

        foreach (GameObject lightOn in lightsOn)
        {
            if (lightOn != null)
            {
                lightOn.SetActive(false);
            }
        }

        foreach (GameObject lightOff in lightsOff)
        {
            if (lightOff != null)
            {
                lightOff.SetActive(true);
            }
        }
        


        Debug.Log("All house lights are OFF.");
    }

    private void TurnHouseLightsOn()
    {
        Debug.Log("Turning ON all house lights...");

        foreach (Light lightSource in gameLights)
        {
            if (lightSource != null)
            {
                lightSource.enabled = true;
            }
        }

        foreach (GameObject lightOn in lightsOn)
        {
            if (lightOn != null)
            {
                lightOn.SetActive(true);
            }
        }

        foreach (GameObject lightOff in lightsOff)
        {
            if (lightOff != null)
            {
                lightOff.SetActive(false);
            }
        }
    }

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