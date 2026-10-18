using UnityEngine;
using UnityEngine.UI;

public class EarthquakeTimer : MonoBehaviour
{
    [Header("Hub Link")]
    public SimulationManager simManager;

    [Header("UI Slider (Earthquake Duration)")]
    public Slider timeSlider;
    public float maxTime = 60f;
    private float currentTime;

    [Header("Damage Mechanics")]
    public DamagePulseUI damagePulseUI;
    public HitTextPopup hitTextPopup;
    public int maxDebrisHits = 4;
    private int currentDebrisHits = 0;

    public static bool IsEarthquakeActive = false;
    private bool isTimerRunning = false;

    void Start()
    {
        currentTime = maxTime;
        if (timeSlider != null)
        {
            timeSlider.maxValue = maxTime;
            timeSlider.value = currentTime;
        }
    }

    void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        UpdateSlider();

        // VICTORY! The player survived the duration of the earthquake
        if (currentTime <= 0)
        {
            currentTime = 0;
            isTimerRunning = false;
            UpdateSlider();
            if (simManager != null) simManager.EndSimulation(true); 
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
        IsEarthquakeActive = true; // The earthquake has officially begun!
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        IsEarthquakeActive = false;
    }

    // Called by the falling debris
    public void TakeDebrisDamage()
    {
        if (TableHiding.IsPlayerHiding) return;

        currentDebrisHits++;
        Debug.Log($"Hit! Strikes: {currentDebrisHits}/{maxDebrisHits}.");

        if (hitTextPopup != null) hitTextPopup.ShowHit();

        if (currentDebrisHits >= maxDebrisHits)
        {
            TriggerDefeat(SimulationManager.DeathCause.Crushed);
            return;
        }

        // Update the screen pulsing severity
        if (damagePulseUI != null) damagePulseUI.SetDamageState(currentDebrisHits);
    }

    // Called by the Inventory Manager
    public void ApplyFirstAid()
    {
        if (currentDebrisHits > 0)
        {
            currentDebrisHits--; // Remove a strike (no time added!)
            if (damagePulseUI != null) damagePulseUI.SetDamageState(currentDebrisHits);
            Debug.Log($"First Aid used. Strikes reduced to {currentDebrisHits}.");
        }
    }

    public bool HasTakenDamage() => currentDebrisHits > 0;

    public void TriggerGasExplosion() => TriggerDefeat(SimulationManager.DeathCause.Explosion);

    private void TriggerDefeat(SimulationManager.DeathCause cause)
    {
        isTimerRunning = false;
        IsEarthquakeActive = false;
        if (simManager != null) simManager.EndSimulation(false, cause);
    }

    private void UpdateSlider()
    {
        if (timeSlider != null) timeSlider.value = currentTime;
    }
}