using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Level Configuration")]
    [Tooltip("Type the exact names of your scene files here.")]
    public List<string> levelNames = new List<string>();
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelNames.Count)
        {
            SceneManager.LoadScene(levelNames[levelIndex]);
        }
        else
        {
            Debug.LogError("Level index out of bounds! Check the number on your UI Button.");
        }
    }

    public void Exit()
    {
        Application.Quit();
        Debug.Log("Game Exited"); 
    }
}