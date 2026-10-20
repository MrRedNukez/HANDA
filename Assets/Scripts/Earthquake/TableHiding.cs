
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TableHiding : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Tooltip("Add PlayerMovement here. CharacterController is optional.")]
    public Behaviour[] movementScriptsToDisable;

    [Header("Table Positions")]
    public Transform hidePoint;
    public Transform exitPoint;

    [Tooltip("The table root Transform that moves during the earthquake. " +
             "If empty, this object's Transform will be used.")]
    public Transform tableToLock;

    [Tooltip("Extra distance to move downward from the Hide Point.")]
    [Range(0f, 1f)]
    public float lowerHidingAmount = 0.2f;

    [Header("Shared UI - Reuse for every table")]
    public Button interactionButton;
    public Text buttonLabel;

    [Header("Active Hold System (Desktop & Mobile)")]
    public GameObject gripUIPanel;
    public TextMeshProUGUI gripStatusText;
    public KeyCode pcGripKey = KeyCode.Space;

    [Tooltip("How long the player can release grip before being forced out.")]
    public float gracePeriodBeforeSlip = 0.5f;

    private bool isPointerHoldingGrip = false;
    private bool isGripCurrentlyHeld = false;
    private float slipTimer = 0f;

    [Header("Table Lock During Grip")]
    [Tooltip("Lock the table in its original hiding position while gripping.")]
    public bool lockTableWhileHolding = true;

    private Vector3 savedTablePosition;
    private Quaternion savedTableRotation;
    private bool savedTablePose;

    [Header("Black Screen")]
    public CanvasGroup blackScreen;
    public float fadeDuration = 0.25f;

    [Header("Ghost Mode")]
    public bool disablePlayerColliders = true;
    public bool freezePlayerRigidbody = true;

    [Header("Grading Data")]
    public bool hasSuccessfullyHidden = false;
    public static bool IsPlayerHiding { get; private set; }

    private static TableHiding candidateTarget;
    private static TableHiding activeTable;
    private static Button sharedButton;
    private static Text sharedLabel;

    private static int uiUpdatedFrame = -1;
    private static bool isTransitioning;

    private bool isHidden;
    private bool isFading;

    private bool[] savedMovementStates;
    private Collider[] playerColliders;
    private bool[] savedColliderStates;

    private Rigidbody playerRigidbody;
    private bool savedIsKinematic;
    private bool savedUseGravity;
    private Vector3 savedVelocity;
    private Vector3 savedAngularVelocity;
    private bool rigidbodyStateSaved;

    private void Awake()
    {
        if (movementScriptsToDisable != null)
        {
            savedMovementStates =
                new bool[movementScriptsToDisable.Length];
        }

        if (tableToLock == null)
        {
            tableToLock = transform;
        }
    }

    private void Start()
    {
        if (player != null)
        {
            playerColliders =
                player.GetComponentsInChildren<Collider>(true);

            savedColliderStates =
                new bool[playerColliders.Length];

            playerRigidbody = player.GetComponent<Rigidbody>();

            if (playerRigidbody == null)
            {
                playerRigidbody =
                    player.GetComponentInChildren<Rigidbody>();
            }
        }

        if (interactionButton != null)
        {
            sharedButton = interactionButton;
            sharedLabel = buttonLabel;

            sharedButton.onClick.RemoveListener(
                HandleSharedButtonClick);

            sharedButton.onClick.AddListener(
                HandleSharedButtonClick);

            sharedButton.gameObject.SetActive(false);
        }

        if (blackScreen != null)
        {
            blackScreen.alpha = 0f;
            blackScreen.blocksRaycasts = false;
            blackScreen.interactable = false;
        }

        if (gripUIPanel != null)
        {
            gripUIPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (activeTable == this &&
            isHidden &&
            !isTransitioning)
        {
            UpdateGripLogic();
        }
    }

    private bool IsGripInputHeld()
    {
        return Input.GetKey(pcGripKey) || isPointerHoldingGrip;
    }

    // Grip input controls both the status and the table lock.
    private void UpdateGripLogic()
    {
        isGripCurrentlyHeld = IsGripInputHeld();

        if (isGripCurrentlyHeld)
        {
            slipTimer = 0f;
            IsPlayerHiding = true;

            if (gripStatusText != null)
            {
                gripStatusText.text = "HOLDING FIRM!";
                gripStatusText.color = Color.green;
            }
        }
        else
        {
            IsPlayerHiding = false;

            if (gripStatusText != null)
            {
                gripStatusText.text = "SLIPPING! GRIP IT!";
                gripStatusText.color = Color.red;
            }

            slipTimer += Time.deltaTime;

            if (slipTimer >= gracePeriodBeforeSlip)
            {
                ExitTable();
            }
        }
    }

    // Mobile UI button: connect Pointer Down and Pointer Up to these.
    public void OnGripPointerDown()
    {
        isPointerHoldingGrip = true;
    }

    public void OnGripPointerUp()
    {
        isPointerHoldingGrip = false;
    }

    // Keep the table at its saved position while the player grips it.
    // LateUpdate helps override ordinary Update-based earthquake motion.
    private void ApplyTableLock()
    {
        if (!lockTableWhileHolding ||
            !savedTablePose ||
            tableToLock == null ||
            activeTable != this ||
            !isHidden ||
            isTransitioning)
        {
            return;
        }

        if (IsGripInputHeld())
        {
            tableToLock.position = savedTablePosition;
            tableToLock.rotation = savedTableRotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") &&
            !isTransitioning &&
            activeTable == null)
        {
            candidateTarget = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") &&
            candidateTarget == this)
        {
            candidateTarget = null;
        }
    }

    private void LateUpdate()
    {
        ApplyTableLock();

        if (uiUpdatedFrame == Time.frameCount) return;
        uiUpdatedFrame = Time.frameCount;

        if (sharedButton == null) return;

        bool hiding = activeTable != null;

        if (hiding)
        {
            if (sharedButton.gameObject.activeSelf)
            {
                sharedButton.gameObject.SetActive(false);
            }

            return;
        }

        TableHiding target = candidateTarget;
        bool showButton = target != null;

        if (sharedButton.gameObject.activeSelf != showButton)
        {
            sharedButton.gameObject.SetActive(showButton);
        }

        if (sharedLabel != null && showButton)
        {
            sharedLabel.text = "HIDE UNDER TABLE";
        }

        sharedButton.interactable =
            showButton && !isTransitioning;
    }

    private static void HandleSharedButtonClick()
    {
        if (isTransitioning) return;

        if (activeTable == null && candidateTarget != null)
        {
            candidateTarget.EnterTable();
        }
    }

    private void EnterTable()
    {
        if (isFading ||
            activeTable != null ||
            player == null ||
            hidePoint == null)
        {
            return;
        }

        // Save the table's position before the earthquake can
        // move it away from the player's hiding spot.
        if (tableToLock != null)
        {
            savedTablePosition = tableToLock.position;
            savedTableRotation = tableToLock.rotation;
            savedTablePose = true;
        }

        activeTable = this;
        isHidden = true;
        hasSuccessfullyHidden = true;

        isGripCurrentlyHeld = false;
        IsPlayerHiding = false;
        slipTimer = 0f;

        StartCoroutine(EnterRoutine());
    }

    private IEnumerator EnterRoutine()
    {
        isFading = true;
        isTransitioning = true;

        yield return StartCoroutine(FadeToBlack());

        SaveAndDisableMovement();
        EnableGhostMode();

        player.position =
            hidePoint.position - Vector3.up * lowerHidingAmount;

        yield return StartCoroutine(FadeFromBlack());

        if (EarthquakeTimer.IsEarthquakeActive)
        {
            isTransitioning = false;
            isFading = false;

            if (gripUIPanel != null)
            {
                gripUIPanel.SetActive(true);
            }

            isPointerHoldingGrip = false;
            isGripCurrentlyHeld = false;
            slipTimer = 0f;

            if (gripStatusText != null)
            {
                gripStatusText.text = "SLIPPING! GRIP IT!";
                gripStatusText.color = Color.red;
            }
        }
        else
        {
            if (gripUIPanel != null)
            {
                gripUIPanel.SetActive(false);
            }

            isTransitioning = false;
            isFading = false;

            Debug.Log(
                "The earthquake hasn't started yet! " +
                "Waiting out here is useless.");

            yield return new WaitForSeconds(0.5f);

            ExitTable();
        }
    }

    private void ExitTable()
    {
        if (isFading || !isHidden || exitPoint == null)
        {
            return;
        }

        StartCoroutine(ExitRoutine());
    }

    private IEnumerator ExitRoutine()
    {
        isFading = true;
        isTransitioning = true;

        if (gripUIPanel != null)
        {
            gripUIPanel.SetActive(false);
        }

        IsPlayerHiding = false;
        isPointerHoldingGrip = false;
        isGripCurrentlyHeld = false;
        slipTimer = 0f;

        yield return StartCoroutine(FadeToBlack());

        player.position = exitPoint.position;

        DisableGhostMode();
        RestoreMovement();

        isHidden = false;
        activeTable = null;

        // The table is no longer locked after leaving.
        // Its earthquake animation can resume normally.
        savedTablePose = false;

        yield return StartCoroutine(FadeFromBlack());

        isTransitioning = false;
        isFading = false;
    }

    private void EnableGhostMode()
    {
        if (disablePlayerColliders && playerColliders != null)
        {
            for (int i = 0; i < playerColliders.Length; i++)
            {
                Collider col = playerColliders[i];
                if (col == null) continue;

                savedColliderStates[i] = col.enabled;
                col.enabled = false;
            }
        }

        if (freezePlayerRigidbody && playerRigidbody != null)
        {
            savedIsKinematic = playerRigidbody.isKinematic;
            savedUseGravity = playerRigidbody.useGravity;
            savedVelocity = playerRigidbody.linearVelocity;
            savedAngularVelocity = playerRigidbody.angularVelocity;

            rigidbodyStateSaved = true;

            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.useGravity = false;
            playerRigidbody.isKinematic = true;
        }
    }

    private void DisableGhostMode()
    {
        if (playerRigidbody != null && rigidbodyStateSaved)
        {
            playerRigidbody.isKinematic = savedIsKinematic;
            playerRigidbody.useGravity = savedUseGravity;

            if (!savedIsKinematic)
            {
                playerRigidbody.linearVelocity = savedVelocity;
                playerRigidbody.angularVelocity = savedAngularVelocity;
            }

            rigidbodyStateSaved = false;
        }

        if (disablePlayerColliders &&
            playerColliders != null &&
            savedColliderStates != null)
        {
            for (int i = 0; i < playerColliders.Length; i++)
            {
                Collider col = playerColliders[i];

                if (col != null)
                {
                    col.enabled = savedColliderStates[i];
                }
            }
        }
    }

    private void SaveAndDisableMovement()
    {
        if (movementScriptsToDisable == null) return;

        for (int i = 0; i < movementScriptsToDisable.Length; i++)
        {
            Behaviour script = movementScriptsToDisable[i];
            if (script == null) continue;

            savedMovementStates[i] = script.enabled;
            script.enabled = false;
        }
    }

    private void RestoreMovement()
    {
        if (movementScriptsToDisable == null ||
            savedMovementStates == null)
        {
            return;
        }

        for (int i = 0; i < movementScriptsToDisable.Length; i++)
        {
            Behaviour script = movementScriptsToDisable[i];

            if (script != null)
            {
                script.enabled = savedMovementStates[i];
            }
        }
    }

    private IEnumerator FadeToBlack()
    {
        if (blackScreen == null) yield break;

        blackScreen.blocksRaycasts = true;
        blackScreen.interactable = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            blackScreen.alpha =
                Mathf.Clamp01(elapsed / fadeDuration);

            yield return null;
        }

        blackScreen.alpha = 1f;
    }

    private IEnumerator FadeFromBlack()
    {
        if (blackScreen == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            blackScreen.alpha =
                1f - Mathf.Clamp01(elapsed / fadeDuration);

            yield return null;
        }

        blackScreen.alpha = 0f;
        blackScreen.blocksRaycasts = false;
        blackScreen.interactable = false;
    }

    private void OnDestroy()
    {
        if (sharedButton != null)
        {
            sharedButton.onClick.RemoveListener(
                HandleSharedButtonClick);
        }

        if (activeTable == this)
        {
            DisableGhostMode();
            RestoreMovement();

            activeTable = null;
            isTransitioning = false;
            IsPlayerHiding = false;
        }

        if (candidateTarget == this)
        {
            candidateTarget = null;
        }
    }
}
