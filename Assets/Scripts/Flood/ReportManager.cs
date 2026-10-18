using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ReportManager : MonoBehaviour
{
    private enum DisasterType { Flood, Earthquake }

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

    [TextArea(2, 3)]
    public string[] crushedQuotes = {
        "Debris does not yield to those standing in the open.",
        "A sturdy table could have been the difference between life and death.",
        "When the earth shakes, the sky falls."
    };

    [TextArea(2, 3)]
    public string[] explosionQuotes = {
        "A single spark in a gas-filled room is all it takes.",
        "The shaking stopped, but the true danger had just begun.",
        "Unsecured hazards turn natural disasters into man-made tragedies."
    };

    [Header("Slide 1 Text")]
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI commentText;
    public TextMeshProUGUI recommendationText;

    [Header("Slide 2 Text")]
    public TextMeshProUGUI checklistText;

    // --- PUBLIC TRIGGERS ---
    
    public void TriggerFloodEvaluation(bool survived, SimulationManager.DeathCause deathCause, bool powerOff, bool flashlightUsed)
    {
        StartCoroutine(EvaluationSequence(DisasterType.Flood, survived, deathCause, powerOff, flashlightUsed, false, false));
    }

    public void TriggerEarthquakeEvaluation(bool survived, SimulationManager.DeathCause deathCause, bool duckAndCovered, bool gasSecured)
    {
        StartCoroutine(EvaluationSequence(DisasterType.Earthquake, survived, deathCause, false, false, duckAndCovered, gasSecured));
    }

    // --- CORE SEQUENCE ---

    private IEnumerator EvaluationSequence(DisasterType disaster, bool survived, SimulationManager.DeathCause deathCause, 
        bool floodPowerOff, bool floodFlashlightUsed, bool eqDuckCovered, bool eqGasSecured)
    {
        Time.timeScale = 0f; 

        if (!survived)
        {
            if (quoteTextUI != null)
            {
                if (deathCause == SimulationManager.DeathCause.Electrocution && electrocutionQuotes.Length > 0)
                    quoteTextUI.text = electrocutionQuotes[Random.Range(0, electrocutionQuotes.Length)];
                else if (deathCause == SimulationManager.DeathCause.Explosion && explosionQuotes.Length > 0)
                    quoteTextUI.text = explosionQuotes[Random.Range(0, explosionQuotes.Length)];
                else if (deathCause == SimulationManager.DeathCause.Crushed && crushedQuotes.Length > 0)
                    quoteTextUI.text = crushedQuotes[Random.Range(0, crushedQuotes.Length)];
                else if (drowningQuotes.Length > 0) // Default to drowning for flood
                    quoteTextUI.text = drowningQuotes[Random.Range(0, drowningQuotes.Length)];
            }

            quotePanel.alpha = 1;
            quotePanel.gameObject.SetActive(true);
            reportPanel.gameObject.SetActive(false);
            
            yield return new WaitForSecondsRealtime(5.0f); 
            
            quotePanel.gameObject.SetActive(false);
        }

        if (disaster == DisasterType.Flood)
            CalculateFloodGrade(survived, deathCause, floodPowerOff, floodFlashlightUsed);
        else if (disaster == DisasterType.Earthquake)
            CalculateEarthquakeGrade(survived, deathCause, eqDuckCovered, eqGasSecured);
        
        reportPanel.gameObject.SetActive(true);
        slide1_Overview.SetActive(true);
        slide2_Checklist.SetActive(false);
    }

    // --- GRADING LOGIC ---

    private void CalculateFloodGrade(bool survived, SimulationManager.DeathCause deathCause, bool powerOff, bool flashlightUsed)
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
            else 
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
            checklistText.text = "[ O ] Evacuated to high ground\n[ X ] Disabled main power breaker\n" + (flashlightUsed ? "[ O ]" : "[ X ]") + " Utilized emergency lighting";
            return;
        }

        if (!flashlightUsed)
        {
            gradeText.text = "B";
            gradeText.color = Color.green;
            commentText.text = "Strong survival skills, but room to improve visibility.";
            recommendationText.text = "TIP: Moving in absolute darkness leads to injuries. Always locate and equip your flashlight.";
            checklistText.text = "[ O ] Evacuated to high ground\n[ O ] Disabled main power breaker\n[ X ] Utilized emergency lighting";
            return;
        }

        gradeText.text = "A";
        gradeText.color = Color.green;
        commentText.text = "Outstanding. You followed all safety protocols.";
        recommendationText.text = "No critical warnings. Simulation passed perfectly.";
        checklistText.text = "[ O ] Evacuated to high ground\n[ O ] Disabled main power breaker\n[ O ] Utilized emergency lighting";
    }

    private void CalculateEarthquakeGrade(bool survived, SimulationManager.DeathCause deathCause, bool duckAndCovered, bool gasSecured)
    {
        if (!survived)
        {
            gradeText.text = "F";
            gradeText.color = Color.red;

            if (deathCause == SimulationManager.DeathCause.Explosion)
            {
                commentText.text = "Simulation Failed. Player caught in a gas explosion.";
                recommendationText.text = "FATAL ERROR: You left the gas valve open during the aftershock. Always secure fire hazards immediately.";
                checklistText.text = "[ " + (duckAndCovered ? "O" : "X") + " ] Duck, Cover, and Hold\n[ X ] Secure the Gas Valve";
            }
            else 
            {
                commentText.text = "Simulation Failed. Player was crushed by falling debris.";
                recommendationText.text = "WARNING: Do not stand in the open. Always find a sturdy table to Duck, Cover, and Hold On.";
                checklistText.text = "[ X ] Duck, Cover, and Hold\n[ " + (gasSecured ? "O" : "-") + " ] Secure the Gas Valve";
            }
            return; 
        }

        if (!gasSecured)
        {
            gradeText.text = "C";
            gradeText.color = Color.yellow;
            commentText.text = "You survived, but a critical safety failure occurred.";
            recommendationText.text = "WARNING: Leaving a gas line open is a massive fire hazard. Always secure the tank when the shaking pauses.";
            checklistText.text = "[ O ] Duck, Cover, and Hold\n[ X ] Secure the Gas Valve";
            return;
        }

        if (!duckAndCovered)
        {
            gradeText.text = "B";
            gradeText.color = Color.green;
            commentText.text = "You survived the hazard, but took unnecessary physical damage.";
            recommendationText.text = "TIP: Taking debris to the head is dangerous. Always utilize sturdy furniture for cover.";
            checklistText.text = "[ X ] Duck, Cover, and Hold\n[ O ] Secure the Gas Valve";
            return;
        }

        gradeText.text = "A";
        gradeText.color = Color.green;
        commentText.text = "Outstanding. You followed all safety protocols.";
        recommendationText.text = "No critical warnings. Simulation passed perfectly.";
        checklistText.text = "[ O ] Duck, Cover, and Hold\n[ O ] Secure the Gas Valve";
    }

    public void NextSlide()
    {
        slide1_Overview.SetActive(false);
        slide2_Checklist.SetActive(true);
    }

    public void RestartSimulation()
    {
        Time.timeScale = 1f; 

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; 

        SceneManager.LoadScene("MainMenu"); 
    }
}