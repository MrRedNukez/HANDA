using UnityEngine;

public class LadderPlacement : MonoBehaviour
{
    public Transform placementPoint;

    private bool ladderPlaced = false;

    public void PlaceLadder(LadderPickup ladder)
    {
        if (ladderPlaced)
            return;

        if (ladder == null)
        {
            Debug.LogError("No ladder was picked up!");
            return;
        }

        if (placementPoint == null)
        {
            Debug.LogError("Placement Point is not assigned!");
            return;
        }

        ladderPlaced = true;

        ladder.Place(
            placementPoint.position,
            placementPoint.rotation
        );

        Debug.Log("SUCCESS: Unfolded ladder placed at balcony!");
    }
}