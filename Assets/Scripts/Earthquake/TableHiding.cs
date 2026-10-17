
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TableHiding : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Camera playerCamera;

    [Tooltip("Add PlayerMovement here. CharacterController is optional.")]
    public Behaviour[] movementScriptsToDisable;

    [Header("Table Positions")]
    public Transform hidePoint;
    public Transform exitPoint;

    [Tooltip("Extra distance to move downward from the Hide Point.")]
    [Range(0f, 1f)]
    public float lowerHidingAmount = 0.2f;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public float lookDistance = 5f;

    [Header("Shared UI - Reuse for every table")]
    public Button interactionButton;
    public Text buttonLabel;

    [Header("Black Screen")]
    public CanvasGroup blackScreen;
    public float fadeDuration = 0.25f;

    [Header("Ghost Mode")]
    public bool disablePlayerColliders = true;
    public bool freezePlayerRigidbody = true;

    private static TableHiding candidateTarget;
    private static TableHiding activeTable;
    private static Button sharedButton;
    private static Text sharedLabel;

    private static int candidateFrame = -1;
    private static int uiUpdatedFrame = -1;
    private static float candidateDistance;
    private static bool candidateIsLookedAt;
    private static bool isTransitioning;

    private Collider tableCollider;

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
        tableCollider = GetComponentInChildren<Collider>();

        if (movementScriptsToDisable != null)
        {
            savedMovementStates =
                new bool[movementScriptsToDisable.Length];
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
    }

    private void Update()
    {
        if (candidateFrame != Time.frameCount)
        {
            candidateFrame = Time.frameCount;
            candidateTarget = null;
            candidateDistance = float.MaxValue;
            candidateIsLookedAt = false;
        }

        if (activeTable != null || isTransitioning)
            return;

        if (player == null ||
            playerCamera == null ||
            tableCollider == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            tableCollider.ClosestPoint(player.position));

        bool nearTable = distance <= interactionDistance;
        bool lookingAtTable = IsPlayerLookingAtTable();

        if (!nearTable && !lookingAtTable)
            return;

        bool shouldReplace =
            candidateTarget == null ||
            (lookingAtTable && !candidateIsLookedAt) ||
            (lookingAtTable == candidateIsLookedAt &&
             distance < candidateDistance);

        if (shouldReplace)
        {
            candidateTarget = this;
            candidateDistance = distance;
            candidateIsLookedAt = lookingAtTable;
        }
    }

    private bool IsPlayerLookingAtTable()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward);

        if (!Physics.Raycast(
            ray, out RaycastHit hit, lookDistance))
            return false;

        Transform hitTransform = hit.transform;

        return hitTransform == transform ||
               hitTransform.IsChildOf(transform);
    }

    private void LateUpdate()
    {
        if (uiUpdatedFrame == Time.frameCount)
            return;

        uiUpdatedFrame = Time.frameCount;

        if (sharedButton == null)
            return;

        bool hiding = activeTable != null;

        TableHiding target = hiding
            ? activeTable
            : candidateTarget;

        bool showButton = target != null;

        if (sharedButton.gameObject.activeSelf != showButton)
            sharedButton.gameObject.SetActive(showButton);

        if (sharedLabel != null && showButton)
        {
            sharedLabel.text = hiding
                ? "EXIT"
                : "HIDE UNDER TABLE";
        }

        sharedButton.interactable =
            showButton && !isTransitioning;
    }

    private static void HandleSharedButtonClick()
    {
        if (isTransitioning)
            return;

        if (activeTable != null)
        {
            activeTable.ExitTable();
            return;
        }

        if (candidateTarget != null)
            candidateTarget.EnterTable();
    }

    private void EnterTable()
    {
        if (isFading ||
            activeTable != null ||
            player == null ||
            hidePoint == null)
            return;

        activeTable = this;
        isHidden = true;

        StartCoroutine(EnterRoutine());
    }

    private IEnumerator EnterRoutine()
    {
        isFading = true;
        isTransitioning = true;

        yield return StartCoroutine(FadeToBlack());

        // IMPORTANT: Disable PlayerMovement FIRST.
        // Its Update() can no longer call CharacterController.Move().
        SaveAndDisableMovement();

        // Only after movement is disabled, disable colliders.
        EnableGhostMode();

        // Move the player without changing rotation.
        player.position =
            hidePoint.position -
            Vector3.up * lowerHidingAmount;

        yield return StartCoroutine(FadeFromBlack());

        isTransitioning = false;
        isFading = false;
    }

    private void ExitTable()
    {
        if (isFading ||
            !isHidden ||
            exitPoint == null)
            return;

        StartCoroutine(ExitRoutine());
    }

    private IEnumerator ExitRoutine()
    {
        isFading = true;
        isTransitioning = true;

        yield return StartCoroutine(FadeToBlack());

        // Position only; preserve player and camera rotation.
        player.position = exitPoint.position;

        // Restore the controller before movement resumes.
        DisableGhostMode();
        RestoreMovement();

        isHidden = false;
        activeTable = null;

        yield return StartCoroutine(FadeFromBlack());

        isTransitioning = false;
        isFading = false;
    }

    private void EnableGhostMode()
    {
        if (disablePlayerColliders &&
            playerColliders != null)
        {
            for (int i = 0; i < playerColliders.Length; i++)
            {
                Collider col = playerColliders[i];

                if (col == null)
                    continue;

                savedColliderStates[i] = col.enabled;
                col.enabled = false;
            }
        }

        if (freezePlayerRigidbody &&
            playerRigidbody != null)
        {
            savedIsKinematic = playerRigidbody.isKinematic;
            savedUseGravity = playerRigidbody.useGravity;
            savedVelocity = playerRigidbody.linearVelocity;
            savedAngularVelocity =
                playerRigidbody.angularVelocity;

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
                playerRigidbody.angularVelocity =
                    savedAngularVelocity;
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
                    col.enabled = savedColliderStates[i];
            }
        }
    }

    private void SaveAndDisableMovement()
    {
        if (movementScriptsToDisable == null)
            return;

        for (int i = 0;
             i < movementScriptsToDisable.Length;
             i++)
        {
            Behaviour script = movementScriptsToDisable[i];

            if (script == null)
                continue;

            savedMovementStates[i] = script.enabled;
            script.enabled = false;
        }
    }

    private void RestoreMovement()
    {
        if (movementScriptsToDisable == null ||
            savedMovementStates == null)
            return;

        for (int i = 0;
             i < movementScriptsToDisable.Length;
             i++)
        {
            Behaviour script = movementScriptsToDisable[i];

            if (script != null)
                script.enabled = savedMovementStates[i];
        }
    }

    private IEnumerator FadeToBlack()
    {
        if (blackScreen == null)
            yield break;

        blackScreen.blocksRaycasts = true;
        blackScreen.interactable = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            blackScreen.alpha = Mathf.Clamp01(
                elapsed / fadeDuration);

            yield return null;
        }

        blackScreen.alpha = 1f;
    }

    private IEnumerator FadeFromBlack()
    {
        if (blackScreen == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            blackScreen.alpha = 1f - Mathf.Clamp01(
                elapsed / fadeDuration);

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
            // Restore everything if this table is removed mid-hide.
            DisableGhostMode();
            RestoreMovement();

            activeTable = null;
            isTransitioning = false;
        }

        if (candidateTarget == this)
            candidateTarget = null;
    }
}
