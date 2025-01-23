using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System.Linq;

// 负责从CSV文件加载资源数据，并在游戏开始时初始化资源信息。
public class ResourceManager : MonoBehaviour
{
    private List<ResourceCardData> allResources = new List<ResourceCardData>();

    void Awake()
    {
        LoadResourceData();
    }

    void Start()
    {
        PrintAllResources();
    }

    private void LoadResourceData()
    {
        // 使用 Resources.Load 加载 CSV 文件
        TextAsset csvFile = Resources.Load<TextAsset>("ZT_TagLibrary/ResourceIndexLibrary");
        if (csvFile == null)
        {
            Debug.LogError("无法加载CSV文件，请检查文件路径和名称是否正确。");
            return;
        }

        // 读取文件内容并分割为行
        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        bool isFirstLine = true;

        foreach (string line in lines)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("ID")) continue;

            string[] fields = line.Split(',');
            var id = fields[0].Trim('"');
            var name = fields[1].Trim('"');
            var description = fields[2].Trim('"');
            var height = fields[3].Trim('"');
            var prefabPath = fields[4].Trim('"');
            var thumbnailPath = fields[5].Trim('"');
            var modelFaces = fields[6].Trim('"');
            var creationDate = fields[7].Trim('"');
            var updatedDate = fields[8].Trim('"');
            var version = fields[9].Trim('"');
            var typeTags = new List<string>(fields[10].Trim('"').Split(';').Select(tag => tag.Trim()));
            var themeTags = new List<string>(fields[11].Trim('"').Split(';').Select(tag => tag.Trim()));
            var functionTags = new List<string>(fields[12].Trim('"').Split(';').Select(tag => tag.Trim()));
            var definitionTags = new List<string>(fields[13].Trim('"').Split(';').Select(tag => tag.Trim()));
            var batchTags = new List<string>(fields[14].Trim('"').Split(';').Select(tag => tag.Trim()));
            var propertyTags = new List<string>(fields[15].Trim('"').Split(';').Select(tag => tag.Trim()));

            ResourceCardData data = new ResourceCardData(id, name, description, height, typeTags, themeTags, functionTags, definitionTags, batchTags, propertyTags, prefabPath, thumbnailPath, modelFaces, creationDate, updatedDate, version);
            allResources.Add(data);
        }
    }


    public ResourceCardData GetResourceById(string id)
    {
        return allResources.FirstOrDefault(resource => resource.ID == id);
    }

    public List<ResourceCardData> GetAllResources()
    {
        // 确保在 Start 或者其他地方初始化了 allResources
        return allResources;
    }

    private void PrintAllResources()
    {
        foreach (var resource in allResources)
        {
            //Debug.Log($"Name: {resource.Name}, Description: {resource.Description}, Height: {resource.Height}, " +
            //          $"Type Tags: {string.Join(", ", resource.TypeTags)}, Theme Tags: {string.Join(", ", resource.ThemeTags)}, " +
            //          $"Function Tags: {string.Join(", ", resource.FunctionTags)}, Definition Tags: {string.Join(", ", resource.DefinitionTags)}, " +
            //          $"Batch Tags: {string.Join(", ", resource.BatchTags)}, Property Tags: {string.Join(", ", resource.PropertyTags)}, " +
            //          $"Prefab Path: {resource.PrefabPath}, Thumbnail Path: {resource.ThumbnailPath}, Model Faces: {resource.ModelFaces}, " +
            //          $"Creation Date: {resource.CreationDate}, Updated Date: {resource.UpdatedDate}, Version: {resource.Version}, ID: {resource.ID}");
        }
    }
}