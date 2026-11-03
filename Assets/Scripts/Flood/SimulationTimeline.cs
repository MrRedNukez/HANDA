using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimulationTimeline : MonoBehaviour
{
    public enum MessageDay { Day1, Day2 }

    [Serializable]
    public class ScheduledMessage
    {
        [TextArea(2, 3)]
        public string message;

        [Tooltip("Day 1: seconds since the game started. Day 2: seconds since Day 2 began (after the fade-in).")]
        public MessageDay day = MessageDay.Day1;

        [Tooltip("Seconds after the start of that day (the timer pauses while Time.timeScale is 0).")]
        public float sendAtSeconds = 5f;
    }

    [Header("Timeline Settings")]
    // NEW: turn this off to disable Day 1 Total Duration, Is Day 1 and Relative Message
    [Tooltip("OFF = skips the Day 1 sequence (storm message, forced phone, Day 2 transition). Scheduled Messages still run.")]
    public bool useTimelineSettings = true;
    public float day1TotalDuration = 60f;
    public bool isDay1 = true;
    [TextArea(2, 3)]
    public string relativeMessage = "Heavy storm incoming. Prepare the house!";

    [Header("Scheduled Messages")]
    public List<ScheduledMessage> scheduledMessages = new List<ScheduledMessage>();

    [Header("System References")]
    public PhoneHUDController phoneManager;
    public Light directionalLight;
    public FloodManager floodManager;
    public PlayerMovement playerMovement;

    [Header("Message Log")]
    public MessageLogApp messageLog;

    [Header("Phone Notification Badge")]
    public GameObject notificationBadge;

    [Header("Transition Settings")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 2f;

    [Header("State")]
    public bool isTransitioning = false;

    [Header("TV Broadcast Sequence")]
    public TVBroadcast tvBroadcast;
    public float tvBroadcastDuration = 8f;

    [Header("Day 2 Buffer & UI")]
    public float day2BufferTime = 5f;
    public TextMeshProUGUI transitionText;

    [Header("Player Teleportation")]
    public Transform playerTransform;
    public Transform day2SpawnPoint;

    private void Start()
    {
        // CHANGED: only force Is Day 1 when the timeline settings are on
        if (useTimelineSettings)
            isDay1 = true;

        if (fadePanel != null)
            fadePanel.alpha = 0f;

        if (transitionText != null)
            transitionText.gameObject.SetActive(false);

        // Hide notification badge at the beginning.
        if (notificationBadge != null)
            notificationBadge.SetActive(false);

        // CHANGED: the Day 1 sequence only runs when the toggle is on
        if (useTimelineSettings)
            StartCoroutine(Day1Sequence());

        // Scheduled messages always run
        StartCoroutine(ScheduledMessageRoutine(MessageDay.Day1));
    }

    // Clears the badge once the player has opened the phone
    private void Update()
    {
        if (notificationBadge != null &&
            notificationBadge.activeSelf &&
            phoneManager != null &&
            phoneManager.isPhoneActive)
        {
            notificationBadge.SetActive(false);
        }
    }

    private void DeliverMessage(string text)
    {
        if (phoneManager != null)
            phoneManager.ReceiveRelativeMessage(text);

        if (messageLog != null)
            messageLog.AddMessage(text, isDay1 ? "Day 1" : "Day 2");
    }

    private IEnumerator ScheduledMessageRoutine(MessageDay day)
    {
        // Only take this day's messages, earliest first
        List<ScheduledMessage> queue = new List<ScheduledMessage>();
        foreach (ScheduledMessage m in scheduledMessages)
        {
            if (m.day == day)
                queue.Add(m);
        }
        queue.Sort((a, b) => a.sendAtSeconds.CompareTo(b.sendAtSeconds));

        float timer = 0f;

        foreach (ScheduledMessage msg in queue)
        {
            // Wait until this message's time (Time.deltaTime pauses with the game)
            while (timer < msg.sendAtSeconds)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // CHANGED: Day 1 messages only stop when the timeline settings are on
            if (useTimelineSettings && day == MessageDay.Day1 && !isDay1)
                yield break;

            if (!string.IsNullOrEmpty(msg.message))
            {
                DeliverMessage(msg.message);

                if (notificationBadge != null &&
                    phoneManager != null &&
                    !phoneManager.isPhoneActive)
                {
                    notificationBadge.SetActive(true);
                }
            }
        }
    }

    private IEnumerator Day1Sequence()
    {
        float waitTime = day1TotalDuration - 15f;

        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }

        if (phoneManager != null)
        {
            // Receive message.
            DeliverMessage(relativeMessage);

            // Show red notification badge if phone is not already open.
            if (notificationBadge != null &&
                !phoneManager.isPhoneActive)
            {
                notificationBadge.SetActive(true);
            }
        }

        // Wait for player to read the message or until forced open.
        yield return new WaitForSeconds(5f);

        // Force open and freeze if ignored.
        if (phoneManager != null && phoneManager.HasUnreadMessage)
        {
            if (playerMovement != null)
                playerMovement.canMove = false;

            phoneManager.ForceOpenPhone();

            // Wait until the phone is opened.
            while (!phoneManager.isPhoneActive)
            {
                yield return null;
            }

            // Remove red notification badge when phone opens.
            if (notificationBadge != null)
                notificationBadge.SetActive(false);

            yield return new WaitForSeconds(5f);

            if (playerMovement != null)
                playerMovement.canMove = true;
        }
        else
        {
            // Player already opened and read the message.
            if (notificationBadge != null)
                notificationBadge.SetActive(false);

            yield return new WaitForSeconds(5f);
        }

        // Wait the remaining 5 seconds of Day 1.
        yield return new WaitForSeconds(5f);

        StartCoroutine(TransitionToDay2());
    }

    private IEnumerator TransitionToDay2()
    {
        isTransitioning = true;

        if (playerMovement != null)
            playerMovement.canMove = false;

        if (phoneManager != null)
            phoneManager.ClosePhone();

        // Hide notification badge during transition.
        if (notificationBadge != null)
            notificationBadge.SetActive(false);

        // 1. Fade out to black.
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = 1f;

        // 2. Day 2 Mid-Transition Setup (Pitch Black).
        isDay1 = false;

        if (transitionText != null)
        {
            transitionText.text = "The next day..";
            transitionText.gameObject.SetActive(true);
        }

        if (playerTransform != null && day2SpawnPoint != null)
        {
            CharacterController cc =
                playerTransform.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;

            playerTransform.position = day2SpawnPoint.position;
            playerTransform.rotation = day2SpawnPoint.rotation;

            if (cc != null)
                cc.enabled = true;
        }

        // 3. Wait in the dark.
        yield return new WaitForSeconds(day2BufferTime);

        if (transitionText != null)
            transitionText.gameObject.SetActive(false);

        // 4. Fade back in.
        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = 0f;

        // Day 2 has begun, start its message timer
        StartCoroutine(ScheduledMessageRoutine(MessageDay.Day2));

        // 5. Unfreeze player and play TV News.
        if (playerMovement != null)
            playerMovement.canMove = true;

        if (tvBroadcast != null)
            tvBroadcast.PlayNews();

        // 6. Wait for the broadcast to finish.
        yield return new WaitForSeconds(tvBroadcastDuration);

        // 7. Start the disaster.
        if (floodManager != null)
            floodManager.StartFloodEvent();

        if (phoneManager != null)
            phoneManager.EnableBatteryDrain();
    }
}