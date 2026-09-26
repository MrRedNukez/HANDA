using UnityEngine;
using TMPro;
using System.Collections;

public class ReportManager : MonoBehaviour
{
    [Header("UI Panels")]
    public CanvasGroup quotePanel;
    public CanvasGroup reportPanel;
    public GameObject slide1_Overview;
    public GameObject slide2_Checklist;

    [Header("Death Quotes")]
    public TextMeshProUGUI quoteTextUI;
    
    [TextArea(2, 3)]
    public string[] drowningQuotes = {
        "A person drowns in their own demise.",
        "Water does not distinguish between the prepared and the ignorant.",
        "The flood takes everything, leaving only silence."
    };

    [TextArea(2, 3)]
    public string[] electrocutionQuotes = {
        "Water and electricity: a deadly embrace.",
        "A single spark in the dark is all it takes.",
        "The power you left on became your own executioner."
    };

    [Header("Slide 1 Text")]
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI commentText;
    public TextMeshProUGUI recommendationText;

    [Header("Slide 2 Text")]
    public TextMeshProUGUI checklistText;

    public void TriggerEvaluation(bool survived, SimulationManager.DeathCause deathCause, bool powerOff, bool flashlightUsed)
    {
        StartCoroutine(EvaluationSequence(survived, deathCause, powerOff, flashlightUsed));
    }

    private IEnumerator EvaluationSequence(bool survived, SimulationManager.DeathCause deathCause, bool powerOff, bool flashlightUsed)
    {
        // 1. FREEZE TIME IMMEDIATELY
        Time.timeScale = 0f; 

        if (!survived)
        {
            // 2. Pick the correct quote based on how they died
            if (quoteTextUI != null)
            {
                if (deathCause == SimulationManager.DeathCause.Electrocution && electrocutionQuotes.Length > 0)
                {
                    quoteTextUI.text = electrocutionQuotes[Random.Range(0, electrocutionQuotes.Length)];
                }
                else if (drowningQuotes.Length > 0)
                {
                    quoteTextUI.text = drowningQuotes[Random.Range(0, drowningQuotes.Length)];
                }
            }

            quotePanel.alpha = 1;
            quotePanel.gameObject.SetActive(true);
            reportPanel.gameObject.SetActive(false);
            
            // CRITICAL: We must use Realtime here since timeScale is 0
            yield return new WaitForSecondsRealtime(3.0f); 
            
            quotePanel.gameObject.SetActive(false);
        }

        // 3. Swap to the Report
        CalculateGrade(survived, deathCause, powerOff, flashlightUsed);
        
        reportPanel.gameObject.SetActive(true);
        slide1_Overview.SetActive(true);
        slide2_Checklist.SetActive(false);
    }

    private void CalculateGrade(bool survived, SimulationManager.DeathCause deathCause, bool powerOff, bool flashlightUsed)
    {
        if (!survived)
        {
            gradeText.text = "F";
            gradeText.color = Color.red;

            if (deathCause == SimulationManager.DeathCause.Electrocution)
            {
                commentText.text = "Simulation Failed. Player was electrocuted.";
                recommendationText.text = "FATAL ERROR: Active power lines contacted floodwater. Always find the breaker panel and turn it off before water rises.";
                checklistText.text = "[ X ] Evacuated to high ground\n[ X ] Disabled main power breaker\n[ - ] Utilized emergency lighting";
            }
            else // Drowning
            {
                commentText.text = "Simulation Failed. Player did not evacuate in time.";
                recommendationText.text = "WARNING: Evacuate vertically immediately. Do not wait on the ground floor when water is rising.";
                checklistText.text = "[ X ] Evacuated to high ground\n[ - ] Disabled main power breaker\n[ - ] Utilized emergency lighting";
            }
            return; 
        }

        if (!powerOff)
        {
            gradeText.text = "C";
            gradeText.color = Color.yellow;
            commentText.text = "Good effort, but a critical safety failure occurred.";
            recommendationText.text = "WARNING: Water + Electricity is deadly. Always find the breaker panel and turn it off immediately.";
            checklistText.text = "[ ✓ ] Evacuated to high ground\n[ X ] Disabled main power breaker\n" + (flashlightUsed ? "[ ✓ ]" : "[ X ]") + " Utilized emergency lighting";
            return;
        }

        if (!flashlightUsed)
        {
            gradeText.text = "B";
            gradeText.color = Color.green;
            commentText.text = "Strong survival skills, but room to improve visibility.";
            recommendationText.text = "TIP: Moving in absolute darkness leads to injuries. Always locate and equip your flashlight.";
            checklistText.text = "[ ✓ ] Evacuated to high ground\n[ ✓ ] Disabled main power breaker\n[ X ] Utilized emergency lighting";
            return;
        }

        gradeText.text = "A";
        gradeText.color = Color.green;
        commentText.text = "Outstanding. You followed all safety protocols.";
        recommendationText.text = "No critical warnings. Simulation passed perfectly.";
        checklistText.text = "[ ✓ ] Evacuated to high ground\n[ ✓ ] Disabled main power breaker\n[ ✓ ] Utilized emergency lighting";
    }

    public void NextSlide()
    {
        slide1_Overview.SetActive(false);
        slide2_Checklist.SetActive(true);
    }
}