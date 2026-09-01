using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FloodBuoyancy : MonoBehaviour
{
    [Header("Flood Setup")]
    public float riseSpeed = 0.3f;
    public float maxHeight = 3.0f;
    public bool isFlooding = false;

    [Header("Buoyancy Settings")]
    public float fluidDensity = 12f;
    public float maxBuoyantForce = 25f;

    private BoxCollider triggerVolume;
    private float currentWaterY;

    private void Awake()
    {
        triggerVolume = GetComponent<BoxCollider>();
        triggerVolume.isTrigger = true;
        currentWaterY = transform.position.y;
    }

    private void Update()
    {
        if (!isFlooding) return;

        // 1. Move water plane and expand trigger upward over time
        if (transform.position.y < maxHeight)
        {
            float deltaY = riseSpeed * Time.deltaTime;
            transform.Translate(Vector3.up * deltaY, Space.World);
            currentWaterY = transform.position.y;

            // Expand collider bounds downward so floating objects stay inside the trigger volume
            Vector3 size = triggerVolume.size;
            size.y += deltaY;
            triggerVolume.size = size;

            Vector3 center = triggerVolume.center;
            center.y -= deltaY * 0.5f;
            triggerVolume.center = center;
        }
    }

    private void OnTriggerStay(Collider other)
    {
    Rigidbody rb = other.attachedRigidbody;

    if (rb == null || rb.isKinematic) return;
  
    float waterSurfaceY = triggerVolume.bounds.max.y;

    float objectBottomY = other.bounds.min.y;

    float submergedDepth = waterSurfaceY - objectBottomY;

    if (submergedDepth > 0)
        {
        float forceMagnitude = Mathf.Clamp(submergedDepth * fluidDensity * rb.mass, 0f, maxBuoyantForce * rb.mass);
        rb.AddForce(Vector3.up * forceMagnitude, ForceMode.Force);

        Vector3 velocity = rb.linearVelocity;
        velocity.x *= 0.95f;
        velocity.z *= 0.95f;
        rb.linearVelocity = velocity;
        }
    }

    public void StartFlood()
    {
        isFlooding = true;
    }
}