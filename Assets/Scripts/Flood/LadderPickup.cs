using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class LadderPickup : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventory; 
    public GameObject interactButton; 
    public GameObject foldedLadderModel; 

    private bool isPlayerNear = false;

    void Start()
    {
        GetComponent<SphereCollider>().isTrigger = true;
        
        if (interactButton != null)
        {
            interactButton.SetActive(false);
            interactButton.GetComponent<Button>().onClick.AddListener(TakeLadder);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !inventory.hasLadder)
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

    public void TakeLadder()
    {
        if (!isPlayerNear) return;

        inventory.hasLadder = true;
        Debug.Log("Ladder added to inventory!");

        if (interactButton != null) interactButton.SetActive(false);

        if (foldedLadderModel != null) foldedLadderModel.SetActive(false);
        GetComponent<SphereCollider>().enabled = false; 
    }
}