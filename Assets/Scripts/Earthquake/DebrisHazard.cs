using UnityEngine;

public class DebrisHazard : MonoBehaviour
{
    private EarthquakeTimer timerSystem;
    
    [Tooltip("How long before the debris deletes itself to prevent lag.")]
    public float lifetime = 3f; 

    public void Setup(EarthquakeTimer timer)
    {
        timerSystem = timer;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (timerSystem != null)
            {
                timerSystem.TakeDebrisDamage(); 
            }
            Destroy(gameObject); 
        }

        else if (other.CompareTag("Table"))
        {
            Debug.Log("Debris blocked by the table!");
    
            Destroy(gameObject); 
        }

        else if (other.CompareTag("Floor"))
        {
            Destroy(gameObject);
        }
    }
}