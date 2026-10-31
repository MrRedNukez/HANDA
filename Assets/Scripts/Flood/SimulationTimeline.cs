using System.Collections;
using UnityEngine;
using TMPro;

public class SimulationTimeline : MonoBehaviour
{
    [Header("Timeline Settings")]
    public float day1TotalDuration = 60f;
    public bool isDay1 = true;
    [TextArea(2, 3)]
    public string relativeMessage = "Heavy storm incoming. Prepare the house!";

    [Header("System References")]
    public PhoneHUDController phoneManager;
    public Light directionalLight;
    public FloodManager floodManager; 
    public PlayerMovement playerMovement; // NEW: Reference to freeze movement

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
        isDay1 = true;
        if (fadePanel != null) fadePanel.alpha = 0f;
        if (transitionText != null) transitionText.gameObject.SetActive(false);

        StartCoroutine(Day1Sequence());
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
            phoneManager.ReceiveRelativeMessage(relativeMessage);
        }

        yield return new WaitForSeconds(5f);

        // Force open and freeze if ignored
        if (phoneManager != null && phoneManager.HasUnreadMessage)
        {
            if (playerMovement != null) playerMovement.canMove = false;
            phoneManager.ForceOpenPhone();
            
            yield return new WaitForSeconds(5f); // Wait while they read it
            
            if (playerMovement != null) playerMovement.canMove = true;
        }
        else
        {
            yield return new WaitForSeconds(5f); // Balance the timer if they opened it manually
        }

        // Wait the remaining 5 seconds of Day 1
        yield return new WaitForSeconds(5f); 

        StartCoroutine(TransitionToDay2());
    }

    private IEnumerator TransitionToDay2()
    {
        isTransitioning = true; 
        if (playerMovement != null) playerMovement.canMove = false;

        if (phoneManager != null) phoneManager.ClosePhone();
        
        // 1. Fade out to black
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 1f;

        // 2. Day 2 Mid-Transition Setup (Pitch Black)
        isDay1 = false;
        if (transitionText != null)
        {
            transitionText.text = "The next day..";
            transitionText.gameObject.SetActive(true);
        }

        if (playerTransform != null && day2SpawnPoint != null)
        {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; 

            playerTransform.position = day2SpawnPoint.position;
            playerTransform.rotation = day2SpawnPoint.rotation;

            if (cc != null) cc.enabled = true;
        }

        // 3. Wait in the dark
        yield return new WaitForSeconds(day2BufferTime);

        if (transitionText != null) transitionText.gameObject.SetActive(false);

        // 4. Fade back in
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 0f;
        
        // 5. Unfreeze player and play TV News
        if (playerMovement != null) playerMovement.canMove = true;
        if (tvBroadcast != null) tvBroadcast.PlayNews();

        // 6. Wait for the broadcast to finish
        yield return new WaitForSeconds(tvBroadcastDuration);

        // 7. Start the disaster
        if (floodManager != null) floodManager.StartFloodEvent();
        if (phoneManager != null) phoneManager.EnableBatteryDrain();
    }
}