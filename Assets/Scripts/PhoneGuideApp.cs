
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhoneGuideApp : MonoBehaviour
{
    [Serializable]
    public class GuidePage
    {
        public string title;

        [TextArea(2, 5)]
        public string description;

        public Sprite image;
    }

    [Header("Guide UI")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private Image guideImage;
    [SerializeField] private TMP_Text guideTitle;
    [SerializeField] private TMP_Text guideDescription;
    [SerializeField] private TMP_Text pageCounter;

    [Header("Guide Pages")]
    [SerializeField] private List<GuidePage> pages =
        new List<GuidePage>();

    [Header("Navigation Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    [Header("Phone Integration")]
    [SerializeField] private PhoneHUDController phoneController;

    [Header("Game Control")]
    [SerializeField] private bool freezeGameWhenOpen = true;

    private int currentPage = 0;
    private bool isOpen = false;
    private float previousTimeScale = 1f;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        // Hide guide initially.
        if (guidePanel != null)
            guidePanel.SetActive(false);

        // Register button events.
        if (previousButton != null)
        {
            previousButton.onClick.RemoveListener(PreviousPage);
            previousButton.onClick.AddListener(PreviousPage);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextPage);
            nextButton.onClick.AddListener(NextPage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseGuide);
            closeButton.onClick.AddListener(CloseGuide);
        }

        Debug.Log("PhoneGuideApp initialized.");
        Debug.Log("Total Guide Pages: " + pages.Count);
    }

    // OPEN GUIDE
    public void OpenGuide()
    {
        if (isOpen)
            return;

        if (pages.Count == 0)
        {
            Debug.LogWarning("No guide pages assigned!");
            return;
        }

        previousTimeScale = Time.timeScale;

        isOpen = true;
        currentPage = 0;

        if (guidePanel != null)
            guidePanel.SetActive(true);

        UpdatePage();

        if (freezeGameWhenOpen)
            Time.timeScale = 0f;

        Debug.Log("Guide opened.");
    }

    // STARTUP GUIDE
    public void ShowGuide()
    {
        OpenGuide();
    }

    // CLOSE GUIDE
    public void CloseGuide()
    {
        CloseGuideInternal();
    }

    public void CloseGuideForPhoneShutdown()
    {
        CloseGuideInternal();
    }

    public void CloseGuideAndPhone()
    {
        CloseGuideInternal();

        if (phoneController != null)
            phoneController.ClosePhone();
    }

    private void CloseGuideInternal()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (guidePanel != null)
            guidePanel.SetActive(false);

        if (freezeGameWhenOpen)
            Time.timeScale = previousTimeScale;

        Debug.Log("Guide closed.");
    }

    // NEXT PAGE
    public void NextPage()
    {
        Debug.Log("Next button pressed.");

        if (!isOpen)
        {
            Debug.LogWarning("Guide is not open!");
            return;
        }

        if (pages.Count == 0)
        {
            Debug.LogWarning("Guide pages list is empty!");
            return;
        }

        if (currentPage < pages.Count - 1)
        {
            currentPage++;

            UpdatePage();

            Debug.Log("Current Guide Page: " + (currentPage + 1));
        }
        else
        {
            Debug.Log("Already on the last page.");
        }
    }

    // PREVIOUS PAGE
    public void PreviousPage()
    {
        Debug.Log("Previous button pressed.");

        if (!isOpen || pages.Count == 0)
            return;

        if (currentPage > 0)
        {
            currentPage--;

            UpdatePage();

            Debug.Log("Current Guide Page: " + (currentPage + 1));
        }
        else
        {
            Debug.Log("Already on the first page.");
        }
    }

    // UPDATE GUIDE CONTENT
    private void UpdatePage()
    {
        if (pages.Count == 0)
            return;

        currentPage = Mathf.Clamp(
            currentPage,
            0,
            pages.Count - 1
        );

        GuidePage page = pages[currentPage];

        if (guideTitle != null)
            guideTitle.text = page.title;

        if (guideDescription != null)
            guideDescription.text = page.description;

        if (guideImage != null)
        {
            guideImage.sprite = page.image;
            guideImage.enabled = page.image != null;
        }

        if (pageCounter != null)
        {
            pageCounter.text =
                (currentPage + 1) + " / " + pages.Count;
        }

        // Update button availability.
        if (previousButton != null)
        {
            previousButton.interactable = currentPage > 0;
        }

        if (nextButton != null)
        {
            nextButton.interactable =
                currentPage < pages.Count - 1;
        }
    }

    private void OnDestroy()
    {
        if (isOpen && freezeGameWhenOpen)
            Time.timeScale = previousTimeScale;
    }
}
