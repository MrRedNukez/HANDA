using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class LadderPickup : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventory; 
    public GameObject interactButton; 
    public GameObject foldedLadderModel; 

    [Header("Context Check")]
    public ContextualMinigame gatekeeper; 

    private bool isPlayerNear = false;

    void Start()
    {
        GetComponent<SphereCollider>().isTrigger = true;
        
        if (interactButton != null)
        {
            interactButton.SetActive(false);
            // REMOVED: Do not add the listener here anymore.
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !inventory.hasLadder)
        {
            isPlayerNear = true;
            if (interactButton != null)
            {
                interactButton.SetActive(true);
                
                // NEW: Clear previous listeners and exclusively attach this specific ladder
                Button btn = interactButton.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                
                if (gatekeeper != null)
                {
                    btn.onClick.AddListener(gatekeeper.TryInteract);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactButton != null)
            {
                interactButton.SetActive(false);
                
                // NEW: Detach the listener when walking away
                interactButton.GetComponent<Button>().onClick.RemoveAllListeners();
            }
        }
    }

    public void TakeLadder()
    {
        if (!isPlayerNear) return;

        inventory.hasLadder = true;
        Debug.Log("Ladder added to inventory!");

        if (interactButton != null)
        {
            interactButton.SetActive(false);
            interactButton.GetComponent<Button>().onClick.RemoveAllListeners(); // Clean up on pickup
        }

        if (foldedLadderModel != null) foldedLadderModel.SetActive(false);
        GetComponent<SphereCollider>().enabled = false; 
    }
}