using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloodProceedureScroller : MonoBehaviour
{
    [System.Serializable]
    public class Procedure
    {
        public string title;
        [TextArea(2, 5)]
        public string description;
        public Sprite image;
    }

    [Header("Procedure Setup")]
    public Procedure[] procedures;

    [Header("UI")]
    public Transform content;
    public GameObject procedurePrefab;

    private void Start()
    {
        GenerateProcedures();
    }

    public void GenerateProcedures()
    {
        if (content == null || procedurePrefab == null)
        {
            Debug.LogWarning(
                "FloodProcedureScroller: Content or Procedure Prefab is missing."
            );

            return;
        }

        // Delete old generated procedures
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        foreach (Procedure procedure in procedures)
        {
            GameObject newProcedure =
                Instantiate(procedurePrefab, content);

            TMP_Text[] texts =
                newProcedure.GetComponentsInChildren<TMP_Text>();

            Image image =
                newProcedure.GetComponentInChildren<Image>();

            if (texts.Length > 0)
            {
                texts[0].text = procedure.title;
            }

            if (texts.Length > 1)
            {
                texts[1].text = procedure.description;
            }

            if (image != null && procedure.image != null)
            {
                image.sprite = procedure.image;
                image.preserveAspect = true;
            }
        }
    }
}