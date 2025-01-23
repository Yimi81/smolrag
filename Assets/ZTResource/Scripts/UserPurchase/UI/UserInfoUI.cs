using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class UserInfoUI : MonoBehaviour
{
    public Image userAvatarImage;
    public TextMeshProUGUI userNameText;

    public void SetupUserInfo(string userName, string userAvatarFileName)
    {
        userNameText.text = userName;

        // 使用Resources.Load从资源文件夹中加载图像
        string avatarResourcePath = "ZT_Sprites/ZT_User_Icon/" + Path.GetFileNameWithoutExtension(userAvatarFileName);
        userAvatarImage.sprite = Resources.Load<Sprite>(avatarResourcePath);

        if (userAvatarImage.sprite == null)
        {
            Debug.LogError("Failed to load avatar sprite from Resources at path: " + avatarResourcePath);
        }
    }

#if UNITY_EDITOR
    private Sprite LoadSpriteFromFile(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError("File not found at path: " + filePath);
            return null;
        }

        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogError("Failed to load texture from file: " + filePath);
            return null;
        }
    }
#endif
}
