
using System.Collections;
using UnityEngine;
using TMPro;

public class PhoneHUDController : MonoBehaviour
{
    [Header("Phone Model")]
    public Transform phoneModel;

public GameObject notificationBadge;

    [Header("States")]
    public PlayerMovement playerMovement;
    public SimulationTimeline timeline;

    [Header("Phone Positions")]
    public Transform bottomRightPosition;
    public Transform openPosition;

    [Header("Animation")]
    public float moveDuration = 0.35f;

    [Header("Phone UI")]
    public GameObject homeScreen;
    public GameObject floodApp;
    public GameObject phoneToggleButton;
    public GameObject deadScreenPanel;

    [Header("Phone Background Overlay")]
    public GameObject phoneBackgroundPanel;

    [Header("Notification System")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;

    [Header("Phone Screen Materials")]
    public Renderer phoneScreenRenderer;
    public Material screenOffMaterial;
    public Material screenOnMaterial;

    [Header("Battery Settings")]
    [Range(0, 100)]
    public int batteryPercentage = 12;

    [Range(1, 100)]
    public int batteryDrainPerUse = 2;

    public TextMeshProUGUI batteryText;

    [Header("Battery & Progression")]
    public float messageDisplayTime = 5f;
    public TextMeshProUGUI playerDialogueText;

    private bool isBatteryDrainActive = false;
    private bool hasSeenBatteryWarning = false;
    private bool shouldDrainBatteryOnClose = false;

    [Header("Guide Integration")]
    public PhoneGuideApp phoneGuideApp;

    [Header("Audio")]
    public AudioSource phoneAudio;
    public AudioClip standardChime;
    public AudioClip emergencyAlarm;
    public AudioClip deadBatteryClick;

    public bool isPhoneActive => phoneOpen;

    private bool phoneOpen = false;
    private bool isMoving = false;

    private Coroutine movementCoroutine;

    // Notification Queue System
    private bool hasPendingMessage = false;
    private string pendingMessage = "";

    private void Start()
    {
        UpdateBatteryDisplay();

        if (!ReferencesAssigned())
            return;

        phoneModel.gameObject.SetActive(false);

        if (phoneToggleButton != null)
            phoneToggleButton.SetActive(true);

        if (homeScreen != null)
            homeScreen.SetActive(false);

        if (floodApp != null)
            floodApp.SetActive(false);

        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (deadScreenPanel != null)
            deadScreenPanel.SetActive(false);

        if (phoneBackgroundPanel != null)
            phoneBackgroundPanel.SetActive(false);

        SetScreen(false);
    }

    public bool HasUnreadMessage => hasPendingMessage;

    public void TogglePhone()
    {
        if (isMoving)
            return;

        if (phoneOpen)
            ClosePhone();
        else
            OpenPhone(false);
    }

    // OPEN PHONE
    public void OpenPhone(bool bypassBatteryCheck = false)
    {
        if (phoneOpen || isMoving || !ReferencesAssigned())
            return;

        bool isDead = false;

        shouldDrainBatteryOnClose = false;

        if (!bypassBatteryCheck && isBatteryDrainActive)
        {
            if (batteryPercentage <= 0)
            {
                isDead = true;

                if (phoneAudio != null && deadBatteryClick != null)
                    phoneAudio.PlayOneShot(deadBatteryClick);
            }
            else
            {
                // Battery is consumed when the phone is put away.
                shouldDrainBatteryOnClose = true;
            }
        }

        phoneOpen = true;

        if (phoneToggleButton != null)
            phoneToggleButton.SetActive(false);

        if (phoneBackgroundPanel != null)
            phoneBackgroundPanel.SetActive(true);

        phoneModel.gameObject.SetActive(true);

        phoneModel.localPosition =
            bottomRightPosition.localPosition;

        phoneModel.localRotation =
            bottomRightPosition.localRotation;

        if (isDead)
        {
            if (homeScreen != null)
                homeScreen.SetActive(false);

            if (floodApp != null)
                floodApp.SetActive(false);

            if (deadScreenPanel != null)
                deadScreenPanel.SetActive(true);
        }
        else
        {
            if (homeScreen != null)
                homeScreen.SetActive(true);

            if (floodApp != null)
                floodApp.SetActive(false);

            if (deadScreenPanel != null)
                deadScreenPanel.SetActive(false);
        }

        UpdateBatteryDisplay();

        SetScreen(true);

        movementCoroutine =
            StartCoroutine(MovePhone(openPosition, false));

        if (hasPendingMessage && !isDead)
        {
            StartCoroutine(
                DisplayQueuedNotification(
                    pendingMessage,
                    messageDisplayTime
                )
            );

            hasPendingMessage = false;
        }
    }

    // CLOSE PHONE
    public void ClosePhone()
    {
        if (!phoneOpen || isMoving || !ReferencesAssigned())
            return;

        if (phoneGuideApp != null)
        {
            phoneGuideApp.CloseGuideForPhoneShutdown();
        }

        StopAllCoroutines();

        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (playerMovement != null &&
            timeline != null &&
            !timeline.isTransitioning)
        {
            playerMovement.canMove = true;
        }

        if (homeScreen != null)
            homeScreen.SetActive(false);

        if (floodApp != null)
            floodApp.SetActive(false);

        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (deadScreenPanel != null)
            deadScreenPanel.SetActive(false);

        // Consume battery after a normal phone-use session.
        if (shouldDrainBatteryOnClose)
        {
            batteryPercentage = Mathf.Max(
                0,
                batteryPercentage - batteryDrainPerUse
            );

            shouldDrainBatteryOnClose = false;

            UpdateBatteryDisplay();

            if (!hasSeenBatteryWarning &&
                playerDialogueText != null)
            {
                hasSeenBatteryWarning = true;

                StartCoroutine(
                    ShowPlayerDialogue(
                        "Geez, I forgot to charge the phone. I have to use it wisely."
                    )
                );
            }
        }

        movementCoroutine =
            StartCoroutine(MovePhone(bottomRightPosition, true));
    }

    // ENABLE BATTERY DRAIN
    public void EnableBatteryDrain()
    {
        isBatteryDrainActive = true;
        UpdateBatteryDisplay();
    }

    // UPDATE BATTERY TEXT
    private void UpdateBatteryDisplay()
    {
        if (batteryText != null)
        {
            batteryText.text = batteryPercentage + "%";
        }
    }

    // OPTIONAL: RECHARGE BATTERY
    public void RechargeBattery(int amount)
    {
        batteryPercentage = Mathf.Clamp(
            batteryPercentage + amount,
            0,
            100
        );

        UpdateBatteryDisplay();
    }

    // OPTIONAL: SET BATTERY DIRECTLY
    public void SetBatteryPercentage(int amount)
    {
        batteryPercentage = Mathf.Clamp(amount, 0, 100);
        UpdateBatteryDisplay();
    }

    public void ForceOpenPhone()
    {
        OpenPhone(true);
    }

    // RECEIVE NOTIFICATION
    public void ReceiveRelativeMessage(string message)
    {
        if (phoneAudio != null && standardChime != null)
        {
            phoneAudio.PlayOneShot(standardChime);
        }

        if (phoneOpen && !isMoving)
        {
            StartCoroutine(
                DisplayQueuedNotification(
                    message,
                    messageDisplayTime
                )
            );
        }
        else
        {
            pendingMessage = message;
            hasPendingMessage = true;
        }
    }

    public void TriggerEmergencyAlarm(string message)
    {
        if (phoneAudio != null && emergencyAlarm != null)
            phoneAudio.PlayOneShot(emergencyAlarm);

        pendingMessage = message;
        hasPendingMessage = true;

        ForceOpenPhone();
    }

    // DISPLAY NOTIFICATION
    private IEnumerator DisplayQueuedNotification(
        string message,
        float displayTime)
    {
        while (isMoving)
            yield return null;

        if (notificationPanel != null &&
            notificationText != null)
        {
            notificationText.text = message;

            notificationPanel.SetActive(true);

            yield return new WaitForSeconds(displayTime);

            notificationPanel.SetActive(false);
        }
    }

    // PLAYER DIALOGUE
    private IEnumerator ShowPlayerDialogue(string text)
    {
        playerDialogueText.text = text;

        playerDialogueText.gameObject.SetActive(true);

        yield return new WaitForSeconds(4f);

        playerDialogueText.gameObject.SetActive(false);
    }

    // PHONE ANIMATION
    private IEnumerator MovePhone(
        Transform target,
        bool hideAfterMovement)
    {
        isMoving = true;

        Vector3 startPosition = phoneModel.localPosition;
        Quaternion startRotation = phoneModel.localRotation;

        Transform parent = phoneModel.parent;

        Vector3 targetPosition =
            parent != null
            ? parent.InverseTransformPoint(target.position)
            : target.position;

        Quaternion targetRotation =
            parent != null
            ? Quaternion.Inverse(parent.rotation) * target.rotation
            : target.rotation;

        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            phoneModel.localPosition =
                Vector3.Lerp(startPosition, targetPosition, t);

            phoneModel.localRotation =
                Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        phoneModel.localPosition = targetPosition;
        phoneModel.localRotation = targetRotation;

        if (hideAfterMovement)
        {
            SetScreen(false);

            phoneModel.gameObject.SetActive(false);

            phoneOpen = false;

            if (phoneToggleButton != null)
                phoneToggleButton.SetActive(true);

            if (phoneBackgroundPanel != null)
                phoneBackgroundPanel.SetActive(false);
        }

        isMoving = false;
    }

    // CHECK REFERENCES
    private bool ReferencesAssigned()
    {
        return phoneModel != null &&
               bottomRightPosition != null &&
               openPosition != null;
    }

    // CHANGE PHONE SCREEN MATERIAL
    private void SetScreen(bool screenOn)
    {
        if (phoneScreenRenderer == null)
            return;

        Material targetMaterial =
            screenOn ? screenOnMaterial : screenOffMaterial;

        if (targetMaterial != null)
            phoneScreenRenderer.material = targetMaterial;
    }
}
