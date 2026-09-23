using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;
    
    [Header("Indicator Setup")]
    public GameObject indicatorVisuals; 

    [Header("Behavior Settings")]
    public bool autoStart = true;    
    public bool isPermanent = false; 
    
    [Header("Timer Settings")]
    public float delayBeforeShowing = 2f; 
    public float displayDuration = 5f;    

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        if (indicatorVisuals != null)
        {
            indicatorVisuals.SetActive(false);
        }

        if (autoStart)
        {
            StartIndicatorSequence();
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null || indicatorVisuals == null || !indicatorVisuals.activeSelf) 
            return;

        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }

    public void StartIndicatorSequence()
    {
        if (indicatorVisuals != null)
        {
            StopAllCoroutines(); 
            StartCoroutine(DisplayRoutine(displayDuration));
        }
    }

    public void ShowIndicator(float customDuration)
    {
        if (indicatorVisuals != null)
        {
            isPermanent = false; 
            StopAllCoroutines();
            StartCoroutine(DisplayRoutine(customDuration));
        }
    }

    private IEnumerator DisplayRoutine(float timeToStayOn)
    {
        yield return new WaitForSeconds(delayBeforeShowing);
        
        indicatorVisuals.SetActive(true);
       
        if (isPermanent) yield break; 

        yield return new WaitForSeconds(timeToStayOn);
        indicatorVisuals.SetActive(false);
    }
}