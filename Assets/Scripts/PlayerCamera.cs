using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody; 

    [Header("Look Settings")]
    public float lookSensitivity = 15f; 
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Touch Look (Mobile)")]
    public bool useTouchLookArea = true;       // tick this for mobile / simulator testing
    public float touchSensitivity = 0.15f;
    public float verticalMultiplier = 1f;      // scale up/down look separately
    [Range(0f, 1f)]
    public float lookAreaStartX = 0f;          // CHANGED: 0 = whole screen can look
    public RectTransform[] excludedAreas;      // NEW: drag your Fixed Joystick(s) here

    [Header("State")]
    public bool canLook = true; // NEW: Toggle to pause camera smoothly

    private PlayerControls controls;
    private Vector2 lookInput;
    private Vector2 touchLookDelta;
    private int lookFingerId = -1;             // the finger currently controlling the camera
    private Vector2 lastLookPos;               // last screen position of the look finger
    private float xRotation = 0f;
    private bool isCursorLocked = false;

    void Awake()
    {
        controls = new PlayerControls();
        
        controls.Gameplay.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void OnEnable()
    {
        controls.Enable();
        EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
        if (useTouchLookArea) TouchSimulation.Enable();
#endif
    }

    void OnDisable()
    {
        controls.Disable();
#if UNITY_EDITOR
        TouchSimulation.Disable();
#endif
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
#if UNITY_EDITOR
        if (!useTouchLookArea) ToggleCursorLock(true);
#endif
    }

    private static bool IsValid(Vector2 v)
    {
        return !float.IsNaN(v.x) && !float.IsNaN(v.y) &&
               !float.IsInfinity(v.x) && !float.IsInfinity(v.y);
    }

    // NEW: true if the screen point is inside one of the excluded rectangles (the joystick)
    private bool IsInExcludedArea(Vector2 screenPos)
    {
        if (excludedAreas == null) return false;

        foreach (RectTransform rect in excludedAreas)
        {
            if (rect == null || !rect.gameObject.activeInHierarchy) continue;

            // Overlay canvases use a null camera; Screen Space - Camera canvases need theirs
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera : null;

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, cam))
                return true;
        }
        return false;
    }

    private void ReadTouchLook()
    {
        bool lookFingerFound = false;

        foreach (Touch t in Touch.activeTouches)
        {
            if (t.phase == TouchPhase.Began)
            {
                if (lookFingerId != -1) continue;
                if (!IsValid(t.screenPosition)) continue;

                bool inLookArea = t.screenPosition.x >= Screen.width * lookAreaStartX;
                bool inExcluded = IsInExcludedArea(t.screenPosition); // NEW
                bool overUI = EventSystem.current != null &&
                              EventSystem.current.IsPointerOverGameObject(t.touchId);

                if (inLookArea && !inExcluded && !overUI)
                {
                    lookFingerId = t.touchId;
                    lastLookPos = t.screenPosition;
                    lookFingerFound = true;
                }
            }
            else if (t.touchId == lookFingerId)
            {
                lookFingerFound = true;

                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    lookFingerId = -1;
                }
                else
                {
                    Vector2 pos = t.screenPosition;
                    if (!IsValid(pos)) continue;

                    touchLookDelta += pos - lastLookPos;
                    lastLookPos = pos;
                }
            }
        }

        // safety: if the finger vanished, release it
        if (lookFingerId != -1 && !lookFingerFound)
            lookFingerId = -1;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleCursorLock(!isCursorLocked);
        }
#endif

        if (!canLook)
        {
            touchLookDelta = Vector2.zero;
            lookFingerId = -1;
            return;
        }

        float mouseX;
        float mouseY;

        if (useTouchLookArea)
        {
            ReadTouchLook();
            mouseX = touchLookDelta.x * touchSensitivity;
            mouseY = touchLookDelta.y * touchSensitivity * verticalMultiplier;
            touchLookDelta = Vector2.zero;
        }
        else
        {
            mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
            mouseY = lookInput.y * lookSensitivity * Time.deltaTime;
        }

        if (float.IsNaN(mouseX) || float.IsInfinity(mouseX)) mouseX = 0f;
        if (float.IsNaN(mouseY) || float.IsInfinity(mouseY)) mouseY = 0f;
        if (float.IsNaN(xRotation) || float.IsInfinity(xRotation)) xRotation = 0f;

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