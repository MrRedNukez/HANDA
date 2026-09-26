using UnityEngine;
using UnityEngine.UI;

public class FloodManager : MonoBehaviour
{
    public enum FloodState { Preparation, Flooding, Ended }
    
    [Header("Current State")]
    public FloodState currentState = FloodState.Preparation;

    [Header("UI References")]
    public Slider waterSlider;
    public CanvasGroup redAlertGroup;
    //public GameObject deathPanel; 

    [Header("Hub Reference")]
    public SimulationManager simManager;

    [Header("Physics Water Reference")]
    public FloodBuoyancy physicalWater;

    [Header("Time Settings")]
    public float prepTime = 30f;

    [Header("Alert Settings")]
    public float maxAlertOpacity = 0.3f; 
    public float pulseSpeed = 1f;
    public float alertDuration = 3f;

    private float currentPrepTime;
    private float currentAlertTime;

    void Start()
    {
        Time.timeScale = 1f; 
        currentPrepTime = prepTime;
        
        if (waterSlider != null)
        {
            waterSlider.minValue = 0f;
            waterSlider.maxValue = 100f;
            waterSlider.value = 0f;
        }

        if (redAlertGroup != null)
        {
            redAlertGroup.alpha = 0f; 
            redAlertGroup.gameObject.SetActive(false);
        }

        //if (deathPanel != null) deathPanel.SetActive(false);
    }

    void Update()
    {
        if (currentState == FloodState.Preparation)
        {
            HandlePreparation();
        }
        else if (currentState == FloodState.Flooding)
        {
            HandleFlooding();
        }
    }

    private void HandlePreparation()
    {
        currentPrepTime -= Time.deltaTime;
        
        if (currentPrepTime <= 0)
        {
            currentState = FloodState.Flooding;
            
            if (physicalWater != null)
            {
                physicalWater.StartFlood(); 
            }

            if (redAlertGroup != null)
            {
                redAlertGroup.gameObject.SetActive(true);
                currentAlertTime = alertDuration;
            }
        }
    }

    private void HandleFlooding()
    {
        if (waterSlider != null && physicalWater != null) 
        {
            waterSlider.value = physicalWater.GetFloodProgress() * 100f;
        }

        if (redAlertGroup != null && currentAlertTime > 0)
        {
            currentAlertTime -= Time.deltaTime;
            redAlertGroup.alpha = Mathf.PingPong(Time.time * pulseSpeed, maxAlertOpacity);
            
            if (currentAlertTime <= 0)
            {
                redAlertGroup.alpha = 0f;
                redAlertGroup.gameObject.SetActive(false);
            }
        }

        if (physicalWater != null && physicalWater.GetFloodProgress() >= 1f)
        {
            currentState = FloodState.Ended;
            TriggerSurvival();
        }
    }

    public void TriggerDrowningDeath()
    {
        if (currentState == FloodState.Ended) return;

        currentState = FloodState.Ended;
        Debug.Log("Game Over: Player Drowned!");
        
        // Tell the hub the simulation ended, and they DID NOT survive (false)
        if (simManager != null) simManager.EndSimulation(false);
    }

    private void TriggerSurvival()
    {
        // Add a safety check here so it doesn't trigger if they already drowned
        if (currentState == FloodState.Ended) return; 
        
        currentState = FloodState.Ended;
        Debug.Log("Simulation Success: Player Survived the flood peak!");
        
        // Tell the hub the simulation ended, and they SURVIVED (true)
        if (simManager != null) simManager.EndSimulation(true);
    }
}