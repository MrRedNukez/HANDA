using UnityEngine;
using UnityEngine.UI; // Required for the fade panel
using System.Collections;

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
    

    [Header("Transition UI")]
    public Image fadePanel; // Link a full-screen black UI Image here
    public float fadeSpeed = 1f;

    void Start()
    {
        // Ensure the screen starts clear
        if (fadePanel != null) fadePanel.color = new Color(0, 0, 0, 0);
        StartCoroutine(Day1Sequence());
    }

    private IEnumerator Day1Sequence()
    {
        // Safe math for your 30-second testing
        float waitTime = day1TotalDuration - 15f;
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }

        // 1. Send the message (Plays audio, queues text)
        if (phoneManager != null) 
        {
            phoneManager.ReceiveRelativeMessage(relativeMessage);
        }

        // 2. Give them exactly 5 seconds to open it themselves
        yield return new WaitForSeconds(5f);

        // 3. Force it open ONLY if they never read the message
        if (phoneManager != null && phoneManager.HasUnreadMessage)
        {
            // (If you have a player controller reference, disable movement here)
            phoneManager.ForceOpenPhone();
        }

        // 4. Wait the final 10 seconds of Day 1
        yield return new WaitForSeconds(10f);

        // 5. Fade to black and start Day 2
        StartCoroutine(TransitionToDay2());
    }

    private IEnumerator TransitionToDay2()
    {
        if (floodManager != null)
        {
            floodManager.StartFloodEvent();
        }

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 2. Execute the environment swap while the screen is pitch black
        isDay1 = false;
        
        if (phoneManager != null) phoneManager.EnableBatteryDrain();
        
        if (directionalLight != null)
        {
            directionalLight.color = Color.gray; 
            directionalLight.intensity = 0.5f;
        }
        
        Debug.Log("Day 2 started. Environment swapped in the dark.");

        // Wait 1 second in total darkness to build tension
        yield return new WaitForSeconds(1f); 

        // 3. Fade back to clear gameplay
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
}