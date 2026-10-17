using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EarthquakeTimer : MonoBehaviour
{
    [Header("Hub Link")]
    public SimulationManager simManager;

    [Header("UI Slider (Survival Limit)")]
    public Slider timeSlider;
    public Image sliderFillImage; 
    public float maxTime = 60f;
    private float currentTime;

    [Header("Damage Mechanics")]
    public float timePenaltyPerHit = 5f;
    [Tooltip("How many debris hits cause an instant Game Over?")]
    public int maxDebrisHits = 4; // Set to 4 or 5 in the Inspector
    private int currentDebrisHits = 0;

    private bool isTimerRunning = false;
    private Color originalColor;

    void Start()
    {
        currentTime = maxTime;
        if (timeSlider != null)
        {
            timeSlider.maxValue = maxTime;
            timeSlider.value = currentTime;
        }

        if (sliderFillImage != null)
        {
            originalColor = sliderFillImage.color;
        }
    }

    void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        UpdateSlider();

        // Natural timeout (Player failed to survive the duration)
        if (currentTime <= 0)
        {
            TriggerDefeat(SimulationManager.DeathCause.Crushed);
        }
    }

    public void StartTimer() => isTimerRunning = true;
    
    public void StopTimer() => isTimerRunning = false;

    // Called by the falling debris
    public void TakeDebrisDamage()
    {
        currentDebrisHits++;
        currentTime -= timePenaltyPerHit;
        UpdateSlider();
        
        Debug.Log($"Hit by debris! Hits: {currentDebrisHits}/{maxDebrisHits}. Time lost: {timePenaltyPerHit}s.");

        // Optional: Flash the slider red to show they took damage
        if (sliderFillImage != null) StartCoroutine(FlashRed());

        // The 4-5 Hit Instant Kill Rule
        if (currentDebrisHits >= maxDebrisHits)
        {
            Debug.Log("Hit limit reached! Fatal blow.");
            TriggerDefeat(SimulationManager.DeathCause.Crushed);
            return;
        }

        // Penalty caused the timer to drop below 0 early
        if (currentTime <= 0)
        {
            TriggerDefeat(SimulationManager.DeathCause.Crushed);
        }
    }

    // Called by the gas stove script
    public void TriggerGasExplosion()
    {
        Debug.Log("Gas Explosion! Survival Limit instantly depleted.");
        TriggerDefeat(SimulationManager.DeathCause.Explosion);
    }

    private void TriggerDefeat(SimulationManager.DeathCause cause)
    {
        currentTime = 0;
        UpdateSlider();
        isTimerRunning = false;

        if (simManager != null)
        {
            simManager.EndSimulation(false, cause);
        }
    }

    private void UpdateSlider()
    {
        if (timeSlider != null) timeSlider.value = currentTime;
    }

    private IEnumerator FlashRed()
    {
        sliderFillImage.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        sliderFillImage.color = originalColor;
    }
}