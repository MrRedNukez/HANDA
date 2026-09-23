using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class LadderPlacement : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventory;
    public GameObject interactButton; 
    public GameObject unfoldedLadderModel; 

    private bool isPlayerNear = false;

    void Start()
    {
        GetComponent<SphereCollider>().isTrigger = true;

        if (unfoldedLadderModel != null) unfoldedLadderModel.SetActive(false);
        
        if (interactButton != null)
        {
            interactButton.SetActive(false);
            interactButton.GetComponent<Button>().onClick.AddListener(PlaceLadder);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && inventory.hasLadder)
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
        }
    }

    public void PlaceLadder()
    {
        if (!isPlayerNear || !inventory.hasLadder) return;

        inventory.hasLadder = false;
        
        if (interactButton != null) interactButton.SetActive(false);
        
        // Show the usable ladder
        if (unfoldedLadderModel != null) unfoldedLadderModel.SetActive(true);

        GetComponent<SphereCollider>().enabled = false; 
    }
}