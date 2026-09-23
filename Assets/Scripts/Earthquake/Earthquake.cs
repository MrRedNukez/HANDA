using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Earthquake : MonoBehaviour
{
    [Header("Earthquake Settings")]
    [Range(0f, 2f)]
    public float intensity = 0.2f;

    public float frequency = 15f;

    public bool earthquakeActive = false;

    private Rigidbody rb;
    private Vector3 originalPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalPosition = rb.position;

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        if (!earthquakeActive)
        {
            rb.MovePosition(originalPosition);
            return;
        }

        float time = Time.fixedTime;

        float shakeX =
            (Mathf.PerlinNoise(time * frequency, 0f) - 0.5f)
            * 2f * intensity;

        float shakeZ =
            (Mathf.PerlinNoise(0f, time * frequency) - 0.5f)
            * 2f * intensity;

        Vector3 newPosition = originalPosition +
                              new Vector3(shakeX, 0f, shakeZ);

        rb.MovePosition(newPosition);
    }

    public void StartEarthquake()
    {
        earthquakeActive = true;
    }

    public void StopEarthquake()
    {
        earthquakeActive = false;
    }
}