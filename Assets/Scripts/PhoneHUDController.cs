using System.Collections;
using UnityEngine;
using TMPro;

public class PhoneHUDController : MonoBehaviour
{
    [Header("Phone Model")]
    public Transform phoneModel;

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
    
    [Header("Notification System")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;

    [Header("Phone Screen Materials")]
    public Renderer phoneScreenRenderer;
    public Material screenOffMaterial;
    public Material screenOnMaterial;

    [Header("Battery & Progression")]
    public float messageDisplayTime = 5f;
    public int batteryCharges = 5;
    public TextMeshProUGUI playerDialogueText; 
    private bool isBatteryDrainActive = false;
    private bool hasSeenBatteryWarning = false;

    [Header("Audio")]
    public AudioSource phoneAudio;
    public AudioClip standardChime;
    public AudioClip emergencyAlarm;
    public AudioClip deadBatteryClick;

    public bool isPhoneActive => phoneOpen;
    private bool phoneOpen = false;
    private bool isMoving = false;
    private Coroutine movementCoroutine;
    
    // NEW: Notification Queue System
    private bool hasPendingMessage = false;
    private string pendingMessage = "";

    private void Start()
    {
        if (!ReferencesAssigned()) return;

        phoneModel.gameObject.SetActive(false);
        if (phoneToggleButton != null) phoneToggleButton.SetActive(true);
        if (homeScreen != null) homeScreen.SetActive(false);
        if (floodApp != null) floodApp.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (deadScreenPanel != null) deadScreenPanel.SetActive(false);

        SetScreen(false);
    }

    public bool HasUnreadMessage => hasPendingMessage;
    public void TogglePhone()
    {
        if (isMoving) return;

        if (phoneOpen) ClosePhone();
        else OpenPhone(false);
    }

    public void OpenPhone(bool bypassBatteryCheck = false)
    {
        if (phoneOpen || isMoving || !ReferencesAssigned()) return;

        bool isDead = false;

        if (!bypassBatteryCheck && isBatteryDrainActive)
        {
            if (batteryCharges <= 0)
            {
                isDead = true;
                if (phoneAudio && deadBatteryClick) phoneAudio.PlayOneShot(deadBatteryClick);
            }
            else
            {
                batteryCharges--;
                if (!hasSeenBatteryWarning && playerDialogueText != null)
                {
                    hasSeenBatteryWarning = true;
                    StartCoroutine(ShowPlayerDialogue("Geez, I forgot to charge the phone. I have to use it wisely."));
                }
            }
        }

        phoneOpen = true;
        if (phoneToggleButton != null) phoneToggleButton.SetActive(false);

        phoneModel.gameObject.SetActive(true);
        phoneModel.localPosition = bottomRightPosition.localPosition;
        phoneModel.localRotation = bottomRightPosition.localRotation;

        if (isDead)
        {
            if (homeScreen != null) homeScreen.SetActive(false);
            if (floodApp != null) floodApp.SetActive(false);
            if (deadScreenPanel != null) deadScreenPanel.SetActive(true);
        }
        else
        {
            if (homeScreen != null) homeScreen.SetActive(true);
            if (floodApp != null) floodApp.SetActive(false);
            if (deadScreenPanel != null) deadScreenPanel.SetActive(false);
        }

        SetScreen(true);
        movementCoroutine = StartCoroutine(MovePhone(openPosition, false));

        // NEW: If a message is waiting, display it only after opening
        if (hasPendingMessage && !isDead)
        {
            StartCoroutine(DisplayQueuedNotification(pendingMessage, messageDisplayTime));
            hasPendingMessage = false;
        }
    }

    public void ClosePhone()
    {
        StopAllCoroutines(); 
        if (notificationPanel != null) notificationPanel.SetActive(false);

        if (playerMovement != null && timeline != null && !timeline.isTransitioning)
        {
            playerMovement.canMove = true;
        }

        if (!phoneOpen || isMoving || !ReferencesAssigned()) return;

        if (homeScreen != null) homeScreen.SetActive(false);
        if (floodApp != null) floodApp.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (deadScreenPanel != null) deadScreenPanel.SetActive(false);

        movementCoroutine = StartCoroutine(MovePhone(bottomRightPosition, true));
    }

    public void EnableBatteryDrain() { isBatteryDrainActive = true; }

    public void ForceOpenPhone() { OpenPhone(true); }

    // NEW: Receive functions now just play audio and queue the text
    public void ReceiveRelativeMessage(string message)
    {
        // 1. Play the audio
        if (phoneAudio != null && standardChime != null) 
        {
            phoneAudio.PlayOneShot(standardChime);
        }

        // 2. Check if they are already holding the phone
        if (phoneOpen && !isMoving)
        {
            // Phone is already open, show the text immediately
            StartCoroutine(DisplayQueuedNotification(message, messageDisplayTime));
        }
        else
        {
            // Phone is closed, queue it up for when they press the button
            pendingMessage = message;
            hasPendingMessage = true;
        }
    }

    public void TriggerEmergencyAlarm(string message)
    {
        if (phoneAudio != null && emergencyAlarm != null) phoneAudio.PlayOneShot(emergencyAlarm);
        pendingMessage = message;
        hasPendingMessage = true;
        ForceOpenPhone();
    }

    private IEnumerator DisplayQueuedNotification(string message, float displayTime)
    {
        // Wait until the phone completely finishes its pop-up animation
        while (isMoving) yield return null;

        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);
            
            yield return new WaitForSeconds(displayTime);
            
            notificationPanel.SetActive(false);
        }
    }

    private IEnumerator ShowPlayerDialogue(string text)
    {
        playerDialogueText.text = text;
        playerDialogueText.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);
        playerDialogueText.gameObject.SetActive(false);
    }

    private IEnumerator MovePhone(Transform target, bool hideAfterMovement)
    {
        isMoving = true;
        Vector3 startPosition = phoneModel.localPosition;
        Quaternion startRotation = phoneModel.localRotation;
        Transform parent = phoneModel.parent;
        
        Vector3 targetPosition = parent != null ? parent.InverseTransformPoint(target.position) : target.position;
        Quaternion targetRotation = parent != null ? Quaternion.Inverse(parent.rotation) * target.rotation : target.rotation;

        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            phoneModel.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            phoneModel.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        phoneModel.localPosition = targetPosition;
        phoneModel.localRotation = targetRotation;

        if (hideAfterMovement)
        {
            SetScreen(false);
            phoneModel.gameObject.SetActive(false);
            phoneOpen = false;
            if (phoneToggleButton != null) phoneToggleButton.SetActive(true);
        }

        isMoving = false;
    }

    private bool ReferencesAssigned()
    {
        return phoneModel != null && bottomRightPosition != null && openPosition != null;
    }

    private void SetScreen(bool screenOn)
    {
        if (phoneScreenRenderer == null) return;
        Material targetMaterial = screenOn ? screenOnMaterial : screenOffMaterial;
        if (targetMaterial != null) phoneScreenRenderer.material = targetMaterial;
    }
}