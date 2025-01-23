#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

public class UserAvatarSelector : MonoBehaviour
{
    public Button openAvatarSelectionButton; // 用于打开选择页面的按钮
    public GameObject avatarSelectionPanel;  // 选择页面的面板
    public Button[] avatarButtons;           // 选择页面上的头像按钮
    public Image selectedAvatarDisplay;      // 用于显示选择的头像

    private void Start()
    {
        // 隐藏选择页面
        avatarSelectionPanel.SetActive(false);

        // 添加按钮点击事件
        openAvatarSelectionButton.onClick.AddListener(OpenAvatarSelection);

        // 为每个头像按钮添加点击事件
        foreach (Button button in avatarButtons)
        {
            button.onClick.AddListener(() => SelectAvatar(button));
        }
    }

    private void OpenAvatarSelection()
    {
        // 显示选择页面
        avatarSelectionPanel.SetActive(true);
    }

    private void SelectAvatar(Button clickedButton)
    {
        // 获取选择的头像（假设头像是按钮的子对象的Image组件）
        Image avatarImage = clickedButton.GetComponentInChildren<Image>();

        // 更新显示的头像
        selectedAvatarDisplay.sprite = avatarImage.sprite;

        // 隐藏选择页面
        avatarSelectionPanel.SetActive(false);
    }
}
#endif