using System.Collections;
using UnityEngine;

public class PhoneHUDController : MonoBehaviour
{
    [Header("Phone")]
    public Transform phoneModel;

    [Header("Phone Positions")]
    public Transform bottomRightPosition;
    public Transform openPosition;

    [Header("Animation")]
    public float moveDuration = 0.35f;

    [Header("Phone UI")]
    public GameObject homeScreen;
    public GameObject floodApp;

    [Header("Phone Screen")]
    public Renderer phoneScreenRenderer;
    public Material screenOffMaterial;
    public Material screenOnMaterial;

    [Header("Settings")]
    public bool startClosed = true;

    private bool phoneOpen = false;
    private bool isMoving = false;

    private void Start()
    {
        Debug.Log("PhoneHUDController started.");

        if (startClosed)
        {
            phoneOpen = false;

            SetPhonePosition(bottomRightPosition);
            SetScreen(false);

            if (homeScreen != null)
                homeScreen.SetActive(false);

            if (floodApp != null)
                floodApp.SetActive(false);
        }
    }

    private void Update()
    {
        // Test with mouse in Unity Editor
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse click detected.");

            TryOpenPhone(Input.mousePosition);
        }

        // Mobile touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("Touch detected.");

                TryOpenPhone(touch.position);
            }
        }
    }

    private void TryOpenPhone(Vector2 screenPosition)
    {
        if (phoneOpen)
        {
            Debug.Log("Phone is already open.");
            return;
        }

        if (isMoving)
        {
            Debug.Log("Phone is currently moving.");
            return;
        }

        if (phoneModel == null)
        {
            Debug.LogError("Phone Model is NOT assigned!");
            return;
        }

        if (openPosition == null)
        {
            Debug.LogError("Open Position is NOT assigned!");
            return;
        }

        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }

        Ray ray = cam.ScreenPointToRay(screenPosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            Debug.Log("Raycast hit: " + hit.transform.name);

            Transform hitObject = hit.transform;

            // Check if we clicked the phone or any child of the phone
            if (hitObject == phoneModel ||
                hitObject.IsChildOf(phoneModel))
            {
                Debug.Log("PHONE CLICKED!");

                OpenPhone();
            }
            else
            {
                Debug.Log("Something else was clicked: " + hitObject.name);
            }
        }
        else
        {
            Debug.Log("Raycast did NOT hit anything.");
        }
    }

    public void OpenPhone()
    {
        if (phoneOpen || isMoving)
            return;

        Debug.Log("Opening phone...");

        phoneOpen = true;

        SetScreen(true);

        if (homeScreen != null)
            homeScreen.SetActive(true);

        if (floodApp != null)
            floodApp.SetActive(false);

        StartCoroutine(MovePhone(openPosition));
    }

    public void ClosePhone()
{
    if (phoneModel == null)
    {
        Debug.LogError("PhoneModel is not assigned!");
        return;
    }

    if (bottomRightPosition == null)
    {
        Debug.LogError("BottomRightPosition is not assigned!");
        return;
    }

    // Move phone directly back to the bottom-right position
    phoneModel.position = bottomRightPosition.position;
    phoneModel.rotation = bottomRightPosition.rotation;

    // Turn phone screen off
    SetScreen(false);

    // Hide phone UI
    if (homeScreen != null)
        homeScreen.SetActive(false);

    if (floodApp != null)
        floodApp.SetActive(false);

    // Mark phone as closed
    phoneOpen = false;

    Debug.Log("Phone returned to BottomRightPosition.");
}

    private IEnumerator ClosePhoneAnimation()
    {
        yield return StartCoroutine(MovePhone(bottomRightPosition));

        SetScreen(false);
    }

    private IEnumerator MovePhone(Transform target)
    {
        if (target == null)
        {
            Debug.LogError("MovePhone target is NULL!");
            yield break;
        }

        if (phoneModel == null)
        {
            Debug.LogError("Phone Model is NULL!");
            yield break;
        }

        isMoving = true;

        Vector3 startPosition = phoneModel.position;
        Quaternion startRotation = phoneModel.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / moveDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            phoneModel.position =
                Vector3.Lerp(
                    startPosition,
                    target.position,
                    t
                );

            phoneModel.rotation =
                Quaternion.Slerp(
                    startRotation,
                    target.rotation,
                    t
                );

            yield return null;
        }

        phoneModel.position = target.position;
        phoneModel.rotation = target.rotation;

        isMoving = false;

        Debug.Log("Phone movement finished.");
    }

    private void SetPhonePosition(Transform target)
    {
        if (phoneModel == null || target == null)
        {
            Debug.LogError(
                "Cannot set phone position. " +
                "Phone Model or target is missing."
            );

            return;
        }

        phoneModel.position = target.position;
        phoneModel.rotation = target.rotation;
    }

    private void SetScreen(bool on)
    {
        if (phoneScreenRenderer == null)
        {
            Debug.LogWarning("Phone Screen Renderer is not assigned.");
            return;
        }

        if (on && screenOnMaterial != null)
        {
            phoneScreenRenderer.material = screenOnMaterial;
        }
        else if (!on && screenOffMaterial != null)
        {
            phoneScreenRenderer.material = screenOffMaterial;
        }
    }
}