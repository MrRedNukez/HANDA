using UnityEngine;

public class FloodAppController : MonoBehaviour
{
    [Header("Home Screen")]
    public GameObject homeScreen;

    [Header("Flood App")]
    public GameObject floodApp;

    [Header("Earthquake App")]
    public GameObject eqApp;

    [Header("Guide App")]
    public GameObject guideApp;

    [Header("App Controller")]
    public PhoneGuideApp phoneGuideApp;

    // Open Flood App
    public void OpenFloodApp()
    {
        if (homeScreen != null)
            homeScreen.SetActive(false);

        if (floodApp != null)
            floodApp.SetActive(true);

        if (eqApp != null)
            eqApp.SetActive(false);

        if (guideApp != null)
            guideApp.SetActive(false);
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

        if (guideApp != null)
            guideApp.SetActive(false);
    }

    // Close Earthquake App
    public void CloseEarthquakeApp()
    {
        if (eqApp != null)
            eqApp.SetActive(false);

        if (homeScreen != null)
            homeScreen.SetActive(true);
    }

    // Open Guide App
    public void OpenGuideApp()
    {
        if (homeScreen != null)
            homeScreen.SetActive(false);

        if (floodApp != null)
            floodApp.SetActive(false);

        if (eqApp != null)
            eqApp.SetActive(false);

        if (guideApp != null)
            guideApp.SetActive(true);

        if (phoneGuideApp != null)
            phoneGuideApp.OpenGuide();
    }

    // Close Guide App - HOME BUTTON
    public void CloseGuideApp()
    {
        // Close guide logic and restore gameplay.
        if (phoneGuideApp != null)
            phoneGuideApp.CloseGuide();

        if (guideApp != null)
            guideApp.SetActive(false);

        if (homeScreen != null)
            homeScreen.SetActive(true);
    }
}