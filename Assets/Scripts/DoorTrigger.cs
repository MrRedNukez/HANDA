using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The object that rotates (must have its pivot at the hinge)")]
    public Transform doorPivot;

    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 3f;
    [Tooltip("Tick this if the door swings toward the player instead of away")]
    public bool invertDirection = false;

    private Quaternion closedRotation;
    private Quaternion targetRotation;

    void Start()
    {
        if (doorPivot == null) doorPivot = transform;

        closedRotation = doorPivot.localRotation;
        targetRotation = closedRotation;
    }

    void Update()
    {
        doorPivot.localRotation = Quaternion.Slerp(
            doorPivot.localRotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Which side of the door is the player on?
        Vector3 toPlayer = other.transform.position - doorPivot.position;
        float side = Vector3.Dot(doorPivot.forward, toPlayer);

        // Swing away from the player
        float direction = side > 0 ? 1f : -1f;
        if (invertDirection) direction *= -1f;

        targetRotation = closedRotation * Quaternion.Euler(0f, openAngle * direction, 0f);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        targetRotation = closedRotation;
    }
}