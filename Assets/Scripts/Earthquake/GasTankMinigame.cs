
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GasTankMinigame : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public float interactionDistance = 3f;
    public Behaviour[] movementScriptsToDisable;

    [Header("Camera Lock")]
    public Behaviour[] cameraLookScriptsToDisable;
    public Transform cameraTransform;
    public Transform playerBody;

    private Quaternion savedCameraRotation;
    private Quaternion savedPlayerBodyRotation;
    private bool cameraRotationSaved;

    [Header("UI")]
    public Button openButton;
    public GameObject gasTankPanel;
    public Button closeButton;
    public TMP_Text statusText;

    [Header("Valve")]
    public RectTransform valveVisual;
    public Slider valveProgressSlider;

    [Tooltip("Degrees required to close the valve. For example, 540 = 1.5 turns.")]
    public float degreesToCloseValve = 540f;

    [Tooltip("Minimum pointer distance from the valve center.")]
    public float minValvePointerRadius = 10f;

    [Tooltip("Enable this to close by clockwise rotation.")]
    public bool clockwiseClosesValve = true;

    [Header("Hose")]
    public RectTransform hoseConnector;
    public Slider hoseProgressSlider;
    public Vector2 hosePullOffset = new Vector2(180f, 0f);
    public float hoseDragDistance = 300f;

    [Header("Completion")]
    public UnityEvent onGasSafetyCompleted;

    private bool panelOpen;
    private bool completed;
    private bool turningValve;
    private bool pullingHose;

    // Signed accumulated rotation: positive and negative directions differ.
    private float valveAngle;
    private float valveProgress;
    private float hoseProgress;

    private float previousPointerAngle;
    private bool hasPreviousPointerAngle;

    private Quaternion valveStartRotation;
    private Vector2 hoseStartPosition;

    private bool[] savedMovementStates;
    private bool[] savedCameraStates;

    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;
    private Coroutine completionRoutine;

    public bool IsPanelOpen => panelOpen;

    // Only the required direction can close the valve.
    public bool IsValveClosed =>
        clockwiseClosesValve
            ? valveAngle >= degreesToCloseValve
            : valveAngle <= -degreesToCloseValve;

    public bool IsHoseRemoved => hoseProgress >= 0.999f;

    private void Awake()
    {
        if (valveVisual != null)
            valveStartRotation = valveVisual.localRotation;

        if (hoseConnector != null)
            hoseStartPosition = hoseConnector.anchoredPosition;

        if (movementScriptsToDisable != null)
            savedMovementStates =
                new bool[movementScriptsToDisable.Length];

        if (cameraLookScriptsToDisable != null)
            savedCameraStates =
                new bool[cameraLookScriptsToDisable.Length];

        ConfigureSlider(valveProgressSlider);
        ConfigureSlider(hoseProgressSlider);
    }

    private void Start()
    {
        if (gasTankPanel != null)
            gasTankPanel.SetActive(false);

        if (openButton != null)
        {
            openButton.onClick.AddListener(OpenPanel);
            openButton.gameObject.SetActive(false);
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        UpdateValveVisual();
        UpdateHoseVisual();
        UpdateSliders();

        SetStatus("Rotate the valve clockwise to close the gas.");
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;
    }

    private void Update()
    {
        if (player == null)
            return;

        if (!panelOpen && !completed)
        {
            bool canInteract =
                Vector3.Distance(player.position, transform.position)
                <= interactionDistance;

            if (openButton != null &&
                openButton.gameObject.activeSelf != canInteract)
            {
                openButton.gameObject.SetActive(canInteract);
            }
        }

        if (panelOpen && cameraRotationSaved)
        {
            if (cameraTransform != null)
                cameraTransform.rotation = savedCameraRotation;

            if (playerBody != null)
                playerBody.rotation = savedPlayerBodyRotation;
        }
    }

    // ---------------- PANEL ----------------

    public void OpenPanel()
    {
        if (completed || panelOpen ||
            player == null || gasTankPanel == null)
            return;

        if (Vector3.Distance(player.position, transform.position)
            > interactionDistance)
            return;

        LockCameraRotation();

        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SaveAndDisableScripts(
            movementScriptsToDisable, savedMovementStates);

        SaveAndDisableScripts(
            cameraLookScriptsToDisable, savedCameraStates);

        panelOpen = true;
        gasTankPanel.SetActive(true);

        if (openButton != null)
            openButton.gameObject.SetActive(false);

        UpdateSliders();
        UpdateValveVisual();
        UpdateHoseVisual();
        RefreshInstructions();
    }

    public void ClosePanel()
    {
        if (!panelOpen)
            return;

        panelOpen = false;
        turningValve = false;
        pullingHose = false;
        hasPreviousPointerAngle = false;

        if (gasTankPanel != null)
            gasTankPanel.SetActive(false);

        RestoreCameraRotation();

        RestoreScripts(
            movementScriptsToDisable, savedMovementStates);

        RestoreScripts(
            cameraLookScriptsToDisable, savedCameraStates);

        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;
    }

    private void LockCameraRotation()
    {
        if (cameraTransform != null)
            savedCameraRotation = cameraTransform.rotation;

        if (playerBody != null)
            savedPlayerBodyRotation = playerBody.rotation;

        cameraRotationSaved = true;
    }

    private void RestoreCameraRotation()
    {
        if (!cameraRotationSaved)
            return;

        if (cameraTransform != null)
            cameraTransform.rotation = savedCameraRotation;

        if (playerBody != null)
            playerBody.rotation = savedPlayerBodyRotation;

        cameraRotationSaved = false;
    }

    // ---------------- VALVE ROTATION ----------------

    public bool BeginValveTurn(Vector2 pointerPosition)
    {
        if (!panelOpen || completed || IsHoseRemoved ||
            valveVisual == null)
            return false;

        turningValve = true;
        hasPreviousPointerAngle = false;

        UpdatePointerAngle(pointerPosition, true);

        SetStatus(
            clockwiseClosesValve
                ? "Rotate clockwise to close the valve."
                : "Rotate counterclockwise to close the valve.");

        return true;
    }

    public void DragValve(Vector2 pointerPosition)
    {
        if (!panelOpen || !turningValve || completed ||
            valveVisual == null)
            return;

        UpdatePointerAngle(pointerPosition, false);
    }

    private void UpdatePointerAngle(
        Vector2 pointerPosition, bool initialize)
    {
        Camera uiCamera = null;
        Canvas canvas = valveVisual.GetComponentInParent<Canvas>();

        if (canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector2 center = RectTransformUtility.WorldToScreenPoint(
            uiCamera, valveVisual.position);

        Vector2 direction = pointerPosition - center;

        if (direction.magnitude < minValvePointerRadius)
            return;

        float currentPointerAngle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (initialize || !hasPreviousPointerAngle)
        {
            previousPointerAngle = currentPointerAngle;
            hasPreviousPointerAngle = true;
            return;
        }

        float deltaAngle = Mathf.DeltaAngle(
            previousPointerAngle, currentPointerAngle);

        previousPointerAngle = currentPointerAngle;

        // Unity UI coordinates: positive angular movement is counterclockwise.
        // Invert the sign when clockwise should close the valve.
        float closingDirection = clockwiseClosesValve ? -1f : 1f;

        valveAngle += deltaAngle * closingDirection;

        // Clamp at fully open. The player must rotate in the closing
        // direction to increase progress.
        valveAngle = Mathf.Max(0f, valveAngle);

        valveProgress = Mathf.Clamp01(
            valveAngle / Mathf.Max(1f, degreesToCloseValve));

        UpdateValveVisual();
        UpdateSliders();

        if (IsValveClosed)
        {
            SetStatus("VALVE CLOSED! Now disconnect the hose.");
        }
        else if (valveAngle <= 0f)
        {
            SetStatus(
                clockwiseClosesValve
                    ? "Valve open. Rotate clockwise to close it."
                    : "Valve open. Rotate counterclockwise to close it.");
        }
        else
        {
            SetStatus(
                clockwiseClosesValve
                    ? "Keep rotating clockwise. Reverse to reopen."
                    : "Keep rotating counterclockwise. Reverse to reopen.");
        }
    }

    public void EndValveTurn()
    {
        turningValve = false;
        hasPreviousPointerAngle = false;
    }

    private void UpdateValveVisual()
    {
        if (valveVisual == null)
            return;

        float visualAngle = clockwiseClosesValve
            ? -valveAngle
            : valveAngle;

        valveVisual.localRotation =
            valveStartRotation *
            Quaternion.Euler(0f, 0f, visualAngle);
    }

    // ---------------- HOSE ----------------

    public bool BeginHosePull()
    {
        if (!panelOpen || completed || IsHoseRemoved)
            return false;

        if (!IsValveClosed)
        {
            SetStatus("Close the gas valve before removing the hose!");
            return false;
        }

        pullingHose = true;
        SetStatus("Pull the hose connector away from the tank.");

        return true;
    }

    public void DragHose(float horizontalDelta)
    {
        if (!panelOpen || !pullingHose || completed)
            return;

        if (!IsValveClosed)
        {
            pullingHose = false;
            SetStatus("Close the gas valve before removing the hose!");
            return;
        }

        hoseProgress = Mathf.Clamp01(
            hoseProgress +
            horizontalDelta / Mathf.Max(1f, hoseDragDistance));

        UpdateHoseVisual();
        UpdateSliders();

        if (IsHoseRemoved)
        {
            pullingHose = false;
            SetStatus("GAS SAFETY COMPLETE!");

            if (completionRoutine == null)
                completionRoutine = StartCoroutine(CompleteRoutine());
        }
        else
        {
            SetStatus("Pull the hose connector away from the tank.");
        }
    }

    public void EndHosePull()
    {
        pullingHose = false;
    }

    private void UpdateHoseVisual()
    {
        if (hoseConnector != null)
        {
            hoseConnector.anchoredPosition =
                hoseStartPosition + hosePullOffset * hoseProgress;
        }
    }

    // ---------------- SLIDERS ----------------

    private void UpdateSliders()
    {
        if (valveProgressSlider != null)
            valveProgressSlider.SetValueWithoutNotify(valveProgress);

        if (hoseProgressSlider != null)
            hoseProgressSlider.SetValueWithoutNotify(hoseProgress);
    }

    // ---------------- COMPLETION ----------------

    private IEnumerator CompleteRoutine()
    {
        completed = true;

        if (openButton != null)
            openButton.gameObject.SetActive(false);

        onGasSafetyCompleted?.Invoke();

        yield return new WaitForSecondsRealtime(1.5f);

        completionRoutine = null;
        ClosePanel();
    }

    private void RefreshInstructions()
    {
        if (!IsValveClosed)
            SetStatus(
                clockwiseClosesValve
                    ? "STEP 1: Rotate clockwise to close the valve."
                    : "STEP 1: Rotate counterclockwise to close the valve.");
        else if (!IsHoseRemoved)
            SetStatus("STEP 2: Pull the hose connector away from the tank.");
        else
            SetStatus("Gas safety complete!");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    // ---------------- SCRIPT STATE ----------------

    private void SaveAndDisableScripts(
        Behaviour[] scripts, bool[] savedStates)
    {
        if (scripts == null || savedStates == null)
            return;

        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i] == null)
                continue;

            savedStates[i] = scripts[i].enabled;
            scripts[i].enabled = false;
        }
    }

    private void RestoreScripts(
        Behaviour[] scripts, bool[] savedStates)
    {
        if (scripts == null || savedStates == null)
            return;

        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i] != null)
                scripts[i].enabled = savedStates[i];
        }
    }

    private void OnDestroy()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);

        if (panelOpen)
        {
            RestoreCameraRotation();

            RestoreScripts(
                movementScriptsToDisable, savedMovementStates);

            RestoreScripts(
                cameraLookScriptsToDisable, savedCameraStates);

            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;
        }
    }
}
