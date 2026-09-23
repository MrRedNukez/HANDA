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
    public GameObject deathPanel; 

    [Header("Physics & Player References")]
    public FloodBuoyancy physicalWater;
    public Transform playerCamera;

    [Header("Time Settings")]
    public float prepTime = 30f;

    [Header("Alert Settings")]
    public float maxAlertOpacity = 0.3f; 
    public float pulseSpeed = 1f;
    public float alertDuration = 3f;

    [Header("Survival Settings")]
    public float timeToDrown = 3f;

    private float currentPrepTime;
    private float currentAlertTime;
    private float currentDrownTime; 

    void Start()
    {
        Time.timeScale = 1f; 

        currentPrepTime = prepTime;
        currentDrownTime = timeToDrown; 
        
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

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case FloodState.Preparation:
                HandlePreparation();
                break;
            case FloodState.Flooding:
                HandleFlooding();
                break;
            case FloodState.Ended:
                break;
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
        float progress = physicalWater.GetFloodProgress();
        if (waterSlider != null) 
        {
            waterSlider.value = progress * 100f;
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

        if (physicalWater.GetCurrentHeight() >= playerCamera.position.y)
        {
            currentDrownTime -= Time.deltaTime;
            
            if (currentDrownTime <= 0)
            {
                currentState = FloodState.Ended;
                TriggerDrowningDeath();
                return;
            }
        }
        else
        {
            currentDrownTime = timeToDrown;
        }

        if (progress >= 1f)
        {
            currentState = FloodState.Ended;
            TriggerSurvival();
        }
    }

    private void TriggerDrowningDeath()
    {
        Debug.Log("Game Over: Player Drowned!");
        
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            Time.timeScale = 0f; 
        }
    }

    private void TriggerSurvival()
    {
        Debug.Log("Simulation Success: Player Survived the flood peak!");
    }
}