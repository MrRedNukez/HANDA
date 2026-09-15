using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FloodBuoyancy : MonoBehaviour
{
    [Header("Flood Setup")]
    public float riseSpeed = 0.3f;
    public float maxHeight = 5.0f;
    public bool isFlooding = false;

    [Header("Buoyancy Settings")]
    public float fluidDensity = 12f;
    public float maxBuoyantForce = 25f;

    private BoxCollider triggerVolume;
    private float startY;

    private void Awake()
    {
        triggerVolume = GetComponent<BoxCollider>();
        triggerVolume.isTrigger = true;
        startY = transform.position.y;
    }

    private void Update()
    {
        if (!isFlooding) return;

        if (transform.position.y < maxHeight)
        {
            float deltaY = riseSpeed * Time.deltaTime;
            transform.Translate(Vector3.up * deltaY, Space.World);
        }
    }

    public float GetFloodProgress()
    {
        return Mathf.Clamp01((transform.position.y - startY) / (maxHeight - startY));
    }

    public float GetCurrentHeight()
    {
        return transform.position.y;
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
            float forceMagnitude = Mathf.Clamp(submergedDepth * fluidDensity, 0f, maxBuoyantForce);
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