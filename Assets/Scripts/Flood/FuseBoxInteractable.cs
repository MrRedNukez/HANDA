using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class FuseBoxInteractable : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactButton; 
    public CanvasGroup fuseBoxPanel; 
    public Button closeButton; 

    private bool isPlayerNear = false;

    void Start()
    {
        GetComponent<SphereCollider>().isTrigger = true;

        if (interactButton != null) interactButton.SetActive(false);
        ClosePanel();

        // Hook up the buttons via code
        if (interactButton != null)
        {
            interactButton.GetComponent<Button>().onClick.AddListener(OpenPanel);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactButton != null) interactButton.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactButton != null) interactButton.SetActive(false);
            ClosePanel(); // Auto-close if they walk away
        }
    }

    public void OpenPanel()
    {
        if (!isPlayerNear) return;

        if (interactButton != null) interactButton.SetActive(false);

        if (fuseBoxPanel != null)
        {
            fuseBoxPanel.alpha = 1f;
            fuseBoxPanel.interactable = true;
            fuseBoxPanel.blocksRaycasts = true;
        }
    }

    public void ClosePanel()
    {
        if (fuseBoxPanel != null)
        {
            fuseBoxPanel.alpha = 0f;
            fuseBoxPanel.interactable = false;
            fuseBoxPanel.blocksRaycasts = false;
        }

        if (isPlayerNear && interactButton != null)
        {
            interactButton.SetActive(true);
        }
    }
}