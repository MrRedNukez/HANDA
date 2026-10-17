using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [Tooltip("How fast the panel fades in and out.")]
    public float flickerSpeed = 3f;
    
    [Tooltip("The lowest opacity (0 = invisible, 1 = fully solid).")]
    public float minAlpha = 0.2f;
    
    [Tooltip("The highest opacity.")]
    public float maxAlpha = 1.0f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * flickerSpeed, 1f);

        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
    }
}