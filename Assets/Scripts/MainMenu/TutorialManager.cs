using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject tutorialPanel; 

    void Start()
    {
        if (PlayerPrefs.GetInt("HasSeenTutorial", 0) == 0)
        {
            tutorialPanel.SetActive(true);

            PlayerPrefs.SetInt("HasSeenTutorial", 1);
            PlayerPrefs.Save();
        }
        else
        {
            tutorialPanel.SetActive(false);
        }
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
    }

    public void OpenTutorialManually()
    {
        tutorialPanel.SetActive(true);
    }

    public void ResetTutorialSave()
    {
        PlayerPrefs.DeleteKey("HasSeenTutorial");
        Debug.Log("Tutorial save deleted. It will auto-pop up on the next playtest.");
    }
}