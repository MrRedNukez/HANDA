using UnityEngine;
using TMPro;
using System.Collections;

public class HitTextPopup : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject textObject; 
    public TextMeshProUGUI textComponent; 

    [Header("Randomized Lines")]
    public string[] hitPhrases = { "Ouch!", "Ow!", "Watch out!", "Ugh!" };
    
    [Tooltip("How long the word stays on screen before vanishing.")]
    public float displayDuration = 1.0f;

    private Coroutine popupRoutine;

    void Start()
    {
        // Ensure it is hidden when the game starts
        if (textObject != null)
        {
            textObject.SetActive(false);
        }
    }

    public void ShowHit()
    {
        if (textObject == null || textComponent == null || hitPhrases.Length == 0) return;

        // Pick a random phrase
        string randomPhrase = hitPhrases[Random.Range(0, hitPhrases.Length)];
        textComponent.text = randomPhrase;

        // Show the text and start the timer
        if (popupRoutine != null) StopCoroutine(popupRoutine);
        popupRoutine = StartCoroutine(PopupSequence());
    }

    private IEnumerator PopupSequence()
    {
        textObject.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        textObject.SetActive(false);
    }
}