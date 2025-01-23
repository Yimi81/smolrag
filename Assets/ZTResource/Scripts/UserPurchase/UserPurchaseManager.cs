#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System.Linq;

// 负责从CSV文件加载用户购买数据，并在游戏开始时初始化用户信息。
public class UserPurchaseManager : MonoBehaviour
{
    private List<UserPurchaseData> allUserPurchases = new List<UserPurchaseData>();

    void Awake()
    {
        LoadUserPurchaseData();
    }

    void Start()
    {
        PrintAllUserPurchases();
    }

    private void LoadUserPurchaseData()
    {
        string path = "Assets/ZTResource/UserInfo/UserPurchaseRecordLibrary.csv";
        Debug.Log($"Loading data from: {path}");
        if (!File.Exists(path))
        {
            Debug.LogError($"File not found: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
        Debug.Log($"Total lines read: {lines.Length}");
        bool isFirstLine = true; // 增加一个变量来跟踪是否是第一行
        bool isNewBatch = false;
        string currentBatchName = "";

        foreach (string line in lines)
        {
            Debug.Log($"Processing line: {line}");

            if (isFirstLine || line.StartsWith("资源ID"))
            {
                isFirstLine = false; // 如果是第一行或标题行，就将其标记为false，并继续下一行
                continue;
            }
            if (string.IsNullOrWhiteSpace(line)) continue; // 跳过空行

            if (line.StartsWith("批次名"))
            {
                currentBatchName = line.Split(',')[0];
                isNewBatch = true;
                Debug.Log($"New batch found: {currentBatchName}");
                continue;
            }

            string[] fields = line.Split(',');

            if (isNewBatch)
            {
                isNewBatch = false;
                continue;
            }

            // 检查字段数量是否足够
            if (fields.Length < 18)
            {
                Debug.LogWarning($"Line skipped due to insufficient fields: {line}");
                continue;
            }

            try
            {
                var userId = fields[0].Trim('"');
                var resourceId = fields[1].Trim('"');
                var userName = fields[2].Trim('"');
                var resourceName = fields[3].Trim('"');
                var resourceDescription = fields[4].Trim('"');
                var resourceHeight = fields[5].Trim('"');              
                var prefabPath = fields[6].Trim('"');
                var thumbnailPath = fields[7].Trim('"');
                var modelFaces = fields[8].Trim('"');
                var creationDate = fields[9].Trim('"');
                var updatedDate = fields[10].Trim('"');
                var version = fields[11].Trim('"');
                var typeTags = new List<string>(fields[12].Trim('"').Split(';').Select(tag => tag.Trim()));
                var themeTags = new List<string>(fields[13].Trim('"').Split(';').Select(tag => tag.Trim()));
                var functionTags = new List<string>(fields[14].Trim('"').Split(';').Select(tag => tag.Trim()));
                var definitionTags = new List<string>(fields[15].Trim('"').Split(';').Select(tag => tag.Trim()));
                var batchTags = new List<string>(fields[16].Trim('"').Split(';').Select(tag => tag.Trim()));
                var propertyTags = new List<string>(fields[17].Trim('"').Split(';').Select(tag => tag.Trim()));

                UserPurchaseData data = new UserPurchaseData(userId, resourceId, userName, resourceName, resourceDescription, resourceHeight, typeTags, themeTags, functionTags, definitionTags, batchTags, propertyTags, prefabPath, thumbnailPath, modelFaces, creationDate, updatedDate, version);
                allUserPurchases.Add(data);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing line: {line}\nException: {ex.Message}");
            }
        }

        Debug.Log($"Total purchases loaded: {allUserPurchases.Count}");
    }

    public List<UserPurchaseData> GetAllUserPurchases()
    {
        return allUserPurchases;
    }

    private void PrintAllUserPurchases()
    {
        Debug.Log("Printing all user purchases...");
        foreach (var purchase in allUserPurchases)
        {
            Debug.Log($"Name: {purchase.ResourceName}, Description: {purchase.ResourceDescription}, Height: {purchase.ResourceHeight}, " +
                      $"Type Tags: {string.Join(", ", purchase.TypeTags)}, Theme Tags: {string.Join(", ", purchase.ThemeTags)}, " +
                      $"Function Tags: {string.Join(", ", purchase.FunctionTags)}, Definition Tags: {string.Join(", ", purchase.DefinitionTags)}, " +
                      $"Batch Tags: {string.Join(", ", purchase.BatchTags)}, Property Tags: {string.Join(", ", purchase.PropertyTags)}, " +
                      $"Prefab Path: {purchase.PrefabPath}, Thumbnail Path: {purchase.ThumbnailPath}, Model Faces: {purchase.ModelFaces}, " +
                      $"Creation Date: {purchase.CreationDate}, Updated Date: {purchase.UpdatedDate}, Version: {purchase.Version}, ID: {purchase.ResourceID}, User: {purchase.UserName}");
        }
    }
}

public class UserPurchaseData
{
    public string UserID { get; private set; }
    public string ResourceID { get; private set; }
    public string UserName { get; private set; }
    public string ResourceName { get; private set; }
    public string ResourceDescription { get; private set; }
    public string ResourceHeight { get; private set; }
    public List<string> TypeTags { get; private set; }
    public List<string> ThemeTags { get; private set; }
    public List<string> FunctionTags { get; private set; }
    public List<string> DefinitionTags { get; private set; }
    public List<string> BatchTags { get; private set; }
    public List<string> PropertyTags { get; private set; }
    public string PrefabPath { get; private set; }
    public string ThumbnailPath { get; private set; }
    public string ModelFaces { get; private set; }
    public string CreationDate { get; private set; }
    public string UpdatedDate { get; private set; }
    public string Version { get; private set; }

    public UserPurchaseData(string userId, string resourceId, string userName, string resourceName, string resourceDescription, string resourceHeight,
                            List<string> typeTags, List<string> themeTags, List<string> functionTags, List<string> definitionTags, List<string> batchTags,
                            List<string> propertyTags, string prefabPath, string thumbnailPath, string modelFaces, string creationDate, string updatedDate, string version)
    {
        UserID = userId;
        ResourceID = resourceId;
        UserName = userName;
        ResourceName = resourceName;
        ResourceDescription = resourceDescription;
        ResourceHeight = resourceHeight;
        TypeTags = typeTags;
        ThemeTags = themeTags;
        FunctionTags = functionTags;
        DefinitionTags = definitionTags;
        BatchTags = batchTags;
        PropertyTags = propertyTags;
        PrefabPath = prefabPath;
        ThumbnailPath = thumbnailPath;
        ModelFaces = modelFaces;
        CreationDate = creationDate;
        UpdatedDate = updatedDate;
        Version = version;
    }
}
#endif