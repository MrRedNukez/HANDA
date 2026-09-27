using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CodeLocation
{
    public string locationName; 
    public GameObject noteObject; 
    public TextMeshProUGUI worldTextDisplay; 
    [TextArea]
    public string[] hintDialogues; 
}

public class FuseButtonSequence : MonoBehaviour
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
    public SimulationManager simManager;
    private bool hasBeenWarned = false;

    [Header("Correct Sequence")]
    public int[] correctSequence = { 3, 1, 5, 0, 4, 2 };

    [Header("Water Detection (Collider)")]
    public string waterTag = "Water";
    private bool isSubmerged = false;

    [Header("Randomized Code Spawns")]
    public CodeLocation[] possibleLocations;
    public TextMeshProUGUI dialogueUI;

    [Header("Actual House Lights")]
    public Light[] gameLights;
    public GameObject[] lightsOn;
    public GameObject[] lightsOff;

    [Header("Colors")]
    public Color offColor = Color.white;
    public Color onColor = Color.green;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public Color readyColor = Color.yellow;

    [Header("Settings")]
    public float wrongResetDelay = 1f;
    public GameObject vignetteUI;

    private List<int> enteredSequence = new List<int>(); 
    private bool puzzleCompleted = false;
    private bool isResetting = false;
    
    private string activeHint;
    private int failedAttempts = 0;

    private void Start()
    {
        if (vignetteUI != null) vignetteUI.SetActive(false);

        for (int i = 0; i < switchButtons.Length; i++)
        {
            int index = i;
            if (switchButtons[i] != null)
            {
                switchButtons[i].onClick.AddListener(() => PressSwitch(index));
            }
        }

        ResetPuzzle();
        SetupRandomCodeLocation();

        if (indicatorLight != null) indicatorLight.color = readyColor;
        if (statusText != null) statusText.text = "ACTIVATE THE SWITCHES";

        TurnHouseLightsOn();
    }

    // --- NEW COLLIDER LOGIC ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(waterTag))
        {
            isSubmerged = true;
            Debug.Log("Fuse Box is now submerged in water.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(waterTag))
        {
            isSubmerged = false;
            Debug.Log("Water receded from Fuse Box.");
        }
    }
    // --------------------------

    private void SetupRandomCodeLocation()
    {
        for (int i = 0; i < correctSequence.Length; i++)
        {
            int temp = correctSequence[i];
            int randomIndex = Random.Range(i, correctSequence.Length);
            correctSequence[i] = correctSequence[randomIndex];
            correctSequence[randomIndex] = temp;
        }

        foreach (CodeLocation loc in possibleLocations)
        {
            if (loc.noteObject != null) loc.noteObject.SetActive(false);
        }

        if (possibleLocations.Length > 0)
        {
            int randomLocIndex = Random.Range(0, possibleLocations.Length);
            CodeLocation activeLocation = possibleLocations[randomLocIndex];
            
            activeLocation.noteObject.SetActive(true);

            if (activeLocation.hintDialogues != null && activeLocation.hintDialogues.Length > 0)
            {
                int randomHintIndex = Random.Range(0, activeLocation.hintDialogues.Length);
                activeHint = activeLocation.hintDialogues[randomHintIndex];
            }

            if (activeLocation.worldTextDisplay != null)
            {
                string readableCode = "";
                for (int i = 0; i < correctSequence.Length; i++)
                {
                    readableCode += (correctSequence[i] + 1).ToString();
                    if (i < correctSequence.Length - 1) readableCode += "-";
                }
                activeLocation.worldTextDisplay.text = readableCode;
            }
        }
    }

    public void PressSwitch(int index)
    {
        if (puzzleCompleted || isResetting) return;

        if (isSubmerged)
        {
            if (!hasBeenWarned)
            {
                // Strike 1: The Warning
                ShowDialogue("Water level is too high! Touching it now will kill me.");
                hasBeenWarned = true; 
            }
            else
            {
                // Strike 2: The Consequence
                Debug.Log("Player ignored flood warning. Triggering Electrocution.");
                if (simManager != null)
                {
                    simManager.SetPlayerInside(true); 
                    simManager.EndSimulation(false); 
                }
            }
            return; 
        }

        if (enteredSequence.Contains(index)) return;

        enteredSequence.Add(index);
        if (switchImages[index] != null) switchImages[index].color = readyColor;

        if (enteredSequence.Count == correctSequence.Length)
        {
            VerifyFullSequence();
        }
    }

    private void VerifyFullSequence()
    {
        bool isCorrect = true;
        
        for (int i = 0; i < correctSequence.Length; i++)
        {
            if (enteredSequence[i] != correctSequence[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            CompletePuzzle();
        }
        else
        {
            failedAttempts++;
            
            if (failedAttempts == 2)
            {
                ShowDialogue(activeHint);
                failedAttempts = 0; 
            }
            else
            {
                ShowDialogue("Incorrect sequence.");
            }

            if (indicatorLight != null) indicatorLight.color = wrongColor;
            if (statusText != null) statusText.text = "WRONG SEQUENCE!";

            foreach (int btnIndex in enteredSequence)
            {
                if (switchImages[btnIndex] != null) switchImages[btnIndex].color = wrongColor;
            }

            StartCoroutine(ResetAfterDelay());
        }
    }

    private IEnumerator ResetAfterDelay()
    {
        isResetting = true;
        yield return new WaitForSeconds(wrongResetDelay);
        ResetPuzzle();
        
        if (indicatorLight != null) indicatorLight.color = readyColor;
        if (statusText != null) statusText.text = "TRY AGAIN";
        isResetting = false;
    }

    public void ResetPuzzle()
    {
        enteredSequence.Clear();
        for (int i = 0; i < switchImages.Length; i++)
        {
            if (switchImages[i] != null) switchImages[i].color = offColor;
        }
    }

    private void CompletePuzzle()
    {
        puzzleCompleted = true;
        ShowDialogue("Power disabled. Phew");

        if (indicatorLight != null) indicatorLight.color = correctColor;
        if (statusText != null) statusText.text = "POWER OFF — COMPLETED!";

        for (int i = 0; i < switchImages.Length; i++)
        {
            if (switchImages[i] != null) switchImages[i].color = correctColor;
        }

        TurnHouseLightsOff();
    }

    private void TurnHouseLightsOff()
    {
        isPowerOff = true;
        RenderSettings.ambientLight = Color.black;
        if (vignetteUI != null) vignetteUI.SetActive(true);

        foreach (Light lightSource in gameLights)
            if (lightSource != null) lightSource.enabled = false;

        foreach (GameObject lightOn in lightsOn)
            if (lightOn != null) lightOn.SetActive(false);

        foreach (GameObject lightOff in lightsOff)
            if (lightOff != null) lightOff.SetActive(true);
    }

    private void TurnHouseLightsOn()
    {
        foreach (Light lightSource in gameLights)
            if (lightSource != null) lightSource.enabled = true;

        foreach (GameObject lightOn in lightsOn)
            if (lightOn != null) lightOn.SetActive(true);

        foreach (GameObject lightOff in lightsOff)
            if (lightOff != null) lightOff.SetActive(false);
    }

    private void ShowDialogue(string text)
    {
        if (dialogueUI != null) dialogueUI.text = text;
        Debug.Log("Dialogue: " + text);
    }
}