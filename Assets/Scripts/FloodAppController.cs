
using UnityEngine;

public class FloodAppController : MonoBehaviour
{
    [Header("Home Screen")]
    public GameObject homeScreen;

    [Header("Flood App")]
    public GameObject floodApp;

    [Header("Earthquake App")]
    public GameObject eqApp;

    // Open Flood App
    public void OpenFloodApp()
    {
        if (homeScreen != null)
            homeScreen.SetActive(false);

        if (floodApp != null)
            floodApp.SetActive(true);

        if (eqApp != null)
            eqApp.SetActive(false);
    }

    // Close Flood App
    public void CloseFloodApp()
    {
        if (floodApp != null)
            floodApp.SetActive(false);

        if (homeScreen != null)
            homeScreen.SetActive(true);
    }

    // Open Earthquake App
    public void OpenEarthquakeApp()
    {
        if (homeScreen != null)
            homeScreen.SetActive(false);

        if (floodApp != null)
            floodApp.SetActive(false);

        if (eqApp != null)
            eqApp.SetActive(true);
    }

    // Close Earthquake App
    public void CloseEarthquakeApp()
    {
        if (eqApp != null)
            eqApp.SetActive(false);

        if (homeScreen != null)
            homeScreen.SetActive(true);
    }
}
