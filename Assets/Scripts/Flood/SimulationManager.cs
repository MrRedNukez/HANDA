using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public enum DeathCause { None, Drowning, Electrocution }

    [Header("System References")]
    public ReportManager reportManager;
    public FuseBoxButtonSequence fuseBox;
    public InventoryManager inventory;

    [Header("Live Data")]
    public bool isPlayerInsideHouse = false;

    public void SetPlayerInside(bool isInside)
    {
        isPlayerInsideHouse = isInside;
    }

    public void EndSimulation(bool survived)
    {
        DeathCause cause = DeathCause.None;

        // Failsafe: Check FuseBox
        bool isPowerOff = false;
        if (fuseBox != null)
        {
            isPowerOff = fuseBox.isPowerOff;
        }
        else
        {
            Debug.LogWarning("Fuse Box not linked to SimulationManager! Assuming power is ON.");
        }

        // Failsafe: Check Inventory
        bool isFlashlightUsed = false;
        if (inventory != null)
        {
            isFlashlightUsed = inventory.playerOwnsFlashlight;
        }
        else
        {
            Debug.LogWarning("Inventory not linked to SimulationManager! Assuming flashlight missing.");
        }

        if (!survived)
        {
            if (isPlayerInsideHouse && !isPowerOff)
            {
                cause = DeathCause.Electrocution;
            }
            else
            {
                cause = DeathCause.Drowning;
            }
        }

        if (reportManager != null)
        {
            reportManager.TriggerEvaluation(survived, cause, isPowerOff, isFlashlightUsed);
        }
    }
}