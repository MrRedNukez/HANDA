using UnityEngine;

public class FuseBoxUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject fuseBoxPanel;

    [Header("Player")]
    public Transform player;

    [Header("Settings")]
    public float interactionDistance = 3f;

    private bool isOpen = false;

    private void Start()
    {
        if (fuseBoxPanel != null)
        {
            fuseBoxPanel.SetActive(false);
        }

        // Lock cursor for PC gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Automatically open when player gets close
        if (distance <= interactionDistance)
        {
            if (!isOpen)
            {
                OpenPanel();
            }
        }
        else
        {
            if (isOpen)
            {
                ClosePanel();
            }
        }
    }

    public void OpenPanel()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (fuseBoxPanel != null)
        {
            fuseBoxPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePanel()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (fuseBoxPanel != null)
        {
            fuseBoxPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}