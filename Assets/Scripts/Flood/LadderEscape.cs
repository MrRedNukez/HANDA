using UnityEngine;

public class LadderEscape : MonoBehaviour
{
    [Header("Hub & Navigation")]
    public SimulationManager simManager;
    public Transform player;
    public Transform roofSpawnPoint;

    // Call this method when the player interacts with the ladder
    public void ClimbLadder()
    {
        // 1. THE TRAP CHECK (Optional, but recommended)
        // If they touch the wet metal ladder while the power is ON, they die.
        if (simManager != null && simManager.fuseBox != null && !simManager.fuseBox.isPowerOff)
        {
            Debug.Log("Ladder Electrocution Trap Triggered!");
            // Force the hub to recognize this as an indoor electrocution
            simManager.SetPlayerInside(true); 
            simManager.EndSimulation(false);
            return; // Stop the code here so they don't teleport
        }

        // 2. THE TELEPORT
        CharacterController cc = player.GetComponent<CharacterController>();
        
        if (cc != null) cc.enabled = false; // Disable physics
        
        player.position = roofSpawnPoint.position;
        player.rotation = roofSpawnPoint.rotation;
        
        if (cc != null) cc.enabled = true; // Re-enable physics

        // 3. THE WIN CONDITION
        Debug.Log("Player reached the safe zone! Triggering survival report.");
        if (simManager != null)
        {
            simManager.EndSimulation(true);
        }
    }
}