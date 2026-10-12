
using System.Collections;
using UnityEngine;

public class PhoneHUDController : MonoBehaviour
{
    [Header("Phone Model")]
    public Transform phoneModel;

    [Header("Phone Positions")]
    public Transform bottomRightPosition;
    public Transform openPosition;

    [Header("Animation")]
    public float moveDuration = 0.35f;

    [Header("Phone UI")]
    public GameObject homeScreen;
    public GameObject floodApp;
    public GameObject phoneToggleButton;

    [Header("Phone Screen Materials")]
    public Renderer phoneScreenRenderer;
    public Material screenOffMaterial;
    public Material screenOnMaterial;

    private bool phoneOpen = false;
    private bool isMoving = false;

    private Coroutine movementCoroutine;

    private void Start()
    {
        if (!ReferencesAssigned())
            return;

        // Start with the phone hidden.
        phoneModel.gameObject.SetActive(false);

        // Show the external phone toggle button.
        if (phoneToggleButton != null)
            phoneToggleButton.SetActive(true);

        // Hide the phone's internal screens.
        if (homeScreen != null)
            homeScreen.SetActive(false);

        if (floodApp != null)
            floodApp.SetActive(false);

        SetScreen(false);
    }

    // Connect the external UI button to this method.
    public void TogglePhone()
    {
        if (isMoving)
            return;

        if (phoneOpen)
            ClosePhone();
        else
            OpenPhone();
    }

    // =====================================================
    // OPEN PHONE
    // =====================================================

    public void OpenPhone()
    {
        if (phoneOpen || isMoving)
            return;

        if (!ReferencesAssigned())
            return;

        phoneOpen = true;

        // Hide the external toggle button.
        if (phoneToggleButton != null)
            phoneToggleButton.SetActive(false);

        // Show the phone at the bottom-right position.
        phoneModel.gameObject.SetActive(true);

        phoneModel.localPosition =
            bottomRightPosition.localPosition;

        phoneModel.localRotation =
            bottomRightPosition.localRotation;

        // Show the home screen.
        if (homeScreen != null)
            homeScreen.SetActive(true);

        // Keep the Flood Safety app closed initially.
        if (floodApp != null)
            floodApp.SetActive(false);

        // Turn the phone screen on.
        SetScreen(true);

        // Animate the phone to the open position.
        movementCoroutine = StartCoroutine(
            MovePhone(openPosition, false)
        );
    }

    // =====================================================
    // CLOSE PHONE
    // =====================================================

    public void ClosePhone()
    {
        if (!phoneOpen || isMoving)
            return;

        if (!ReferencesAssigned())
            return;

        // Hide the internal phone screens.
        if (homeScreen != null)
            homeScreen.SetActive(false);

        if (floodApp != null)
            floodApp.SetActive(false);

        // Move the phone back, then hide it.
        movementCoroutine = StartCoroutine(
            MovePhone(bottomRightPosition, true)
        );
    }

    // =====================================================
    // PHONE MOVEMENT
    // =====================================================

    private IEnumerator MovePhone(
        Transform target,
        bool hideAfterMovement)
    {
        if (target == null || phoneModel == null)
        {
            Debug.LogError(
                "PhoneHUDController: Movement target or phone is missing."
            );

            isMoving = false;
            yield break;
        }

        isMoving = true;

        Vector3 startPosition = phoneModel.localPosition;
        Quaternion startRotation = phoneModel.localRotation;

        // Convert the target marker's world transform into
        // the phone model's parent's local coordinate space.
        Transform parent = phoneModel.parent;

        Vector3 targetPosition = parent != null
            ? parent.InverseTransformPoint(target.position)
            : target.position;

        Quaternion targetRotation = parent != null
            ? Quaternion.Inverse(parent.rotation) * target.rotation
            : target.rotation;

        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            phoneModel.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            phoneModel.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        // Ensure the phone reaches the exact target.
        phoneModel.localPosition = targetPosition;
        phoneModel.localRotation = targetRotation;

        if (hideAfterMovement)
        {
            // Turn the screen off.
            SetScreen(false);

            // Hide the entire phone.
            phoneModel.gameObject.SetActive(false);

            phoneOpen = false;

            // Show the external toggle button again.
            if (phoneToggleButton != null)
                phoneToggleButton.SetActive(true);
        }

        isMoving = false;
        movementCoroutine = null;
    }

    // =====================================================
    // VALIDATE REFERENCES
    // =====================================================

    private bool ReferencesAssigned()
    {
        if (phoneModel == null)
        {
            Debug.LogError(
                "PhoneHUDController: Phone Model is not assigned."
            );
            return false;
        }

        if (bottomRightPosition == null)
        {
            Debug.LogError(
                "PhoneHUDController: Bottom Right Position is not assigned."
            );
            return false;
        }

        if (openPosition == null)
        {
            Debug.LogError(
                "PhoneHUDController: Open Position is not assigned."
            );
            return false;
        }

        return true;
    }

    // =====================================================
    // PHONE SCREEN MATERIAL
    // =====================================================

    private void SetScreen(bool screenOn)
    {
        if (phoneScreenRenderer == null)
            return;

        Material targetMaterial = screenOn
            ? screenOnMaterial
            : screenOffMaterial;

        if (targetMaterial != null)
        {
            phoneScreenRenderer.material = targetMaterial;
        }
    }
}
