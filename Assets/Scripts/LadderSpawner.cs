using UnityEngine;

public class LadderSpawner : MonoBehaviour
{
    public GameObject ladderPrefab;
    public Transform[] spawnPoints;

    private void Start()
    {
        SpawnLadder();
    }

    private void SpawnLadder()
    {
        if (ladderPrefab == null)
        {
            Debug.LogError("Ladder prefab is not assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);

        Transform spawnPoint = spawnPoints[randomIndex];

        Instantiate(
            ladderPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}