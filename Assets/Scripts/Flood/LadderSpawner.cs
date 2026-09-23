using UnityEngine;

public class LadderSpawner : MonoBehaviour
{
    [Header("Pre-placed Ladders")]
    public GameObject[] possibleLadders; 

    private void Start()
    {
        if (possibleLadders == null || possibleLadders.Length == 0) return;

        foreach (GameObject ladder in possibleLadders)
        {
            if (ladder != null) ladder.SetActive(false);
        }

        int randomIndex = Random.Range(0, possibleLadders.Length);

        if (possibleLadders[randomIndex] != null)
        {
            possibleLadders[randomIndex].SetActive(true);
            Debug.Log("Ladder spawned at location index: " + randomIndex);
        }
    }
}