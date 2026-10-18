using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class DamagePulseUI : MonoBehaviour
{
    [System.Serializable]
    public class DamageStage
    {
        public string stageName = "Hit Stage";
        [Tooltip("If true, it loops forever. If false, it flashes once and fades out.")]
        public bool isContinuous = true; 
        [Tooltip("How fast the pulse beats.")]
        public float pulseSpeed = 2f;
        [Tooltip("How dark/red the screen gets (0 = invisible, 1 = solid).")]
        public float maxAlpha = 0.5f;
    }

    [Header("Damage Severity Stages")]
    [Tooltip("Index 0 is Healthy. Index 1 is 1st hit. Index 2 is 2nd hit, etc.")]
    public DamageStage[] stages;

    private CanvasGroup canvasGroup;
    private int currentStageIndex = 0;
    private Coroutine singlePulseRoutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void SetDamageState(int hitCount)
    {
        // Prevent going out of bounds if they take too many hits
        currentStageIndex = Mathf.Clamp(hitCount, 0, stages.Length - 1);
        
        DamageStage currentStage = stages[currentStageIndex];

        // If this stage is a one-time flash (like the 1st hit)
        if (!currentStage.isContinuous && hitCount > 0)
        {
            if (singlePulseRoutine != null) StopCoroutine(singlePulseRoutine);
            singlePulseRoutine = StartCoroutine(SinglePulse(currentStage.maxAlpha, currentStage.pulseSpeed));
        }
        else
        {
            // Reset the single pulse just in case
            if (singlePulseRoutine != null) StopCoroutine(singlePulseRoutine);
            singlePulseRoutine = null;
        }
    }

    void Update()
    {
        // If healthy, ensure the screen is clear
        if (currentStageIndex == 0) 
        {
            if (singlePulseRoutine == null) canvasGroup.alpha = 0f;
            return;
        }

        DamageStage currentStage = stages[currentStageIndex];

        // If this stage is set to continuous (like 2nd or 3rd hit), loop the pulse
        if (currentStage.isContinuous && singlePulseRoutine == null)
        {
            // PingPong smoothly bounces between 0 and your max opacity
            float t = Mathf.PingPong(Time.time * currentStage.pulseSpeed, 1f);
            canvasGroup.alpha = Mathf.Lerp(0f, currentStage.maxAlpha, t);
        }
    }

    private IEnumerator SinglePulse(float targetAlpha, float speed)
    {
        // Fade in quickly
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            canvasGroup.alpha = Mathf.Lerp(0f, targetAlpha, t);
            yield return null;
        }
        // Fade out quickly
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            canvasGroup.alpha = Mathf.Lerp(targetAlpha, 0f, t);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        singlePulseRoutine = null;
    }
}