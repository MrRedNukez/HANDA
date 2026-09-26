using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class HouseZone : MonoBehaviour
{
    [Header("Hub Reference")]
    public SimulationManager simManager;

    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detects the parent object holding the "Player" tag
        if (other.CompareTag("Player"))
        {
            if (simManager != null) simManager.SetPlayerInside(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (simManager != null) simManager.SetPlayerInside(false);
        }
    }
}