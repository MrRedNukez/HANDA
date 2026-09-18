using UnityEngine;

public class LadderPickup : MonoBehaviour
{
    public GameObject unfoldedLadderPrefab;

    public bool isPickedUp = false;

    public void Pickup()
    {
        if (isPickedUp)
            return;

        isPickedUp = true;

        // Hide the folded ladder
        gameObject.SetActive(false);

        Debug.Log("Ladder picked up!");
    }

    public void Place(Vector3 position, Quaternion rotation)
    {
        if (!isPickedUp)
            return;

        if (unfoldedLadderPrefab == null)
        {
            Debug.LogError("Unfolded Ladder Prefab is not assigned!");
            return;
        }

        // Spawn the unfolded ladder at the placement point
        Instantiate(
            unfoldedLadderPrefab,
            position,
            rotation
        );

        Debug.Log("Unfolded ladder spawned!");

        isPickedUp = false;
    }
}