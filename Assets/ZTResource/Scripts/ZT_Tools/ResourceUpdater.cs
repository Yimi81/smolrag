#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

public class ResourceUpdater : EditorWindow
{
    private string csvFilePath = "Assets/ZTResource/Resources/ZT_TagLibrary/ResourceIndexLibrary.csv";
    private List<string> prefabPaths = new List<string>(); // List of user-specified prefab paths
    private Vector2 scrollPosition; // For scroll view
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>(); // Prefab cache

    [MenuItem("ZTResource/Tools/CSV给资源添加Resourcelnfo", false, 7)]
    public static void ShowWindow()
    {
        GetWindow<ResourceUpdater>("CSV给资源添加Resourcelnfo");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV文件路径", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        csvFilePath = EditorGUILayout.TextField(csvFilePath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("选择CSV文件", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                csvFilePath = "Assets" + path.Substring(Application.dataPath.Length); // Convert to relative path
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("预制体路径", EditorStyles.boldLabel);
        HandleDragAndDrop();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        if (prefabPaths.Count > 0)
        {
            GUILayout.Label("已选择的预制体或文件夹：");
            DisplayPathsWithRemoveButton(prefabPaths);
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("载入"))
        {
            UpdateResourceInfo();
        }
    }

    private TagLibrary LoadTagLibrary(string name)
    {
        return AssetDatabase.LoadAssetAtPath<TagLibrary>($"Assets/ZTResource/Resources/ZT_TagLibrary/{name}.asset");
    }

    private HashSet<string> LoadAllTags()
    {
        var tagLibraries = new List<TagLibrary>
        {
            LoadTagLibrary("TypeTagLibrary"),
            LoadTagLibrary("ThemeTagLibrary"),
            LoadTagLibrary("FunctionTagLibrary"),
            LoadTagLibrary("DefinitionTagLibrary"),
            LoadTagLibrary("BatchTagLibrary"),
            LoadTagLibrary("PropertyTagLibrary")
        };

        var allTags = new HashSet<string>();

        foreach (var library in tagLibraries)
        {
            if (library != null)
            {
                foreach (var tag in library.tags)
                {
                    allTags.Add(tag);
                }
            }
        }

        return allTags;
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "拖拽预制体或文件夹到这里");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (string draggedObject in DragAndDrop.paths)
                    {
                        if (Directory.Exists(draggedObject) || File.Exists(draggedObject))
                        {
                            prefabPaths.Add(draggedObject);
                        }
                    }
                }
                Event.current.Use();
                break;
        }
    }

    private void DisplayPathsWithRemoveButton(List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(paths[i]);
            if (GUILayout.Button("移除", GUILayout.Width(60)))
            {
                paths.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }
    }

    private GameObject GetPrefab(string prefabPath)
    {
        if (!prefabCache.TryGetValue(prefabPath, out var prefab))
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                prefabCache[prefabPath] = prefab;
            }
        }
        return prefab;
    }

    private void UpdateResourceInfo()
    {
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError("CSV文件不存在: " + csvFilePath);
            return;
        }

        var resourceDataList = LoadResourceData();
        List<string> processedPrefabs = new List<string>();

        foreach (var data in resourceDataList)
        {
            // 构建新的Prefab路径
            string prefabPath = "Assets/ArtResource/Scenes/Standard/" + data.Id + ".prefab";

            if (prefabPaths.Count == 0)
            {
                if (ProcessPrefab(data, prefabPath))
                {
                    processedPrefabs.Add(prefabPath);
                }
            }
            else
            {
                bool underSpecifiedPaths = false;
                foreach (string path in prefabPaths)
                {
                    if (prefabPath.StartsWith(path))
                    {
                        underSpecifiedPaths = true;
                        break;
                    }
                }

                if (underSpecifiedPaths)
                {
                    if (ProcessPrefab(data, prefabPath))
                    {
                        processedPrefabs.Add(prefabPath);
                    }
                }
            }
        }

        AssetDatabase.SaveAssets(); // Save all changes after processing
        AssetDatabase.Refresh(); // Refresh the database

        if (processedPrefabs.Count > 0)
        {
            foreach (var prefab in processedPrefabs)
            {
                Debug.Log($"成功给预制体 {prefab} 添加了 ResourceInfo");
            }
        }
        else
        {
            Debug.Log("没有预制体被处理");
        }

        Debug.Log("资源信息更新完成");
    }

    private bool ProcessPrefab(ResourceCardData data, string prefabPath)
    {
        try
        {
            GameObject prefab = GetPrefab(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("未找到预制体: " + prefabPath);
                return false;
            }

            ResourceInfo info = prefab.GetComponent<ResourceInfo>();
            if (info == null)
            {
                info = prefab.AddComponent<ResourceInfo>();
            }

            var tagLibrary = LoadAllTags();

            info.id = data.Id;
            info.resourceName = data.Name;
            info.resourceDescription = data.Description;
            info.itemHeight = data.Height;
            info.prefabPath = prefabPath; // Update to set the prefabPath
            info.thumbnailPath = data.ThumbnailPath;
            info.modelFaces = data.ModelFaces;
            info.creationDate = data.CreationDate;
            info.updatedDate = data.UpdatedDate;
            info.version = data.Version;

            // Filter tags that exist in the TagLibrary
            info.typeTags = data.TypeTags.Where(tag => tagLibrary.Contains(tag)).ToList();
            info.themeTags = data.ThemeTags.Where(tag => tagLibrary.Contains(tag)).ToList();
            info.functionTags = data.FunctionTags.Where(tag => tagLibrary.Contains(tag)).ToList();
            info.definitionTags = data.DefinitionTags.Where(tag => tagLibrary.Contains(tag)).ToList();
            info.batchTags = data.BatchTags.Where(tag => tagLibrary.Contains(tag)).ToList();
            info.propertyTags = data.PropertyTags.Where(tag => tagLibrary.Contains(tag)).ToList();

            // Load and assign resourceThumbnail
            if (!string.IsNullOrEmpty(info.thumbnailPath))
            {
                Texture2D thumbnailTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(info.thumbnailPath);
                if (thumbnailTexture != null)
                {
                    info.resourceThumbnail = thumbnailTexture;
                    Debug.Log($"成功加载缩略图: {info.thumbnailPath}");
                }
                else
                {
                    Debug.LogWarning($"未能加载缩略图: {info.thumbnailPath}");
                }
            }

            EditorUtility.SetDirty(prefab); // Mark prefab as dirty

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"处理预制体 {prefabPath} 时出错: {ex.Message}");
            return false;
        }
    }
    private List<ResourceCardData> LoadResourceData()
    {
        var resourceDataList = new List<ResourceCardData>();
        string[] lines = File.ReadAllLines(csvFilePath);
        bool isFirstLine = true;

        foreach (string line in lines)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] fields = line.Split(',');
            if (fields.Length < 16) // Ensure correct number of fields
            {
                Debug.LogWarning($"行被跳过，因为字段数量不正确: {line}");
                continue;
            }

            var id = fields[0].Trim('"');
            var name = fields[1].Trim('"');
            var description = fields[2].Trim('"');
            var height = fields[3].Trim('"');
            // Skip fields[4], which is the unreliable prefabPath
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

            ResourceCardData data = new ResourceCardData(id, name, description, height, typeTags, themeTags, functionTags, definitionTags, batchTags, propertyTags, thumbnailPath, modelFaces, creationDate, updatedDate, version);
            resourceDataList.Add(data);
        }

        return resourceDataList;
    }

    public class ResourceCardData
    {
        public string Id;
        public string Name;
        public string Description;
        public string Height;
        public string ThumbnailPath;
        public string ModelFaces;
        public string CreationDate;
        public string UpdatedDate;
        public string Version;
        public List<string> TypeTags;
        public List<string> ThemeTags;
        public List<string> FunctionTags;
        public List<string> DefinitionTags;
        public List<string> BatchTags;
        public List<string> PropertyTags;

        public ResourceCardData(string id, string name, string description, string height, List<string> typeTags, List<string> themeTags, List<string> functionTags, List<string> definitionTags, List<string> batchTags, List<string> propertyTags, string thumbnailPath, string modelFaces, string creationDate, string updatedDate, string version)
        {
            Id = id;
            Name = name;
            Description = description;
            Height = height;
            ThumbnailPath = thumbnailPath;
            ModelFaces = modelFaces;
            CreationDate = creationDate;
            UpdatedDate = updatedDate;
            Version = version;
            TypeTags = typeTags;
            ThemeTags = themeTags;
            FunctionTags = functionTags;
            DefinitionTags = definitionTags;
            BatchTags = batchTags;
            PropertyTags = propertyTags;
        }
    }
}

#endif
