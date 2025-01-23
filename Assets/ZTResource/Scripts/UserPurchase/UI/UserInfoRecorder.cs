#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement; // 引入SceneManager类
using UnityEditor;

/// <summary>
/// 用户信息记录器，用于处理用户注册、文件选择和场景切换等功能
/// </summary>
public class UserInfoRecorder : MonoBehaviour
{
    public TMP_InputField usernameInputField; // 用户名输入框 (TextMeshPro)
    public Image avatarImage; // 头像图片
    public Button createButton; // 创建按钮
    public Button selectFileButton; // 选择文件按钮
    public Button openSceneButton; // 打开道具场景按钮
    public Button openCharacterEditorButton; // 打开CharacterEditor场景按钮

    public GameObject registrationPanel; // 注册页Panel
    public GameObject selectionPanel; // 选择页Panel
    public TMP_Text selectionUsernameText; // 选择页中的用户名文本
    public Image selectionAvatarImage; // 选择页中的头像图片

    public Button deletePrefsButton; // 删除PlayerPrefs按钮

    void Start()
    {
        // 给创建按钮添加点击事件监听
        createButton.onClick.AddListener(SaveUserInfo);
        // 给选择文件按钮添加点击事件监听
        selectFileButton.onClick.AddListener(OpenFileSelectionDialog);
        // 给删除PlayerPrefs按钮添加点击事件监听
        deletePrefsButton.onClick.AddListener(DeletePlayerPrefs);
        // 给打开道具场景按钮添加点击事件监听
        openSceneButton.onClick.AddListener(OpenScene);
        // 给打开CharacterEditor场景按钮添加点击事件监听
        openCharacterEditorButton.onClick.AddListener(OpenCharacterEditorScene);

        // 检查PlayerPrefs是否有保存的用户信息
        string userKey = PlayerPrefs.GetString("currentUserKey", string.Empty);
        if (!string.IsNullOrEmpty(userKey))
        {
            LoadUserInfo(userKey);
            SwitchToSceneSelectionPanel();
        }
        else
        {
            // 如果PlayerPrefs为空，显示注册页面
            registrationPanel.SetActive(true);
            selectionPanel.SetActive(false);
        }
    }

    void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteKey("currentUserKey");
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs data deleted.");

        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SaveUserInfo()
    {
        string username = usernameInputField.text;
        string avatarName = avatarImage.sprite.name + ".png"; // 添加文件后缀名

        // 生成唯一标识符 (用户名 + PurchaseRecordLibrary)
        string userKey = username + "PurchaseRecordLibrary";

        // 将用户名 + PurchaseRecordLibrary 保存到 PlayerPrefs 中
        PlayerPrefs.SetString("currentUserKey", userKey);
        PlayerPrefs.Save();

        // 创建CSV文件名
        string fileName = userKey + ".csv";
        StringBuilder csvContent = new StringBuilder();
        csvContent.AppendLine("用户,头像");
        csvContent.AppendLine($"{username},{avatarName}");

        // 保存路径
        string folderPath = Path.Combine(Application.dataPath, "ZTResource/UserInfo");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, fileName);
        File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);

        Debug.Log("User information saved to: " + filePath);

        // 刷新项目窗口
        AssetDatabase.Refresh();

        // 切换到选择页Panel
        LoadUserInfo(userKey);
        SwitchToSceneSelectionPanel();
    }

    void OpenFileSelectionDialog()
    {
        string folderPath = Path.Combine(Application.dataPath, "ZTResource/UserInfo");
        string filePath = EditorUtility.OpenFilePanel("Select User CSV", folderPath, "csv");

        if (!string.IsNullOrEmpty(filePath))
        {
            // 从文件名中提取用户名并保存到 PlayerPrefs
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            PlayerPrefs.SetString("currentUserKey", fileName);
            PlayerPrefs.Save();

            Debug.Log("User information loaded from: " + filePath);

            // 切换到选择页Panel
            LoadUserInfo(fileName);
            SwitchToSceneSelectionPanel();
        }
    }

    void LoadUserInfo(string userKey)
    {
        string folderPath = Path.Combine(Application.dataPath, "ZTResource/UserInfo");
        string fileName = userKey + ".csv";
        string filePath = Path.Combine(folderPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError("User CSV file not found: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length > 1)
        {
            string[] data = lines[1].Split(',');
            string username = data[0];
            string avatarName = data[1];

            selectionUsernameText.text = username;

            // 加载头像图片
            string avatarPath = Path.Combine(Application.dataPath, "ZTResource/Resources/ZT_Sprites/ZT_User_Icon", Path.GetFileNameWithoutExtension(avatarName) + ".png");
            if (File.Exists(avatarPath))
            {
                byte[] fileData = File.ReadAllBytes(avatarPath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                Sprite avatarSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                selectionAvatarImage.sprite = avatarSprite;
            }
            else
            {
                Debug.LogError("Avatar sprite not found at path: " + avatarPath);
            }
        }
        else
        {
            Debug.LogError("CSV file format is incorrect or empty.");
        }
    }

    void SwitchToSceneSelectionPanel()
    {
        // 隐藏注册页Panel，显示选择页Panel
        registrationPanel.SetActive(false);
        selectionPanel.SetActive(true);
    }

    void OpenScene()
    {
        SceneManager.LoadScene("Assets/ZTResource/ZTResource.unity");
    }

    void OpenCharacterEditorScene()
    {
        SceneManager.LoadScene("CharacterEditor");
    }
}
#endif
