using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    // EXPANDED: Added Crushed and Explosion for the Earthquake level
    public enum DeathCause { None, Drowning, Electrocution, Crushed, Explosion }

    [Header("System References")]
    public ReportManager reportManager;
    
    [Header("Flood Level Mechanics")]
    public FuseButtonSequence fuseBox;
    public InventoryManager inventory;

    [Header("Earthquake Level Mechanics")]
    // We will link your colleague's gas stove script here later
    public bool isUnderCover = false; 
    public bool isGasOff = false;

    [Header("Live Data")]
    public bool isPlayerInsideHouse = false;

    public void SetPlayerInside(bool isInside)
    {
        isPlayerInsideHouse = isInside;
    }

    // UPDATED: Now accepts an optional specific cause of death (defaults to None)
    public void EndSimulation(bool survived, DeathCause specificCause = DeathCause.None)
    {
        DeathCause cause = specificCause;

        // Failsafe: Only check Flood Mechanics if they actually exist in this scene
        bool isPowerOff = false;
        if (fuseBox != null) isPowerOff = fuseBox.isPowerOff;

        bool isFlashlightUsed = false;
        if (inventory != null) isFlashlightUsed = inventory.playerOwnsFlashlight;

        // AUTOMATIC CAUSE DETECTION (If a specific cause wasn't provided by a trap)
        if (!survived && cause == DeathCause.None)
        {
            // If we are in the Flood level (because a fuseBox is linked)
            if (fuseBox != null)
            {
                if (isPlayerInsideHouse && !isPowerOff) cause = DeathCause.Electrocution;
                else cause = DeathCause.Drowning;
            }
            // If we are in the Earthquake level (no fuseBox linked)
            else
            {
                if (!isUnderCover) cause = DeathCause.Crushed;
                else if (!isGasOff) cause = DeathCause.Explosion;
            }
        }

        if (reportManager != null)
        {
            // Note: We may need to update ReportManager later to track the gas stove boolean!
            reportManager.TriggerEvaluation(survived, cause, isPowerOff, isFlashlightUsed);
        }
    }
}