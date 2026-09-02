using UnityEngine;

public class RandomPrefabSpawner : MonoBehaviour
{
    [Header("Spawner Configuration")]
    [Tooltip("Drag multiple empty GameObjects here to act as random spawn positions.")]
    public Transform[] spawnPoints;
    
    [Tooltip("Drag all your furniture/appliance prefabs here.")]
    public GameObject[] testPrefabs;

    [Header("Safety Limit")]
    [Tooltip("How many seconds to wait before allowing another spawn.")]
    public float spawnCooldown = 1.0f;
    private float nextSpawnTime = 0f;

    public void SpawnRandomPrefab()
    {
        if (Time.time < nextSpawnTime) 
        {
            return; 
        }

        // 2. Critical failsafes
        if (testPrefabs == null || testPrefabs.Length == 0)
        {
            Debug.LogError("Spawner failed: No prefabs assigned in the array.");
            return;
        }
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawner failed: No spawn points assigned in the array.");
            return;
        }

        int randomPrefabIndex = Random.Range(0, testPrefabs.Length);
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);

        Transform selectedSpawn = spawnPoints[randomSpawnIndex];

        Instantiate(testPrefabs[randomPrefabIndex], selectedSpawn.position, selectedSpawn.rotation);

        nextSpawnTime = Time.time + spawnCooldown;
    }
}