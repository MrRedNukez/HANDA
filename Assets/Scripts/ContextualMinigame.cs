using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro; 

public class ContextualMinigame : MonoBehaviour
{
    [Header("System References")]
    public SimulationTimeline timeline;
    public TextMeshProUGUI playerDialogueText; 

    [Header("Day 1 Settings")]
    [TextArea(2, 3)]
    public string day1Hint = "Better turn off the fusebox when it rains hard.";
    public float displayTime = 3f;

    [Header("Day 2 Action")]
    public UnityEvent onDay2Interact; 

    private Coroutine hintCoroutine;

    public void TryInteract()
    {
        if (timeline != null && timeline.isDay1)
        {
            if (hintCoroutine != null) StopCoroutine(hintCoroutine);
            hintCoroutine = StartCoroutine(ShowHintSequence());
        }
        else
        {
            // Allow access to the minigame
            onDay2Interact.Invoke();
        }
    }

    private IEnumerator ShowHintSequence()
    {
        if (playerDialogueText != null)
        {
            playerDialogueText.text = day1Hint;
            playerDialogueText.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(displayTime);
            
            playerDialogueText.gameObject.SetActive(false);
        }
    }
}