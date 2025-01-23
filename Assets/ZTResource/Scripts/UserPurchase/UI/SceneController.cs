
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class SceneController : MonoBehaviour
{
    public GameObject searchPanel; // 插槽：搜索页面
    public GameObject purchaseRecordPanel; // 插槽：购买记录页面
    public Image userAvatar; // 插槽：用户头像
    public TMP_Text userName; // 插槽：用户名称
    public Button searchPageButton; // 插槽：搜索页面按钮
    public Button returnToHomePageButton; // 插槽：退回首页按钮
    public Button enterPurchaseRecordButton; // 插槽：进入购买记录页面按钮

    private string currentUserFilePath;

    private void Start()
    {
        LoadUserInfo();
        ShowSearchPanel();

        searchPageButton.onClick.AddListener(ShowSearchPanel);
        returnToHomePageButton.onClick.AddListener(ReturnToHomePage);
        enterPurchaseRecordButton.onClick.AddListener(ShowPurchaseRecordPanel);
    }
    // 这个脚本的目的是加载用户信息，包括用户名和用户头像
    private void LoadUserInfo()
    {
        string userKey = PlayerPrefs.GetString("currentUserKey", string.Empty);
        if (!string.IsNullOrEmpty(userKey))
        {
            string folderPath = Path.Combine(Application.dataPath, "ZTResource/UserInfo");
            string fileName = userKey + ".csv";
            currentUserFilePath = Path.Combine(folderPath, fileName);

            if (File.Exists(currentUserFilePath))
            {
                string[] lines = File.ReadAllLines(currentUserFilePath);
                if (lines.Length > 1)
                {
                    string[] data = lines[1].Split(',');
                    string username = data[0];
                    string avatarName = Path.GetFileNameWithoutExtension(data[1]);

                    userName.text = username;
                    string avatarPath = Path.Combine(Application.dataPath, "ZTResource/Resources/ZT_Sprites/ZT_User_Icon", avatarName + ".png");
                    if (File.Exists(avatarPath))
                    {
                        byte[] fileData = File.ReadAllBytes(avatarPath);
                        Texture2D texture = new Texture2D(2, 2);
                        texture.LoadImage(fileData);
                        Sprite avatarSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        userAvatar.sprite = avatarSprite;
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
            else
            {
                Debug.LogError("User CSV file not found: " + currentUserFilePath);
            }
        }
    }


    private void ShowSearchPanel()
    {
        searchPanel.SetActive(true);
        purchaseRecordPanel.SetActive(false);
    }

    private void ShowPurchaseRecordPanel()
    {
        LoadPurchaseRecords(); // 新增方法调用
        searchPanel.SetActive(false);
        purchaseRecordPanel.SetActive(true);
    }

    private void LoadPurchaseRecords()
    {
        var (userName, userAvatar, batches) = CSVParser.ParseCSV(currentUserFilePath);

        // 更新UI逻辑
        // 例如，假设有一个 `MainUIController` 来管理购买记录的UI
        MainUIController mainUIController = purchaseRecordPanel.GetComponent<MainUIController>();
        if (mainUIController != null)
        {
            mainUIController.SetupPurchaseRecords(userName, userAvatar, batches, currentUserFilePath); // 传递CSV文件路径
        }
    }

    private void ReturnToHomePage()
    {
        SceneManager.LoadScene("Assets/ZTResource/ZTResource_Home.unity");
    }
}
