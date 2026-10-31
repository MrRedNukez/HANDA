using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class FuseBoxInteractable : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactButton; 
    public CanvasGroup fuseBoxPanel; 
    public Button closeButton; 
    
    [Header("Context Check")]
    public ContextualMinigame gatekeeper; 

    private bool isPlayerNear = false;

    void Start()
    {
        GetComponent<SphereCollider>().isTrigger = true; 

        if (interactButton != null) interactButton.SetActive(false); 
        ClosePanel(); 

        if (interactButton != null && gatekeeper != null)
        {
            // REPLACED: Routes the click to the gatekeeper instead of opening the panel directly
            interactButton.GetComponent<Button>().onClick.AddListener(gatekeeper.TryInteract);
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
            isPlayerNear = true; //[cite: 2]
            if (interactButton != null) interactButton.SetActive(true); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false; //[cite: 2]
            if (interactButton != null) interactButton.SetActive(false); 
            ClosePanel(); //[cite: 2]
        }
    }

    public void OpenPanel()
    {
        if (!isPlayerNear) return; //[cite: 2]

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