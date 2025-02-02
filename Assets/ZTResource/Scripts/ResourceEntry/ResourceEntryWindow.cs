#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class ResourceEntryWindow : EditorWindow
{
    private GameObject prefabToUpload;
    private string resourceName;
    private string resourceDescription;
    private Texture2D resourceThumbnail;
    private List<string> selectedTypeTags = new List<string>();
    private List<string> selectedThemeTags = new List<string>();
    private List<string> selectedFunctionTags = new List<string>();
    private List<string> selectedDefinitionTags = new List<string>();
    private List<string> selectedBatchTags = new List<string>();
    private List<string> selectedPropertyTags = new List<string>();

    private string searchFilter = "";
    private int itemsPerPage = 40;
    private Dictionary<string, List<string>> tagSearchResults = new Dictionary<string, List<string>>();

    private readonly string csvFilePath = "Assets/ZTResource/Resources/ZT_TagLibrary/ResourceIndexLibrary.csv";

    private TagLibrary typeTagLibrary, themeTagLibrary, functionTagLibrary, definitionTagLibrary, batchTagLibrary, propertyTagLibrary;

    private Dictionary<string, int> currentPages = new Dictionary<string, int>();

    [MenuItem("ZTResource/录入-资源",false, 0)]
    public static void ShowWindow()
    {
        GetWindow<ResourceEntryWindow>("资源录入");
    }

    private void Awake()
    {
        typeTagLibrary = LoadTagLibrary("TypeTagLibrary");
        themeTagLibrary = LoadTagLibrary("ThemeTagLibrary");
        functionTagLibrary = LoadTagLibrary("FunctionTagLibrary");
        definitionTagLibrary = LoadTagLibrary("DefinitionTagLibrary");
        batchTagLibrary = LoadTagLibrary("BatchTagLibrary");
        propertyTagLibrary = LoadTagLibrary("PropertyTagLibrary");

        string[] tagTypes = new string[] { "Type", "Theme", "Function", "Definition", "Batch", "Property" };
        foreach (string tagType in tagTypes)
        {
            currentPages[tagType] = 0;
            FilterTagsForLibrary(LoadTagLibrary(tagType + "TagLibrary"), "", tagType);
        }
    }

    private void FilterTags(string filter)
    {
        FilterTagsForLibrary(typeTagLibrary, filter, "Type");
        FilterTagsForLibrary(themeTagLibrary, filter, "Theme");
        FilterTagsForLibrary(functionTagLibrary, filter, "Function");
        FilterTagsForLibrary(definitionTagLibrary, filter, "Definition");
        FilterTagsForLibrary(batchTagLibrary, filter, "Batch");
        FilterTagsForLibrary(propertyTagLibrary, filter, "Property");
    }

    private void FilterTagsForLibrary(TagLibrary library, string filter, string tagType)
    {
        if (library == null) return;
        if (string.IsNullOrEmpty(filter))
        {
            tagSearchResults[tagType] = new List<string>(library.tags);
        }
        else
        {
            tagSearchResults[tagType] = library.tags.Where(tag => tag.ToLowerInvariant().Contains(filter.ToLowerInvariant())).ToList();
        }
    }

    private void DisplayTagSelectionWithPaging(string label, TagLibrary library, ref List<string> selectedTags, string tagType)
    {
        EditorGUILayout.LabelField(label);

        if (!currentPages.ContainsKey(tagType))
        {
            currentPages[tagType] = 0;
        }

        if (!tagSearchResults.ContainsKey(tagType) || tagSearchResults[tagType] == null)
        {
            FilterTagsForLibrary(library, "", tagType);
        }

        var tagsToShow = tagSearchResults[tagType];
        int totalTags = tagsToShow.Count;
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)totalTags / itemsPerPage));

        int currentPage = currentPages[tagType];

        int columns = 4;
        int startIdx = currentPage * itemsPerPage;
        int endIdx = Mathf.Min(startIdx + itemsPerPage, totalTags);

        for (int i = startIdx; i < endIdx; i++)
        {
            if ((i - startIdx) % columns == 0)
            {
                EditorGUILayout.BeginHorizontal();
            }

            string tag = tagsToShow[i];
            bool isSelected = selectedTags.Contains(tag);
            bool selection = GUILayout.Toggle(isSelected, tag, "Button", GUILayout.Width(EditorGUIUtility.currentViewWidth / columns - 6));

            if (selection != isSelected)
            {
                if (selection)
                {
                    selectedTags.Add(tag);
                }
                else
                {
                    selectedTags.Remove(tag);
                }
            }

            if ((i - startIdx + 1) % columns == 0 || i == endIdx - 1)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        if (totalPages > 1)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("上一页") && currentPage > 0)
            {
                currentPages[tagType] = currentPage - 1;
            }
            if (GUILayout.Button("下一页") && currentPage < totalPages - 1)
            {
                currentPages[tagType] = currentPage + 1;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"页码: {currentPage + 1} / {totalPages}", EditorStyles.centeredGreyMiniLabel);
        }
    }

    private Vector2 scrollPosition;

    private void OnGUI()
    {
        float windowHeight = this.position.height;
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(this.position.width - 5), GUILayout.Height(windowHeight - 25));

        prefabToUpload = (GameObject)EditorGUILayout.ObjectField("预制体", prefabToUpload, typeof(GameObject), false);
        if (prefabToUpload != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(prefabToUpload);
            if (assetPath.StartsWith("Assets") && assetPath.EndsWith(".prefab"))
            {
                // 去除公共部分路径
                string basePath = "Assets/ArtResource/Scenes/Standard/";
                if (assetPath.StartsWith(basePath))
                {
                    assetPath = assetPath.Substring(basePath.Length);
                }

                // 删除路径中的“/”符号
                string modifiedPrefabPath = assetPath.Replace("/", "");

                // 去除 .prefab 后缀
                string prefabName = Path.GetFileNameWithoutExtension(modifiedPrefabPath);

                // 获取缩略图名称
                string thumbnailName = prefabName + ".png";

                // 加载缩略图
                resourceThumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ZTResource/Resources/ZT_IconTextures/" + thumbnailName);
                if (resourceThumbnail == null)
                {
                    EditorGUILayout.HelpBox("未找到对应的缩略图文件！", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField("已加载缩略图: " + thumbnailName);
                    EditorGUILayout.ObjectField("资源缩略图", resourceThumbnail, typeof(Texture2D), false);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("这不是一个有效的预制体文件！", MessageType.Warning);
                prefabToUpload = null;
            }
        }

        resourceName = EditorGUILayout.TextField("资源名称", resourceName);
        resourceDescription = EditorGUILayout.TextField("资源描述", resourceDescription);

        searchFilter = EditorGUILayout.TextField("搜索标签", searchFilter);
        if (EditorGUI.EndChangeCheck())
        {
            FilterTags(searchFilter);
        }

        DisplayTagSelectionWithPaging("类型标签", typeTagLibrary, ref selectedTypeTags, "Type");
        DisplayTagSelectionWithPaging("主题标签", themeTagLibrary, ref selectedThemeTags, "Theme");
        DisplayTagSelectionWithPaging("区域标签", definitionTagLibrary, ref selectedDefinitionTags, "Definition");
        DisplayTagSelectionWithPaging("功能标签", functionTagLibrary, ref selectedFunctionTags, "Function");
        DisplayTagSelectionWithPaging("批次标签", batchTagLibrary, ref selectedBatchTags, "Batch");
        DisplayTagSelectionWithPaging("属性标签", propertyTagLibrary, ref selectedPropertyTags, "Property");

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("保存资源"))
        {
            SaveResource();
        }
    }


    private TagLibrary LoadTagLibrary(string name)
    {
        return AssetDatabase.LoadAssetAtPath<TagLibrary>($"Assets/ZTResource/Resources/ZT_TagLibrary/{name}.asset");
    }

    public class ResourceData
    {
        public string ID;
        public string Name;
        public string Description;
        public string Height;
        public string PrefabPath;
        public string ThumbnailPath;
        public string ModelFaces;
        public string CreationDate;
        public string UpdatedDate;
        public string Version;
        public string TypeTags;
        public string ThemeTags;
        public string FunctionTags;
        public string DefinitionTags;
        public string BatchTags;
        public string PropertyTags;

        public override string ToString()
        {
            List<string> entries = new List<string>
            {
                $"\"{ID}\"",
                $"\"{Name}\"",
                $"\"{Description}\"",
                $"\"{Height}\"",                
                $"\"{PrefabPath}\"",
                $"\"{ThumbnailPath}\"",
                $"\"{ModelFaces}\"",
                $"\"{CreationDate}\"",
                $"\"{UpdatedDate}\"",
                $"\"{Version}\"",
                $"\"{TypeTags}\"",
                $"\"{ThemeTags}\"",
                $"\"{FunctionTags}\"",
                $"\"{DefinitionTags}\"",
                $"\"{BatchTags}\"",
                $"\"{PropertyTags}\""
            };
            return string.Join(",", entries);
        }
    }

    private string CalculatePrefabDimensions(GameObject prefab)
    {
        if (prefab == null) return "0|0|0";

        var renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return "0|0|0";

        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        float length = (float)Math.Round(bounds.size.x, 1);
        float width = (float)Math.Round(bounds.size.z, 1);
        float height = (float)Math.Round(bounds.size.y, 1);

        return $"{length}|{width}|{height}";
    }



    private string CalculateModelFaces(GameObject prefab)
    {
        int totalFaces = 0;
        MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh != null)
            {
                totalFaces += meshFilter.sharedMesh.triangles.Length / 3;
            }
        }
        return totalFaces.ToString();
    }

    private void SaveResource()
    {
        if (prefabToUpload == null)
        {
            EditorUtility.DisplayDialog("错误", "请添加一个预制体。", "确定");
            return;
        }

        string dimensions = CalculatePrefabDimensions(prefabToUpload);

        // 更新 UI
        Repaint();

        string thumbnailName = resourceThumbnail != null ? resourceThumbnail.name : "";

        if (string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(resourceDescription))
        {
            EditorUtility.DisplayDialog("错误", "资源名称和描述不能为空。", "确定");
            return;
        }

        // 检查预制体上是否已有 ResourceInfo 组件
        ResourceInfo info = prefabToUpload.GetComponent<ResourceInfo>();
        if (info == null)
        {
            info = prefabToUpload.AddComponent<ResourceInfo>();
        }

        // 使用路径生成ID，替换原有的GUID生成方式
        string prefabPath = AssetDatabase.GetAssetPath(prefabToUpload);
        string basePath = "Assets/ArtResource/Scenes/Standard/";

        if (prefabPath.StartsWith(basePath))
        {
            prefabPath = prefabPath.Substring(basePath.Length);
        }
        string resourceId = System.IO.Path.ChangeExtension(prefabPath, null); // 去掉文件后缀作为ID

        ResourceData data = new ResourceData
        {
            ID = resourceId,
            Name = resourceName,
            Description = resourceDescription,
            Height = dimensions,
            PrefabPath = AssetDatabase.GetAssetPath(prefabToUpload),
            ThumbnailPath = thumbnailName,
            ModelFaces = CalculateModelFaces(prefabToUpload),
            CreationDate = DateTime.Now.ToString("yyyy/MM/dd"),
            UpdatedDate = DateTime.Now.ToString("yyyy/MM/dd"),
            Version = "0",
            TypeTags = string.Join(";", selectedTypeTags),
            ThemeTags = string.Join(";", selectedThemeTags),
            FunctionTags = string.Join(";", selectedFunctionTags),
            DefinitionTags = string.Join(";", selectedDefinitionTags),
            BatchTags = string.Join(";", selectedBatchTags),
            PropertyTags = string.Join(";", selectedPropertyTags)
        };

        info.id = data.ID;
        info.resourceName = resourceName;
        info.resourceDescription = resourceDescription;
        info.resourceThumbnail = resourceThumbnail;
        info.prefabPath = data.PrefabPath;
        info.thumbnailPath = thumbnailName;
        info.modelFaces = data.ModelFaces;
        info.creationDate = data.CreationDate;
        info.updatedDate = data.UpdatedDate;
        info.version = data.Version;
        info.itemHeight = dimensions;
        info.typeTags = selectedTypeTags;
        info.themeTags = selectedThemeTags;
        info.functionTags = selectedFunctionTags;
        info.definitionTags = selectedDefinitionTags;
        info.batchTags = selectedBatchTags;
        info.propertyTags = selectedPropertyTags;

        EditorUtility.SetDirty(prefabToUpload);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        WriteToCsv(data);
    }


    private void WriteToCsv(ResourceData data)
    {
        string csvContent = data.ToString() + "\n";
        bool isSuccess = false;

        if (!File.Exists(csvFilePath))
        {
            string header = "资源ID,资源名称,资源描述,长宽高,属性标签,预制体路径,缩略图路径,面数,创建时间,更新时间,版本,类型标签,主题标签,功能标签,区域标签,批次标签\n";
            try
            {
                File.WriteAllText(csvFilePath, header + csvContent, new System.Text.UTF8Encoding(true));
                isSuccess = true;
            }
            catch (IOException ex)
            {
                if (IsFileLocked(ex))
                {
                    EditorUtility.DisplayDialog("错误", "无法保存资源信息，文件可能为只读或已被打开，请关闭文件后重试。", "确定");
                }
                else
                {
                    throw;
                }
            }
        }
        else
        {
            try
            {
                File.AppendAllText(csvFilePath, csvContent, new System.Text.UTF8Encoding(true));
                isSuccess = true;
            }
            catch (IOException ex)
            {
                if (IsFileLocked(ex))
                {
                    EditorUtility.DisplayDialog("错误", "无法保存资源信息，文件可能为只读或已被打开，请关闭文件后重试。", "确定");
                }
                else
                {
                    throw;
                }
            }
        }

        if (isSuccess)
        {
            EditorUtility.DisplayDialog("资源保存", "资源信息已保存！", "确定");
        }
    }

    private bool IsFileLocked(IOException exception)
    {
        int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(exception) & ((1 << 16) - 1);
        return errorCode == 32 || errorCode == 33;
    }
}
#endif