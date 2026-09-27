using UnityEngine;
using UnityEngine.UI;

public class LadderEscape : MonoBehaviour
{
    [Header("Hub & Navigation")]
    public SimulationManager simManager;
    public InventoryManager inventory; // NEW: directly reads your inventory script
    public Transform player;
    public Transform roofSpawnPoint;

    [Header("Ladder Visuals")]
    public GameObject ladderMesh; 

    [Header("State")]
    public bool isLadderPlaced = false;

    [Header("UI Buttons")]
    public GameObject climbButtonUI;
    public GameObject placeButtonUI; 

    private void Start()
    {
        if (climbButtonUI != null) climbButtonUI.SetActive(false);
        if (placeButtonUI != null) placeButtonUI.SetActive(false);
        if (ladderMesh != null) ladderMesh.SetActive(false); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Now it checks your actual InventoryManager script!
            if (!isLadderPlaced && inventory != null && inventory.hasLadder)
            {
                if (placeButtonUI != null) placeButtonUI.SetActive(true);
            }
            else if (isLadderPlaced)
            {
                if (climbButtonUI != null) climbButtonUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (climbButtonUI != null) climbButtonUI.SetActive(false);
            if (placeButtonUI != null) placeButtonUI.SetActive(false);
        }
    }

    public void PlaceLadder()
    {
        isLadderPlaced = true;
        
        if (placeButtonUI != null) placeButtonUI.SetActive(false);
        if (climbButtonUI != null) climbButtonUI.SetActive(true);
        if (ladderMesh != null) ladderMesh.SetActive(true);
        
        Debug.Log("Ladder placed successfully.");
    }

    public void ClimbLadder()
    {
        if (climbButtonUI != null) climbButtonUI.SetActive(false);

        // 1. SAFETY TRAP
        if (simManager != null && simManager.fuseBox != null && !simManager.fuseBox.isPowerOff)
        {
            Debug.Log("Ladder Trap Triggered: Power is still ON!");
            simManager.SetPlayerInside(true); 
            simManager.EndSimulation(false);
            return; 
        }

        // 2. INSTANT TELEPORT
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; 
        
        player.position = roofSpawnPoint.position;
        player.rotation = roofSpawnPoint.rotation;
        
        if (cc != null) cc.enabled = true; 

        // 3. WIN CONDITION
        if (simManager != null)
        {
            simManager.EndSimulation(true);
        }
    }
}