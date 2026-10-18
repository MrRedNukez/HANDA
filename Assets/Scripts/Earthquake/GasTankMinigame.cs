using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GasTankMinigame : MonoBehaviour
{
    [Header("Player Interaction")]
    public PlayerMovement playerMovementScript;
    public PlayerCamera playerCameraScript;

    [Header("Explosion Penalty")]
    public EarthquakeTimer timerSystem;
    public float explosionRadius = 7f;
    
    [Header("UI")]
    public Button openButton;
    public GameObject gasTankPanel;
    public Button closeButton;
    public TMP_Text statusText;

    [Header("Valve")]
    public RectTransform valveVisual;
    public Slider valveProgressSlider;
    public float degreesToCloseValve = 540f;
    public float minValvePointerRadius = 10f;
    public bool clockwiseClosesValve = true;

    [Header("Hose")]
    public RectTransform hoseConnector;
    public Slider hoseProgressSlider;
    public Vector2 hosePullOffset = new Vector2(180f, 0f);
    public float hoseDragDistance = 300f;
    
    [Tooltip("How fast the hose snaps back to the tank if you let go early.")]
    public float hoseSnapBackSpeed = 3f; 

    [Header("Completion")]
    public UnityEvent onGasSafetyCompleted;

    // --- NEW: GRADING HOOK ---
    [Tooltip("Becomes true when completed. ReportManager will read this.")]
    public bool hasSecuredGas { get; private set; }

    private bool panelOpen;
    private bool completed;
    private bool turningValve;
    private bool pullingHose;

    private float valveAngle;
    private float valveProgress;
    private float hoseProgress;

    private float previousPointerAngle;
    private bool hasPreviousPointerAngle;

    private Quaternion valveStartRotation;
    private Vector2 hoseStartPosition;
    
    
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;
    private Coroutine completionRoutine;
    private bool playerInRange = false;

    public bool IsValveClosed => clockwiseClosesValve ? valveAngle >= degreesToCloseValve : valveAngle <= -degreesToCloseValve;
    public bool IsHoseRemoved => hoseProgress >= 0.999f;

    private void Awake()
    {
        if (valveVisual != null) valveStartRotation = valveVisual.localRotation;
        if (hoseConnector != null) hoseStartPosition = hoseConnector.anchoredPosition;

        ConfigureSlider(valveProgressSlider);
        ConfigureSlider(hoseProgressSlider);
    }

    private void Start()
    {
        if (gasTankPanel != null) gasTankPanel.SetActive(false);
        if (openButton != null)
        {
            openButton.onClick.AddListener(OpenPanel);
            openButton.gameObject.SetActive(false);
        }
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);

        UpdateValveVisual();
        UpdateHoseVisual();
        UpdateSliders();
        SetStatus("Rotate the valve clockwise to close the gas.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !completed)
        {
            playerInRange = true;
            
            // FIX: Only show the button if the earthquake HAS NOT started
            if (!panelOpen && openButton != null && !EarthquakeTimer.IsEarthquakeActive) 
            {
                openButton.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (openButton != null) openButton.gameObject.SetActive(false);
            
            if (panelOpen) ClosePanel(); 
        }
    }

    private void Update()
    {
        if (EarthquakeTimer.IsEarthquakeActive)
        {
            if (panelOpen) ClosePanel();

            if (openButton != null && openButton.gameObject.activeSelf) 
            {
                openButton.gameObject.SetActive(false);
            }
        }
        else
        {
            if (playerInRange && !completed && !panelOpen && openButton != null && !openButton.gameObject.activeSelf)
            {
                openButton.gameObject.SetActive(true);
            }
        }

        // --- 3. HOSE SNAP-BACK TENSION ---
        if (!pullingHose && hoseProgress > 0f && hoseProgress < 1f && IsValveClosed)
        {
            hoseProgress -= Time.deltaTime * hoseSnapBackSpeed;
            hoseProgress = Mathf.Clamp01(hoseProgress);
            UpdateHoseVisual();
            UpdateSliders();
        }
    }

    // ---------------- PANEL ----------------
    public void OpenPanel()
    {
        if (completed || panelOpen || gasTankPanel == null) return;

        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMovementScript != null) playerMovementScript.canMove = false;
        if (playerCameraScript != null) playerCameraScript.canLook = false;

        panelOpen = true;
        gasTankPanel.SetActive(true);
        if (openButton != null) openButton.gameObject.SetActive(false);

        UpdateSliders();
        UpdateValveVisual();
        UpdateHoseVisual();
        RefreshInstructions();
    }

    public void ClosePanel()
    {
        if (!panelOpen) return;

        panelOpen = false;
        turningValve = false;
        pullingHose = false;
        hasPreviousPointerAngle = false;

        if (gasTankPanel != null) gasTankPanel.SetActive(false);

        if (playerMovementScript != null) playerMovementScript.canMove = true;
        if (playerCameraScript != null) playerCameraScript.canLook = true;

        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;

        // Give them the open button back if they are still standing near it and the quake hasn't started
        if (playerInRange && !completed && openButton != null && !EarthquakeTimer.IsEarthquakeActive)
        {
            openButton.gameObject.SetActive(true);
        }
    }

    // ---------------- VALVE ROTATION ----------------
    public bool BeginValveTurn(Vector2 pointerPosition)
    {
        if (!panelOpen || completed || IsHoseRemoved || valveVisual == null) return false;
        turningValve = true;
        hasPreviousPointerAngle = false;
        UpdatePointerAngle(pointerPosition, true);
        return true;
    }

    public void DragValve(Vector2 pointerPosition)
    {
        if (!panelOpen || !turningValve || completed || valveVisual == null) return;
        UpdatePointerAngle(pointerPosition, false);
    }

    private void UpdatePointerAngle(Vector2 pointerPosition, bool initialize)
    {
        Camera uiCamera = null;
        Canvas canvas = valveVisual.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector2 center = RectTransformUtility.WorldToScreenPoint(uiCamera, valveVisual.position);
        Vector2 direction = pointerPosition - center;

        if (direction.magnitude < minValvePointerRadius) return;

        float currentPointerAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (initialize || !hasPreviousPointerAngle)
        {
            previousPointerAngle = currentPointerAngle;
            hasPreviousPointerAngle = true;
            return;
        }

        float deltaAngle = Mathf.DeltaAngle(previousPointerAngle, currentPointerAngle);
        previousPointerAngle = currentPointerAngle;

        float closingDirection = clockwiseClosesValve ? -1f : 1f;
        valveAngle += deltaAngle * closingDirection;
        valveAngle = Mathf.Max(0f, valveAngle);

        valveProgress = Mathf.Clamp01(valveAngle / Mathf.Max(1f, degreesToCloseValve));

        UpdateValveVisual();
        UpdateSliders();

        if (IsValveClosed) SetStatus("VALVE CLOSED! Now disconnect the hose.");
        else if (valveAngle <= 0f) SetStatus(clockwiseClosesValve ? "Valve open. Rotate clockwise to close it." : "Valve open. Rotate counterclockwise to close it.");
        else SetStatus(clockwiseClosesValve ? "Keep rotating clockwise. Reverse to reopen." : "Keep rotating counterclockwise. Reverse to reopen.");
    }

    public void EndValveTurn() 
    { 
        turningValve = false; 
        hasPreviousPointerAngle = false; 
    }

    private void UpdateValveVisual()
    {
        if (valveVisual == null) return;
        float visualAngle = clockwiseClosesValve ? -valveAngle : valveAngle;
        valveVisual.localRotation = valveStartRotation * Quaternion.Euler(0f, 0f, visualAngle);
    }

    // ---------------- HOSE ----------------
    public bool BeginHosePull()
    {
        if (!panelOpen || completed || IsHoseRemoved) return false;
        if (!IsValveClosed) { SetStatus("Close the gas valve before removing the hose!"); return false; }
        pullingHose = true;
        SetStatus("Pull the hose connector away from the tank.");
        return true;
    }

    public void DragHose(float horizontalDelta)
    {
        if (!panelOpen || !pullingHose || completed) return;
        if (!IsValveClosed) { pullingHose = false; SetStatus("Close the gas valve before removing the hose!"); return; }

        hoseProgress = Mathf.Clamp01(hoseProgress + horizontalDelta / Mathf.Max(1f, hoseDragDistance));
        UpdateHoseVisual();
        UpdateSliders();

        if (IsHoseRemoved)
        {
            pullingHose = false;
            SetStatus("GAS SAFETY COMPLETE!");
            if (completionRoutine == null) completionRoutine = StartCoroutine(CompleteRoutine());
        }
        else SetStatus("Pull the hose connector away from the tank.");
    }

    public void EndHosePull() 
    { 
        pullingHose = false; 
    }

    private void UpdateHoseVisual()
    {
        if (hoseConnector != null) hoseConnector.anchoredPosition = hoseStartPosition + hosePullOffset * hoseProgress;
    }

    // ---------------- UTILS ----------------
    private void ConfigureSlider(Slider slider)
    {
        if (slider != null) { slider.minValue = 0f; slider.maxValue = 1f; slider.wholeNumbers = false; slider.interactable = false; }
    }

    private void UpdateSliders()
    {
        if (valveProgressSlider != null) valveProgressSlider.SetValueWithoutNotify(valveProgress);
        if (hoseProgressSlider != null) hoseProgressSlider.SetValueWithoutNotify(hoseProgress);
    }

    private IEnumerator CompleteRoutine()
    {
        completed = true;
        hasSecuredGas = true; 
        
        if (openButton != null) openButton.gameObject.SetActive(false);
        onGasSafetyCompleted?.Invoke();
        yield return new WaitForSecondsRealtime(1.5f);
        completionRoutine = null;
        ClosePanel();
    }

    private void RefreshInstructions()
    {
        if (!IsValveClosed) SetStatus(clockwiseClosesValve ? "STEP 1: Rotate clockwise to close the valve." : "STEP 1: Rotate counterclockwise to close the valve.");
        else if (!IsHoseRemoved) SetStatus("STEP 2: Pull the hose connector away from the tank.");
        else SetStatus("Gas safety complete!");
    }

    private void SetStatus(string message) { if (statusText != null) statusText.text = message; }

    private void SaveAndDisableScripts(Behaviour[] scripts, bool[] savedStates)
    {
        if (scripts == null || savedStates == null) return;
        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i] == null) continue;
            savedStates[i] = scripts[i].enabled;
            scripts[i].enabled = false;
        }
    }

    private void RestoreScripts(Behaviour[] scripts, bool[] savedStates)
    {
        if (scripts == null || savedStates == null) return;
        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i] != null) scripts[i].enabled = savedStates[i];
        }
    }

    public void TriggerAftershockCheck()
    {
        if (hasSecuredGas) return;

        Debug.Log("The aftershock ignited the open gas valve!");
        // if (explosionEffect != null) explosionEffect.Play();

        // Check if the player is caught in the blast radius
        float distance = Vector3.Distance(transform.position, playerMovementScript.transform.position);
        if (distance <= explosionRadius)
        {
            if (timerSystem != null) timerSystem.TriggerGasExplosion();
        }
    }
}