using UnityEngine;

public class TVBroadcast : MonoBehaviour
{
    [Header("TV Components")]
    public GameObject blackScreenPanel; // Your world-space UI panel
    public AudioSource newsAudio;       // The dialogue audio

    private bool hasPower = true;

    private void Start()
    {
        // Ensure the TV starts "off" on Day 1
        if (blackScreenPanel != null) blackScreenPanel.SetActive(true);
    }

    public void PlayNews()
    {
        // Only play if the power is still on
        if (!hasPower) return;

        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);
        if (newsAudio != null) newsAudio.Play();
    }

    public void ShutOff()
    {
        hasPower = false; // Prevents the TV from turning back on

        if (blackScreenPanel != null) blackScreenPanel.SetActive(true);
        if (newsAudio != null) newsAudio.Stop();
    }
}