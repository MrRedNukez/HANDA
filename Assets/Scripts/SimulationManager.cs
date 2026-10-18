using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public enum DeathCause { None, Drowning, Electrocution, Crushed, Explosion }

    [Header("System References")]
    public ReportManager reportManager;
    
    [Header("Flood Level Mechanics")]
    public FuseButtonSequence fuseBox;
    public InventoryManager inventory;

    [Header("Earthquake Level Mechanics")]
    public GasTankMinigame gasTank;
    public bool hasSuccessfullyHidden = false; 

    [Header("Live Data")]
    public bool isPlayerInsideHouse = false;

    public void SetPlayerInside(bool isInside)
    {
        isPlayerInsideHouse = isInside;
    }

    public void EndSimulation(bool survived, DeathCause specificCause = DeathCause.None)
    {
        DeathCause cause = specificCause;

        bool isFloodLevel = (fuseBox != null); 

        if (isFloodLevel)
        {
            // --- FLOOD GRADING ---
            bool isPowerOff = fuseBox.isPowerOff;
            bool isFlashlightUsed = (inventory != null) && inventory.playerOwnsFlashlight;

            if (!survived && cause == DeathCause.None)
            {
                if (isPlayerInsideHouse && !isPowerOff) cause = DeathCause.Electrocution;
                else cause = DeathCause.Drowning;
            }

            if (reportManager != null)
            {
                reportManager.TriggerFloodEvaluation(survived, cause, isPowerOff, isFlashlightUsed);
            }
        }
        else 
        {
            // --- EARTHQUAKE GRADING ---
            bool isGasOff = false;
            if (gasTank != null) isGasOff = gasTank.hasSecuredGas;

            if (!survived && cause == DeathCause.None)
            {
                if (!hasSuccessfullyHidden) cause = DeathCause.Crushed;
                else if (!isGasOff) cause = DeathCause.Explosion;
            }

            if (reportManager != null)
            {
                reportManager.TriggerEarthquakeEvaluation(survived, cause, hasSuccessfullyHidden, isGasOff);
            }
        }
    }
}