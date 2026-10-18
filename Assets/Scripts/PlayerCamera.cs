using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody; 

    [Header("Look Settings")]
    public float lookSensitivity = 15f; 
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("State")]
    public bool canLook = true; // NEW: Toggle to pause camera smoothly

    private PlayerControls controls;
    private Vector2 lookInput;
    private float xRotation = 0f;
    private bool isCursorLocked = false;

    void Awake()
    {
        controls = new PlayerControls();
        
        controls.Gameplay.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
#if UNITY_EDITOR
        ToggleCursorLock(true);
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleCursorLock(!isCursorLocked);
        }
#endif

        // NEW: Instantly stop calculating camera math if the minigame is open
        if (!canLook) return; 

        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }

#if UNITY_EDITOR
    private void ToggleCursorLock(bool lockIt)
    {
        isCursorLocked = lockIt;
        if (isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
#endif
}