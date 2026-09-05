using UnityEngine;

public class ObjectMassController : MonoBehaviour
{
    public float mass = 50f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.mass = mass;
        }
    }

    public void SetMass(float newMass)
    {
        mass = newMass;

        if (rb != null)
        {
            rb.mass = mass;
        }
    }
}