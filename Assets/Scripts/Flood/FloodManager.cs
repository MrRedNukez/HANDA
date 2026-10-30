using UnityEngine;
using UnityEngine.UI;

public class FloodManager : MonoBehaviour
{
    public enum FloodState { WaitingForDay2, Flooding, Ended }
    
    [Header("Current State")]
    public FloodState currentState = FloodState.WaitingForDay2;

    [Header("UI References")]
    public Slider waterSlider;

    [Header("Environment Effects")]
    public GameObject rainParticles; // NEW: Drag your empty rain object here

    [Header("Hub Reference")]
    public SimulationManager simManager;

    [Header("Physics Water Reference")]
    public FloodBuoyancy physicalWater;

    void Start()
    {
        Time.timeScale = 1f; 
        
        if (waterSlider != null)
        {
            waterSlider.minValue = 0f;
            waterSlider.maxValue = 100f;
            waterSlider.value = 0f;
            
            // Hide the slider on Day 1
            waterSlider.gameObject.SetActive(false); 
        }

        // Ensure rain is off during the calm phase
        if (rainParticles != null)
        {
            rainParticles.SetActive(false);
        }
    }

    void Update()
    {
        if (currentState == FloodState.Flooding)
        {
            HandleFlooding();
        }
    }

    public void StartFloodEvent()
    {
        if (currentState == FloodState.WaitingForDay2)
        {
            currentState = FloodState.Flooding;
            
            // Show the slider and start the rain exactly when Day 2 begins
            if (waterSlider != null) waterSlider.gameObject.SetActive(true);
            if (rainParticles != null) rainParticles.SetActive(true);
            
            if (physicalWater != null)
            {
                physicalWater.StartFlood(); 
            }
        }
    }

    private void HandleFlooding()
    {
        if (waterSlider != null && physicalWater != null) 
        {
            waterSlider.value = physicalWater.GetFloodProgress() * 100f;
        }

        if (physicalWater != null && physicalWater.GetFloodProgress() >= 1f)
        {
            TriggerSurvival();
        }
    }

    public void TriggerDrowningDeath()
    {
        if (currentState == FloodState.Ended) return;

        currentState = FloodState.Ended;
        
        if (simManager != null) simManager.EndSimulation(false);
    }

    private void TriggerSurvival()
    {
        if (currentState == FloodState.Ended) return; 
        
        currentState = FloodState.Ended;
        
        if (simManager != null) simManager.EndSimulation(true);
    }
}