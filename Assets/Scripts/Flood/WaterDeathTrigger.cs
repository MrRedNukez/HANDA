using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class WaterDeathTrigger : MonoBehaviour
{
    [Header("Manager Reference")]
    public FloodManager floodManager;

    [Header("Survival Settings")]
    public float timeToDrown = 3f;

    private Coroutine drownRoutine;

    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger when the water hits the camera (head)
        if (other.CompareTag("MainCamera"))
        {
            if (drownRoutine == null)
            {
                drownRoutine = StartCoroutine(DrownTimer());
                Debug.Log("Player submerged! Drowning timer started.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            if (drownRoutine != null)
            {
                StopCoroutine(drownRoutine);
                drownRoutine = null;
                Debug.Log("Player reached air! Drowning timer reset.");
            }
        }
    }

    private IEnumerator DrownTimer()
    {
        yield return new WaitForSeconds(timeToDrown);
        
        if (floodManager != null)
        {
            floodManager.TriggerDrowningDeath();
        }
    }
}