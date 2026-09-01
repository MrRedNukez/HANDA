using UnityEngine;

public class RandomPrefabSpawner : MonoBehaviour
{
    [Header("Spawner Configuration")]
    [Tooltip("The Empty GameObject where prefabs will appear.")]
    public Transform spawnPoint;
    
    [Tooltip("Drag all your furniture/appliance prefabs here.")]
    public GameObject[] testPrefabs;

    public void SpawnRandomPrefab()
    {
        if (testPrefabs == null || testPrefabs.Length == 0)
        {
            Debug.LogError("Spawner failed: No prefabs assigned in the array.");
            return;
        }
        if (spawnPoint == null)
        {
            Debug.LogError("Spawner failed: No spawn point assigned.");
            return;
        }

        int randomIndex = Random.Range(0, testPrefabs.Length);

        Instantiate(testPrefabs[randomIndex], spawnPoint.position, spawnPoint.rotation);
    }
}