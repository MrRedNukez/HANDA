using UnityEngine;

public class FloodAppController : MonoBehaviour
{
    public GameObject homeScreen;
    public GameObject floodApp;

    public void OpenFloodApp()
    {
        if (homeScreen != null)
            homeScreen.SetActive(false);

        if (floodApp != null)
            floodApp.SetActive(true);
    }

    public void CloseFloodApp()
    {
        if (floodApp != null)
            floodApp.SetActive(false);

        if (homeScreen != null)
            homeScreen.SetActive(true);
    }
}