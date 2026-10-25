using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloodProceedureScroller : MonoBehaviour
{
    public enum ProcedureType
    {
        Flood,
        Earthquake
    }

    [System.Serializable]
    public class Procedure
    {
        public string title;

        [TextArea(2, 5)]
        public string description;

        public Sprite image;
    }

    [Header("Procedure Type")]
    public ProcedureType procedureType = ProcedureType.Flood;

    [Header("Flood Procedures")]
    public Procedure[] floodProcedures;
    public GameObject floodProcedurePrefab;

    [Header("Earthquake Procedures")]
    public Procedure[] earthquakeProcedures;
    public GameObject earthquakeProcedurePrefab;

    [Header("UI")]
    public Transform content;

    private void Start()
    {
        GenerateProcedures();
    }

    public void GenerateProcedures()
    {
        if (content == null)
        {
            Debug.LogWarning(
                "FloodProceedureScroller: Content is missing."
            );
            return;
        }

        // Choose the correct prefab and procedure array.
        GameObject selectedPrefab;
        Procedure[] selectedProcedures;

        if (procedureType == ProcedureType.Flood)
        {
            selectedPrefab = floodProcedurePrefab;
            selectedProcedures = floodProcedures;
        }
        else
        {
            selectedPrefab = earthquakeProcedurePrefab;
            selectedProcedures = earthquakeProcedures;
        }

        if (selectedPrefab == null)
        {
            Debug.LogWarning(
                "FloodProceedureScroller: The prefab for " +
                procedureType + " is missing."
            );
            return;
        }

        if (selectedProcedures == null)
        {
            Debug.LogWarning(
                "FloodProceedureScroller: The procedure array for " +
                procedureType + " is missing."
            );
            return;
        }

        // Delete previously generated procedures.
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        // Generate the selected procedures.
        foreach (Procedure procedure in selectedProcedures)
        {
            GameObject newProcedure = Instantiate(
                selectedPrefab,
                content
            );

            TMP_Text[] texts =
                newProcedure.GetComponentsInChildren<TMP_Text>(true);

            Image[] images =
                newProcedure.GetComponentsInChildren<Image>(true);

            // Assign title.
            if (texts.Length > 0)
            {
                texts[0].text = procedure.title;
            }

            // Assign description.
            if (texts.Length > 1)
            {
                texts[1].text = procedure.description;
            }

            // Find the image component whose GameObject
            // is intended to display the procedure image.
            if (images.Length > 0 && procedure.image != null)
            {
                // By default, use the first Image component.
                images[0].sprite = procedure.image;
                images[0].preserveAspect = true;
            }
        }
    }
}