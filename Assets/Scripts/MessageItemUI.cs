using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageItemUI : MonoBehaviour
{
    public Image avatarImage;     // the Aunt picture
    public TMP_Text messageText;  // the message text

    public void Setup(string text, Sprite avatar, bool showAvatar)
    {
        messageText.text = text;
        avatarImage.sprite = avatar;
        avatarImage.enabled = showAvatar && avatar != null;
    }
}