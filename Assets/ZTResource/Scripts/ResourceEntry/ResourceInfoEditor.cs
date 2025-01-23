#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceInfo))]
public class ResourceInfoEditor : Editor
{
    private SerializedProperty idProp, resourceNameProp, resourceDescriptionProp, thumbnailPathProp, modelFacesProp, creationDateProp, updatedDateProp, versionProp, itemHeightProp;
    private SerializedProperty typeTagsProp, themeTagsProp, functionTagsProp, definitionTagsProp, batchTagsProp, propertyTagsProp;

    private TagLibrary typeTagLibrary, themeTagLibrary, functionTagLibrary, definitionTagLibrary, batchTagLibrary, propertyTagLibrary;
    private int typeTagPageIndex = 0, themeTagPageIndex = 0, functionTagPageIndex = 0, definitionTagPageIndex = 0, batchTagPageIndex = 0, propertyTagPageIndex = 0;
    private string searchFilter = "";
    private Dictionary<string, string> searchFilters = new Dictionary<string, string>();

    public void OnEnable()
    {
        typeTagLibrary = LoadTagLibrary("TypeTagLibrary");
        themeTagLibrary = LoadTagLibrary("ThemeTagLibrary");
        functionTagLibrary = LoadTagLibrary("FunctionTagLibrary");
        definitionTagLibrary = LoadTagLibrary("DefinitionTagLibrary");
        batchTagLibrary = LoadTagLibrary("BatchTagLibrary");
        propertyTagLibrary = LoadTagLibrary("PropertyTagLibrary");

        idProp = serializedObject.FindProperty("id");
        resourceNameProp = serializedObject.FindProperty("resourceName");
        resourceDescriptionProp = serializedObject.FindProperty("resourceDescription");
        thumbnailPathProp = serializedObject.FindProperty("thumbnailPath");
        modelFacesProp = serializedObject.FindProperty("modelFaces");
        itemHeightProp = serializedObject.FindProperty("itemHeight");
        creationDateProp = serializedObject.FindProperty("creationDate");
        updatedDateProp = serializedObject.FindProperty("updatedDate");
        versionProp = serializedObject.FindProperty("version");
        typeTagsProp = serializedObject.FindProperty("typeTags");
        themeTagsProp = serializedObject.FindProperty("themeTags");
        functionTagsProp = serializedObject.FindProperty("functionTags");
        definitionTagsProp = serializedObject.FindProperty("definitionTags");
        batchTagsProp = serializedObject.FindProperty("batchTags");
        propertyTagsProp = serializedObject.FindProperty("propertyTags");


        searchFilters["Type"] = "";
        searchFilters["Theme"] = "";
        searchFilters["Function"] = "";
        searchFilters["Definition"] = "";
        searchFilters["Batch"] = "";
        searchFilters["Property"] = "";
    }

    public override void OnInspectorGUI()
    {
        ResourceInfo resourceInfo = (ResourceInfo)target;

        if (resourceInfo == null)
        {
            EditorGUILayout.HelpBox("ResourceInfo is null", MessageType.Error);
            return;
        }

        serializedObject.Update();
        GUI.color = Color.cyan;

        if (GUILayout.Button("<b><color=yellow>资源版本更新</color></b>", new GUIStyle(GUI.skin.button) { richText = true }))
        {
            resourceInfo.updatedDate = DateTime.Now.ToString("yyyy/MM/dd");
            int version = int.Parse(resourceInfo.version);
            resourceInfo.version = (version + 1).ToString();
            EditorUtility.SetDirty(resourceInfo);
        }

        // 恢复GUI颜色为默认值
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(idProp, new GUIContent("ID"));
        EditorGUILayout.PropertyField(resourceNameProp, new GUIContent("名称"));
        EditorGUILayout.PropertyField(resourceDescriptionProp, new GUIContent("资源"));
        EditorGUILayout.PropertyField(modelFacesProp, new GUIContent("面数"));
        EditorGUILayout.PropertyField(itemHeightProp, new GUIContent("长、宽、高"));
        EditorGUILayout.PropertyField(creationDateProp, new GUIContent("创建日期"));
        EditorGUILayout.PropertyField(updatedDateProp, new GUIContent("更新时间"));
        EditorGUILayout.PropertyField(versionProp, new GUIContent("版本"));

        EditorGUILayout.LabelField("缩略图");

        // 调用新的展示缩略图方法，避免标记为脏
        RefreshThumbnailAndDisplay();

        if (GUILayout.Button("更新信息"))
        {
            RefreshData();
        }

        EditorGUI.BeginChangeCheck();
        searchFilter = EditorGUILayout.TextField("搜索所有标签", searchFilter).ToLower();
        if (EditorGUI.EndChangeCheck())
        {
            ResetAllPageIndexes();
            UpdateSearchFilters(searchFilter);
        }

        DisplayTagSelectionWithPaging("类型标签", typeTagLibrary, ref typeTagPageIndex, typeTagsProp);
        DisplayTagSelectionWithPaging("主题标签", themeTagLibrary, ref themeTagPageIndex, themeTagsProp);
        DisplayTagSelectionWithPaging("区域分类", definitionTagLibrary, ref definitionTagPageIndex, definitionTagsProp);
        DisplayTagSelectionWithPaging("功能标签", functionTagLibrary, ref functionTagPageIndex, functionTagsProp);
        DisplayTagSelectionWithPaging("批次标签", batchTagLibrary, ref batchTagPageIndex, batchTagsProp);
        DisplayTagSelectionWithPaging("属性标签", propertyTagLibrary, ref propertyTagPageIndex, propertyTagsProp);

        serializedObject.ApplyModifiedProperties();
    }


    private void RefreshThumbnailAndDisplay()
    {
        ResourceInfo resourceInfo = (ResourceInfo)target;

        if (resourceInfo == null)
        {
            Debug.LogWarning("ResourceInfo 脚本未找到！");
            return;
        }

        // 获取预制体的路径
        string prefabPath = AssetDatabase.GetAssetPath(resourceInfo.gameObject);
        if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab"))
        {
            return;
        }

        // 删除路径中的“/”符号并去除公共部分路径
        string basePath = "Assets/ArtResource/Scenes/Standard/";
        if (prefabPath.StartsWith(basePath))
        {
            prefabPath = prefabPath.Substring(basePath.Length);
        }

        string modifiedPrefabPath = prefabPath.Replace("/", "");
        string ThumbnailName = System.IO.Path.GetFileNameWithoutExtension(modifiedPrefabPath);

        // 构建缩略图路径
        string thumbnailPath = $"Assets/ZTResource/Resources/ZT_IconTextures/{ThumbnailName}.png";
        Texture2D newThumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);

        if (newThumbnail)
        {
            // 直接显示图片，不修改 SerializedProperty
            GUILayout.Label("缩略图预览：");
            GUILayout.Label(newThumbnail, GUILayout.Width(100), GUILayout.Height(100));
        }
        else
        {
            // 自定义样式：红色加粗字体
            GUIStyle redBoldStyle = new GUIStyle(EditorStyles.boldLabel);
            redBoldStyle.normal.textColor = Color.yellow;
            redBoldStyle.fontSize = 14;

            // 显示提示信息，未找到对应缩略图
            GUILayout.Label("未找到对应缩略图,在资源录入前一定要先截图", redBoldStyle);
        }
    }


    private void RefreshData()
    {
        ResourceInfo resourceInfo = (ResourceInfo)target;
        if (resourceInfo != null && resourceInfo.gameObject != null)
        {
            string prefabPath = AssetDatabase.GetAssetPath(resourceInfo.gameObject);
            string basePath = "Assets/ArtResource/Scenes/Standard/";

            if (prefabPath.StartsWith(basePath))
            {
                prefabPath = prefabPath.Substring(basePath.Length);
            }

            string prefabPathWithoutExtension = System.IO.Path.ChangeExtension(prefabPath, null);
            idProp.stringValue = prefabPathWithoutExtension;

            string facesCount = CalculateModelFaces(resourceInfo.gameObject);
            modelFacesProp.stringValue = facesCount;

            string dimensions = CalculatePrefabDimensions(resourceInfo.gameObject);
            itemHeightProp.stringValue = dimensions;

            // 删除过时的缩略图路径更新逻辑，仅记录 ThumbnailName
            string modifiedPrefabPath = prefabPath.Replace("/", "");
            string ThumbnailName = System.IO.Path.GetFileNameWithoutExtension(modifiedPrefabPath);

            // 记录 ThumbnailName（假设你有一个对应的字段）
            resourceInfo.thumbnailPath = ThumbnailName;  // 假设有一个名为 thumbnailName 的字段

            thumbnailPathProp.stringValue = ThumbnailName;

            serializedObject.ApplyModifiedProperties();
            Debug.Log($"更新后的 ThumbnailName: {ThumbnailName}");
            Debug.Log($"更新后的 thumbnailPath: {resourceInfo.thumbnailPath}");
        }
    }


    private TagLibrary LoadTagLibrary(string libraryName)
    {
        return AssetDatabase.LoadAssetAtPath<TagLibrary>($"Assets/ZTResource/Resources/ZT_TagLibrary/{libraryName}.asset");
    }

    private void DisplayTagSelectionWithPaging(string label, TagLibrary tagLibrary, ref int pageIndex, SerializedProperty tagsProp)
    {
        if (tagLibrary == null)
        {
            EditorGUILayout.HelpBox("标签库未找到。请确保TagLibrary对象已经创建并且路径正确。", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField(label);
        int itemsPerPage = 35;
        int columns = 4;

        string searchFilterKey = label.Replace(" ", "");
        if (!searchFilters.ContainsKey(searchFilterKey))
        {
            searchFilters[searchFilterKey] = "";
        }

        IEnumerable<string> filteredTags = tagLibrary.tags.Where(tag => tag.ToLower().Contains(searchFilters[searchFilterKey]));
        int totalTags = filteredTags.Count();
        int totalPages = (totalTags + itemsPerPage - 1) / itemsPerPage;

        int startIdx = pageIndex * itemsPerPage;
        int endIdx = Mathf.Min(startIdx + itemsPerPage, totalTags);

        for (int i = startIdx; i < endIdx; i++)
        {
            if ((i - startIdx) % columns == 0)
            {
                EditorGUILayout.BeginHorizontal();
            }

            bool present = IsTagPresent(tagsProp, filteredTags.ElementAt(i - startIdx));
            bool toggled = GUILayout.Toggle(present, filteredTags.ElementAt(i - startIdx), "Button", GUILayout.Width(EditorGUIUtility.currentViewWidth / columns - 10));

            if (toggled != present)
            {
                if (toggled)
                    AddTag(tagsProp, filteredTags.ElementAt(i - startIdx));
                else
                    RemoveTag(tagsProp, filteredTags.ElementAt(i - startIdx));
            }

            if ((i - startIdx + 1) % columns == 0 || i == endIdx - 1)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        if (totalPages > 1)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("上一页") && pageIndex > 0)
            {
                pageIndex--;
            }
            if (GUILayout.Button("下一页") && pageIndex < totalPages - 1)
            {
                pageIndex++;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"页码: {pageIndex + 1} / {totalPages}", EditorStyles.centeredGreyMiniLabel);
        }
    }

    private bool IsTagPresent(SerializedProperty tagsProp, string tag)
    {
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return true;
        }
        return false;
    }

    private void AddTag(SerializedProperty tagsProp, string tag)
    {
        tagsProp.arraySize++;
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
    }

    private void RemoveTag(SerializedProperty tagsProp, string tag)
    {
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
            {
                tagsProp.DeleteArrayElementAtIndex(i);
                return;
            }
        }
    }

    private void ResetAllPageIndexes()
    {
        typeTagPageIndex = themeTagPageIndex = functionTagPageIndex = definitionTagPageIndex = batchTagPageIndex = propertyTagPageIndex = 0;
    }

    private void UpdateSearchFilters(string newFilter)
    {
        searchFilters["Type"] = searchFilters["Theme"] = searchFilters["Function"] = searchFilters["Definition"] = searchFilters["Batch"] = searchFilters["Property"] = newFilter;
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

    private string CalculatePrefabDimensions(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.Log("预制体中没有找到任何模型");
            return string.Empty;
        }

        Bounds overallBounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            overallBounds.Encapsulate(renderer.bounds);
        }

        float length = Mathf.Round(overallBounds.size.x * 10) / 10f;
        float width = Mathf.Round(overallBounds.size.z * 10) / 10f;
        float height = Mathf.Round(overallBounds.size.y * 10) / 10f;

        return $"{length}|{width}|{height}";
    }
}

#endif
