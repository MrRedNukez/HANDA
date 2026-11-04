using UnityEngine;
using TMPro;
using System.Collections;

public class TVBroadcast : MonoBehaviour
{
    [System.Serializable]
    public struct SubtitleLine
    {
        [TextArea(2, 3)] 
        public string text;
        public float duration; 
    }

    [Header("TV Components")]
    public GameObject blackScreenPanel; 
    public AudioSource newsAudio;       

    [Header("Subtitles")]
    public TextMeshProUGUI subtitleUI;
    public SubtitleLine[] subtitles;

    private bool hasPower = true;
    private Coroutine subtitleCoroutine;

    private void Start()
    {
        if (blackScreenPanel != null) blackScreenPanel.SetActive(true);
        if (subtitleUI != null) subtitleUI.gameObject.SetActive(false);
    }

    public void PlayNews()
    {
        if (!hasPower) return;

        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);
        if (newsAudio != null) newsAudio.Play();

        // Start the subtitle sequence
        if (subtitleUI != null && subtitles.Length > 0)
        {
            subtitleCoroutine = StartCoroutine(ShowSubtitles());
        }
    }

    public void ShutOff()
    {
        hasPower = false; 

        if (blackScreenPanel != null) blackScreenPanel.SetActive(true);
        if (newsAudio != null) newsAudio.Stop();

        if (subtitleCoroutine != null) StopCoroutine(subtitleCoroutine);
        if (subtitleUI != null) subtitleUI.gameObject.SetActive(false);
    }

    private IEnumerator ShowSubtitles()
    {
        subtitleUI.gameObject.SetActive(true);

        foreach (SubtitleLine line in subtitles)
        {
            subtitleUI.text = line.text;
            yield return new WaitForSeconds(line.duration);
        }

        // Hide text when all lines are done
        subtitleUI.gameObject.SetActive(false);
    }
}