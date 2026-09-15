using UnityEngine;

public class FirstPersonLookDEL : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;   // Drag your Player object here (the one with PlayerMovement)

    [Header("Mouse Settings")]
    public float mouseSensitivity = 200f;
    public bool lockCursor = true;

    [Header("Pitch Clamp")]
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private float xRotation = 0f;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Look up/down (camera only)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Look left/right (rotate the whole player body)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}