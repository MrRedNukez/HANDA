using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class InventoryBag : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactButton; 
    public CanvasGroup inventoryPanel; 
    public Button closeInventoryButton; 

    private bool isPlayerNear = false;

    void Start()
    {
        
        GetComponent<SphereCollider>().isTrigger = true;

        if (interactButton != null) interactButton.SetActive(false);
        CloseInventory();

        if (interactButton != null) 
        {
            interactButton.GetComponent<Button>().onClick.AddListener(OpenInventory);
        }
        
        if (closeInventoryButton != null)
        {
            closeInventoryButton.onClick.AddListener(CloseInventory);
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
            CloseInventory(); 
        }
    }

    public void OpenInventory()
    {
        if (!isPlayerNear) return;

        if (interactButton != null) interactButton.SetActive(false);

        if (inventoryPanel != null)
        {
            inventoryPanel.alpha = 1f;
            inventoryPanel.interactable = true;
            inventoryPanel.blocksRaycasts = true;
        }
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.alpha = 0f;
            inventoryPanel.interactable = false;
            inventoryPanel.blocksRaycasts = false;
        }

        if (isPlayerNear && interactButton != null)
        {
            interactButton.SetActive(true);
        }
    }
}