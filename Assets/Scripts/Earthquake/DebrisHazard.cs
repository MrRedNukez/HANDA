using UnityEngine;

public class DebrisHazard : MonoBehaviour
{
    private EarthquakeTimer timerSystem;
    
    [Tooltip("How long before the debris deletes itself to prevent lag.")]
    public float lifetime = 3f; 

    public void Setup(EarthquakeTimer timer)
    {
        timerSystem = timer;
        
        // Failsafe: Destroy this object after a few seconds so it doesn't pile up
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only checks for the player to test the damage and Game Over loop
        if (other.CompareTag("Player"))
        {
            if (timerSystem != null)
            {
                timerSystem.TakeDebrisDamage(); 
            }
            
            Destroy(gameObject); 
        }
    }
}