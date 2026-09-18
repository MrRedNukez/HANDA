using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3f;

    private LadderPickup carriedLadder;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void Interact()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, interactDistance))
        {
            Debug.Log("Nothing detected.");
            return;
        }

        Debug.Log("Raycast hit: " + hit.collider.name);

        // PICK UP FOLDED LADDER
        LadderPickup ladder =
            hit.collider.GetComponentInParent<LadderPickup>();

        if (ladder != null && carriedLadder == null)
        {
            ladder.Pickup();
            carriedLadder = ladder;

            Debug.Log("Picked up ladder!");
            return;
        }

        // PLACE LADDER
        LadderPlacement placement =
            hit.collider.GetComponentInParent<LadderPlacement>();

        if (placement != null && carriedLadder != null)
        {
            placement.PlaceLadder(carriedLadder);
            carriedLadder = null;

            Debug.Log("Placed ladder!");
            return;
        }

        Debug.Log("Object has no ladder interaction.");
    }
}