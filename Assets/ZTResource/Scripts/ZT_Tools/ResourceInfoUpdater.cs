#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 给所有携带ResourceInfo的物体，整体更新一次长宽高计算，并提供复制、粘贴、清除标签的功能
/// </summary>
public class ResourceInfoUpdater : EditorWindow
{
    private HashSet<string> directories = new HashSet<string>(); // 存储拖拽的文件夹路径
    private HashSet<string> specificPrefabPaths = new HashSet<string>(); // 存储直接粘贴的预制体路径
    private string pastedPathsText = string.Empty; // 存储粘贴区域的文本
    private Vector2 scrollPosition; // 滚动视图的位置
    // 用于保存复制的标签
    private Dictionary<string, List<string>> copiedTags = new Dictionary<string, List<string>>();

    [MenuItem("ZTResource/Tools/更新-长宽高", false, 9)]
    public static void ShowWindow()
    {
        GetWindow<ResourceInfoUpdater>("ResourceInfo编辑工具");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition); // 开始滚动视图

        GUILayout.Label("更新所有预制体信息", EditorStyles.boldLabel);

        // 显示拖拽区域
        GUILayout.Label("拖拽文件夹到这里:");
        HandleDragAndDrop();

        // 显示当前已添加的文件夹
        GUILayout.Label("已添加的文件夹:");
        DisplayPathsWithRemoveButton(directories.ToList(), directories);

        GUILayout.Space(10);

        // 新增的粘贴区域
        GUILayout.Label("直接粘贴预制体路径:");
        pastedPathsText = EditorGUILayout.TextArea(pastedPathsText, GUILayout.Height(60));
        if (GUILayout.Button("添加粘贴的路径"))
        {
            AddPastedPaths();
        }

        // 显示已添加的直接粘贴的预制体路径
        if (specificPrefabPaths.Count > 0)
        {
            GUILayout.Label("已添加的预制体路径:");
            DisplayPathsWithRemoveButton(specificPrefabPaths.ToList(), specificPrefabPaths);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("ALL更新信息"))
        {
            UpdateAllResourceInfo();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("复制标签"))
        {
            CopyTagsFromSelected();
        }

        if (GUILayout.Button("粘贴标签"))
        {
            PasteTagsToSelected();
        }

        if (GUILayout.Button("清除所有标签"))
        {
            ClearAllTagsFromSelected();
        }

        EditorGUILayout.EndScrollView(); // 结束滚动视图
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "拖拽文件夹或预制体到这里");

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
                        if (Directory.Exists(draggedObject))
                        {
                            // 如果是文件夹，添加文件夹路径
                            directories.Add(draggedObject);
                        }
                        else if (draggedObject.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                        {
                            // 如果是预制体，添加预制体路径
                            specificPrefabPaths.Add(draggedObject);
                        }
                    }
                }
                Event.current.Use();
                break;
        }
    }

    private void AddPastedPaths()
    {
        if (string.IsNullOrWhiteSpace(pastedPathsText))
        {
            Debug.LogWarning("粘贴区域为空！");
            return;
        }

        string[] paths = pastedPathsText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        int addedCount = 0;

        foreach (string path in paths)
        {
            string trimmedPath = path.Trim();
            if (trimmedPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(trimmedPath) != null)
                {
                    if (specificPrefabPaths.Add(trimmedPath))
                    {
                        addedCount++;
                    }
                }
                else
                {
                    Debug.LogWarning($"无效的预制体路径：{trimmedPath}");
                }
            }
            else
            {
                Debug.LogWarning($"路径不是预制体文件：{trimmedPath}");
            }
        }

        pastedPathsText = string.Empty; // 清空粘贴区域
        if (addedCount > 0)
        {
            Debug.Log($"成功添加了 {addedCount} 个预制体路径！");
        }
        else
        {
            Debug.LogWarning("没有有效的预制体路径被添加！");
        }
    }

    private void UpdateAllResourceInfoDimensions()
    {
        List<string> allFiles = GetAllPrefabPaths();
        int updatedCount = 0;

        foreach (string path in allFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            ResourceInfo resourceInfo = prefab.GetComponent<ResourceInfo>();
            if (resourceInfo != null)
            {
                string dimensions = CalculatePrefabDimensions(prefab);
                resourceInfo.itemHeight = dimensions;

                EditorUtility.SetDirty(resourceInfo);
                updatedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"更新了 {updatedCount} 个预制体的尺寸。");
    }

    private void UpdateAllResourceInfo()
    {
        List<string> allFiles = GetAllPrefabPaths();
        int updatedCount = 0;

        foreach (string path in allFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            ResourceInfo resourceInfo = prefab.GetComponent<ResourceInfo>();
            if (resourceInfo != null)
            {
                RefreshThumbnail(resourceInfo);
                RefreshData(resourceInfo);

                EditorUtility.SetDirty(resourceInfo);
                updatedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"更新了 {updatedCount} 个预制体的信息。");
    }


    private List<string> GetAllPrefabPaths()
    {
        List<string> allFiles = new List<string>();
        foreach (string directory in directories)
        {
            allFiles.AddRange(Directory.GetFiles(directory, "*.prefab", SearchOption.AllDirectories));
        }

        allFiles.AddRange(specificPrefabPaths);

        // 去重
        return allFiles.Distinct().ToList();
    }

    private void RefreshData(ResourceInfo resourceInfo)
    {
        if (resourceInfo != null && resourceInfo.gameObject != null)
        {
            // 获取预制体的路径并去除公共部分和文件后缀
            string prefabPath = AssetDatabase.GetAssetPath(resourceInfo.gameObject);
            string basePath = "Assets/ArtResource/Scenes/Standard/";

            // 去掉公共部分路径
            if (prefabPath.StartsWith(basePath))
            {
                prefabPath = prefabPath.Substring(basePath.Length);
            }

            // 去除文件后缀
            string prefabPathWithoutExtension = Path.ChangeExtension(prefabPath, null);
            resourceInfo.id = prefabPathWithoutExtension;

            string facesCount = CalculateModelFaces(resourceInfo.gameObject);
            resourceInfo.modelFaces = facesCount;

            string dimensions = CalculatePrefabDimensions(resourceInfo.gameObject);
            resourceInfo.itemHeight = dimensions;
        }
    }

    private void RefreshThumbnail(ResourceInfo resourceInfo)
    {
        if (resourceInfo == null)
        {
            Debug.LogWarning("ResourceInfo 脚本未找到！");
            return;
        }

        // 获取预制体的路径
        string prefabPath = AssetDatabase.GetAssetPath(resourceInfo.gameObject);
        if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab"))
        {
            Debug.LogWarning("无法找到预制体的路径，或者选择的不是一个预制体！");
            return;
        }

        // 去除公共部分路径
        string basePath = "Assets/ArtResource/Scenes/Standard/";
        if (prefabPath.StartsWith(basePath))
        {
            prefabPath = prefabPath.Substring(basePath.Length);
        }

        // 删除路径中的“/”符号
        string modifiedPrefabPath = prefabPath.Replace("/", "");

        // 去除文件后缀
        string prefabName = Path.GetFileNameWithoutExtension(modifiedPrefabPath);

        // 构建缩略图路径
        string thumbnailPath = $"Assets/ZTResource/Resources/ZT_IconTextures/{prefabName}.png";
        Texture2D newThumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);

        if (newThumbnail)
        {
            resourceInfo.thumbnailPath = prefabName;
            EditorUtility.SetDirty(resourceInfo);

            Debug.Log("缩略图已更新！");
        }
        else
        {
            Debug.LogWarning($"未找到缩略图文件：{thumbnailPath}");
        }
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

    private void DisplayPathsWithRemoveButton(List<string> paths, HashSet<string> pathSet)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(paths[i]);
            if (GUILayout.Button("移除", GUILayout.Width(60)))
            {
                pathSet.Remove(paths[i]);
            }
            GUILayout.EndHorizontal();
        }
    }

    // 复制标签
    private void CopyTagsFromSelected()
    {
        copiedTags.Clear();

        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("没有选中的预制体！");
            return;
        }

        ResourceInfo resourceInfo = selectedObjects[0].GetComponent<ResourceInfo>();
        if (resourceInfo == null)
        {
            Debug.LogWarning("选中的对象没有ResourceInfo组件！");
            return;
        }

        copiedTags["TypeTags"] = new List<string>(resourceInfo.typeTags);
        copiedTags["ThemeTags"] = new List<string>(resourceInfo.themeTags);
        copiedTags["DefinitionTags"] = new List<string>(resourceInfo.definitionTags);
        copiedTags["FunctionTags"] = new List<string>(resourceInfo.functionTags);
        copiedTags["BatchTags"] = new List<string>(resourceInfo.batchTags);
        copiedTags["PropertyTags"] = new List<string>(resourceInfo.propertyTags);

        Debug.Log("标签已复制！");
    }

    // 粘贴标签
    private void PasteTagsToSelected()
    {
        if (copiedTags.Count == 0)
        {
            Debug.LogWarning("没有复制的标签可粘贴！");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("没有选中的预制体！");
            return;
        }

        foreach (GameObject selectedObject in selectedObjects)
        {
            ResourceInfo resourceInfo = selectedObject.GetComponent<ResourceInfo>();
            if (resourceInfo == null)
            {
                Debug.LogWarning($"选中的对象 {selectedObject.name} 没有ResourceInfo组件！");
                continue;
            }

            PasteTagsToList(ref resourceInfo.typeTags, "TypeTags");
            PasteTagsToList(ref resourceInfo.themeTags, "ThemeTags");
            PasteTagsToList(ref resourceInfo.definitionTags, "DefinitionTags");
            PasteTagsToList(ref resourceInfo.functionTags, "FunctionTags");
            PasteTagsToList(ref resourceInfo.batchTags, "BatchTags");
            PasteTagsToList(ref resourceInfo.propertyTags, "PropertyTags");

            EditorUtility.SetDirty(resourceInfo);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("标签已粘贴到选中的所有预制体！");
    }

    // 清除所有标签
    private void ClearAllTagsFromSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("没有选中的预制体！");
            return;
        }

        foreach (GameObject selectedObject in selectedObjects)
        {
            ResourceInfo resourceInfo = selectedObject.GetComponent<ResourceInfo>();
            if (resourceInfo == null)
            {
                Debug.LogWarning($"选中的对象 {selectedObject.name} 没有ResourceInfo组件！");
                continue;
            }

            ClearTagsList(ref resourceInfo.typeTags);
            ClearTagsList(ref resourceInfo.themeTags);
            ClearTagsList(ref resourceInfo.definitionTags);
            ClearTagsList(ref resourceInfo.functionTags);
            ClearTagsList(ref resourceInfo.batchTags);
            ClearTagsList(ref resourceInfo.propertyTags);

            EditorUtility.SetDirty(resourceInfo);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("已清除选中预制体的所有标签！");
    }

    private void PasteTagsToList(ref List<string> tags, string tagType)
    {
        if (tags == null)
        {
            tags = new List<string>();
        }

        tags.Clear();
        if (copiedTags.ContainsKey(tagType))
        {
            tags.AddRange(copiedTags[tagType]);
        }
    }

    private void ClearTagsList(ref List<string> tags)
    {
        if (tags != null)
        {
            tags.Clear();
        }
    }
}

#endif
