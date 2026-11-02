using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageLogApp : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject logPanel;             // the messages app screen
    [SerializeField] private RectTransform contentParent;     // Scroll View > Viewport > Content
    [SerializeField] private MessageItemUI messageItemPrefab; // prefab with MessageItemUI on it
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject emptyLabel;          // optional: "No messages yet"

    [Header("Sender")]
    [SerializeField] private Sprite senderAvatar;             // the Aunt image

    [Header("Log Messages (shown in the order the phone receives messages)")]
    [TextArea(2, 4)]
    [SerializeField] private List<string> logMessages = new List<string>();

    [Header("Buttons (optional, leave empty if your icon already opens the app)")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    private int nextIndex = 0;

    public bool IsOpen => logPanel != null && logPanel.activeSelf;

    private void Awake()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(OpenLog);
            openButton.onClick.AddListener(OpenLog);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseLog);
            closeButton.onClick.AddListener(CloseLog);
        }

        RefreshEmptyLabel();
    }

    // SimulationTimeline calls this whenever the phone receives a message.
    // The received text is only used as a backup if your list runs out.
    public void AddMessage(string receivedText, string dayLabel = "")
    {
        if (messageItemPrefab == null || contentParent == null)
        {
            Debug.LogWarning("MessageLogApp: Message Item Prefab or Content Parent is not assigned!");
            return;
        }

        string textToShow;

        if (nextIndex < logMessages.Count)
        {
            textToShow = logMessages[nextIndex];
        }
        else
        {
            Debug.LogWarning("MessageLogApp: no log entry for message #" + (nextIndex + 1) +
                             ", showing the received text instead.");
            textToShow = receivedText;
        }

        // Only the first message shows the Aunt picture
        bool showAvatar = nextIndex == 0;

        MessageItemUI item = Instantiate(messageItemPrefab, contentParent);
        item.Setup(textToShow, senderAvatar, showAvatar);

        nextIndex++;
        RefreshEmptyLabel();

        if (IsOpen)
            StartCoroutine(ScrollToBottom());
    }

    public void OpenLog()
    {
        if (logPanel != null)
            logPanel.SetActive(true);

        StartCoroutine(ScrollToBottom());
    }

    public void CloseLog()
    {
        if (logPanel != null)
            logPanel.SetActive(false);
    }

    private void RefreshEmptyLabel()
    {
        if (emptyLabel != null)
            emptyLabel.SetActive(nextIndex == 0);
    }

    private IEnumerator ScrollToBottom()
    {
        yield return null;

        if (contentParent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}