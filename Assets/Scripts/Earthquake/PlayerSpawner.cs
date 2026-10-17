using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Existing player in the scene to move. Leave empty to spawn the prefab instead.")]
    public Transform player;
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    [Tooltip("Optional: drop a parent here and right-click the script > 'Fill From Children' to fill the array.")]
    public Transform spawnPointsParent;

    [Header("Options")]
    public bool spawnOnStart = true;
    [Tooltip("Use each spawn point's blue arrow (forward) as the facing direction. Off = random rotation.")]
    public bool useSpawnPointRotation = true;
    [Tooltip("Avoid picking the same point twice in a row when respawning.")]
    public bool avoidRepeat = true;
    [Tooltip("Parent the player to the spawn point so they move with the shaking house.")]
    public bool parentToSpawnPoint = false;

    private int lastIndex = -1;

    void Start()
    {
        if (spawnOnStart) Spawn();
    }

    public void Spawn()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("PlayerSpawner: No spawn points assigned!", this);
            return;
        }

        Transform point = PickPoint();
        if (point == null) return;

        Vector3 pos = point.position;
        Quaternion rot = useSpawnPointRotation
            ? Quaternion.Euler(0f, point.eulerAngles.y, 0f)
            : Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        if (player == null)
        {
            if (playerPrefab == null)
            {
                Debug.LogWarning("PlayerSpawner: Assign a player or a prefab!", this);
                return;
            }
            player = Instantiate(playerPrefab, pos, rot).transform;
        }
        else
        {
            Teleport(player, pos, rot);
        }

        if (parentToSpawnPoint) player.SetParent(point, true);
    }

    Transform PickPoint()
    {
        // Count valid (non-null) points
        int validCount = 0;
        foreach (var p in spawnPoints) if (p != null) validCount++;
        if (validCount == 0)
        {
            Debug.LogWarning("PlayerSpawner: All spawn points are empty!", this);
            return null;
        }

        int index;
        int tries = 0;
        do
        {
            index = Random.Range(0, spawnPoints.Length);
            tries++;
        }
        while ((spawnPoints[index] == null || (avoidRepeat && validCount > 1 && index == lastIndex)) && tries < 100);

        lastIndex = index;
        return spawnPoints[index];
    }

    void Teleport(Transform target, Vector3 pos, Quaternion rot)
    {
        // CharacterController overrides transform changes unless disabled first
        var cc = target.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        target.SetPositionAndRotation(pos, rot);

        var body = target.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;   // use body.velocity on older Unity versions
            body.angularVelocity = Vector3.zero;
        }

        if (cc != null) cc.enabled = true;
    }

    [ContextMenu("Fill From Children")]
    void FillFromChildren()
    {
        if (spawnPointsParent == null)
        {
            Debug.LogWarning("PlayerSpawner: Assign a Spawn Points Parent first.", this);
            return;
        }

        spawnPoints = new Transform[spawnPointsParent.childCount];
        for (int i = 0; i < spawnPoints.Length; i++)
            spawnPoints[i] = spawnPointsParent.GetChild(i);
    }

    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.green;
        foreach (var p in spawnPoints)
        {
            if (p == null) continue;
            Gizmos.DrawWireSphere(p.position, 0.3f);
            Gizmos.DrawLine(p.position, p.position + p.forward * 0.6f);
        }
    }
}