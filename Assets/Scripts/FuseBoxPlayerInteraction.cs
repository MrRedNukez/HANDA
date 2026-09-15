using UnityEngine;

public class FuseBoxPlayerInteraction : MonoBehaviour
{
    [Header("Fuse Box UI")]
    public FuseBoxUI fuseBoxUI;

    [Header("Player")]
    public Transform player;

    [Header("Settings")]
    public float interactionDistance = 3f;

    private void Update()
    {
        if (player == null || fuseBoxUI == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // PC keyboard interaction
        if (distance <= interactionDistance)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!fuseBoxUI.IsOpen())
                {
                    fuseBoxUI.OpenPanel();
                }
            }
        }
    }

    // Android / UI button
    public void OpenFuseBox()
    {
        if (fuseBoxUI != null)
        {
            fuseBoxUI.OpenPanel();
        }
    }

    // Close button
    public void CloseFuseBox()
    {
        if (fuseBoxUI != null)
        {
            fuseBoxUI.ClosePanel();
        }
    }
}