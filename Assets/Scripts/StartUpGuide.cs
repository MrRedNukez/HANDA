using System.Collections;
using UnityEngine;

public class StartUpGuide : MonoBehaviour
{
    [Header("Startup Guide")]
    [SerializeField] private bool showGuideOnStartup = true;

    [SerializeField] private PhoneGuideApp guideApp;

    [Tooltip("Delay before showing the guide.")]
    [SerializeField] private float startupDelay = 1f;

    [Header("Notification Sound")]
    [SerializeField] private bool playNotificationSound = true;

    [SerializeField] private AudioClip notificationSound;

    [Tooltip("Optional. If empty, a temporary AudioSource is used.")]
    [SerializeField] private AudioSource audioSource;

    private IEnumerator Start()
    {
        // Wait using real time, even if gameplay is paused.
        if (startupDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(startupDelay);
        }

        if (playNotificationSound && notificationSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(notificationSound);
            }
            else
            {
                // Play independently of the game's time scale.
                GameObject soundObject =
                    new GameObject("StartupNotificationAudio");

                AudioSource source =
                    soundObject.AddComponent<AudioSource>();

                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.ignoreListenerPause = true;

                source.PlayOneShot(notificationSound);

                Destroy(
                    soundObject,
                    notificationSound.length + 0.2f
                );
            }
        }

        if (showGuideOnStartup && guideApp != null)
        {
            guideApp.ShowGuide();
        }
    }
}