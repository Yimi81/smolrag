#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 用于管理和添加ResourceInfo脚本的编辑器窗口
/// </summary>
public class AddResourceInfo : EditorWindow
{
    private List<string> directories = new List<string>();
    private static List<GameObject> objectsWithoutResourceInfo = new List<GameObject>();

    [MenuItem("ZTResource/ResourceInfo批处理", false, 5)]
    static void Init()
    {
        AddResourceInfo window = (AddResourceInfo)EditorWindow.GetWindow(typeof(AddResourceInfo));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("ResourceInfo批处理", EditorStyles.boldLabel);

        GUILayout.Space(10);

        HandleDragAndDrop();

        if (directories.Count > 0)
        {
            GUILayout.Label("已选择的目录：");
            DisplayPathsWithRemoveButton(directories);
        }

        if (GUILayout.Button("选择未添加ResourceInfo的预制体"))
        {
            SelectObjectsWithoutResourceInfo();
        }

        if (GUILayout.Button("添加ResourceInfo"))
        {
            AddResourceInfoScript();
        }

        GUILayout.Space(10);

        GUIStyle highlightStyle = new GUIStyle(GUI.skin.button);
        highlightStyle.normal.textColor = Color.white;
        highlightStyle.normal.background = MakeTex(2, 2, Color.red);

        if (objectsWithoutResourceInfo.Count > 0 && GUILayout.Button("重新选择上次的预制体", highlightStyle))
        {
            ReSelectPreviousObjects();
        }
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "拖拽文件夹或Prefab到这里");

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

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(draggedObject);

                        // 判断是否是文件夹
                        if (Directory.Exists(assetPath))
                        {
                            directories.Add(assetPath);
                        }
                        // 判断是否是Prefab
                        else if (draggedObject is GameObject)
                        {
                            directories.Add(assetPath); // 直接添加Prefab的路径
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

    void SelectObjectsWithoutResourceInfo()
    {
        objectsWithoutResourceInfo.Clear();

        foreach (string directory in directories)
        {
            // 如果是文件夹，查找其中的Prefab
            if (Directory.Exists(directory))
            {
                string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { directory });
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                    if (obj != null && obj.GetComponent<ResourceInfo>() == null)
                    {
                        objectsWithoutResourceInfo.Add(obj);
                    }
                }
            }
            // 如果是Prefab，直接处理
            else if (File.Exists(directory) && directory.EndsWith(".prefab"))
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(directory);
                if (obj != null && obj.GetComponent<ResourceInfo>() == null)
                {
                    objectsWithoutResourceInfo.Add(obj);
                }
            }
        }

        if (objectsWithoutResourceInfo.Count > 0)
        {
            Selection.objects = objectsWithoutResourceInfo.ToArray();
            Debug.Log($"选择了 {objectsWithoutResourceInfo.Count} 个未添加 ResourceInfo 脚本的预制体。");
        }
        else
        {
            Debug.Log("没有找到未添加 ResourceInfo 脚本的预制体。");
        }
    }


    void ReSelectPreviousObjects()
    {
        if (objectsWithoutResourceInfo.Count > 0)
        {
            Selection.objects = objectsWithoutResourceInfo.ToArray();
            Debug.Log("重新选择了上次的预制体。");
        }
        else
        {
            Debug.Log("没有上次的预制体记录。");
        }
    }

    void AddResourceInfoScript()
    {
        int addedCount = 0;
        int alreadyExistCount = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            ResourceInfo resourceInfo = obj.GetComponent<ResourceInfo>();
            if (resourceInfo == null)
            {
                resourceInfo = obj.AddComponent<ResourceInfo>();

                // 使用路径生成ID，替换原有的GUID生成方式
                string prefabPath = AssetDatabase.GetAssetPath(obj);
                string basePath = "Assets/ArtResource/Scenes/Standard/";

                if (prefabPath.StartsWith(basePath))
                {
                    prefabPath = prefabPath.Substring(basePath.Length);
                }

                string resourceId = System.IO.Path.ChangeExtension(prefabPath, null); // 去掉文件后缀作为ID
                resourceInfo.id = resourceId;
                resourceInfo.resourceName = "未打标签";
                resourceInfo.creationDate = System.DateTime.Now.ToString("yyyy/MM/dd");
                resourceInfo.version = "0"; // 版本从0开始

                // 按照 ResourceInfoEditor 的方式查找缩略图
                string modifiedPrefabPath = prefabPath.Replace("/", "");
                string prefabName = System.IO.Path.GetFileNameWithoutExtension(modifiedPrefabPath);

                // 构建缩略图路径
                string thumbnailPath = $"Assets/ZTResource/Resources/ZT_IconTextures/{prefabName}.png";
                Texture2D newThumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);

                if (newThumbnail)
                {
                    resourceInfo.resourceThumbnail = newThumbnail;
                    Debug.Log($"找到并设置了缩略图: {thumbnailPath}");
                }
                else
                {
                    Debug.LogWarning($"未找到缩略图文件：{thumbnailPath}");
                }

                RefreshData(resourceInfo);
                addedCount++;
                Debug.Log($"已添加 ResourceInfo 脚本到: {obj.name}");
            }
            else
            {
                alreadyExistCount++;
                Debug.LogWarning($"{obj.name} 已经存在 ResourceInfo 脚本");
            }
        }

        Debug.Log($"操作完成，共添加了 {addedCount} 个 ResourceInfo 脚本，{alreadyExistCount} 个对象已经存在 ResourceInfo 脚本。");
    }



    static void RefreshData(ResourceInfo resourceInfo)
    {
        if (resourceInfo != null && resourceInfo.gameObject != null)
        {
            string facesCount = CalculateModelFaces(resourceInfo.gameObject);
            resourceInfo.modelFaces = facesCount;

            string dimensions = CalculatePrefabDimensions(resourceInfo.gameObject);
            resourceInfo.itemHeight = dimensions;

            EditorUtility.SetDirty(resourceInfo);
            Debug.Log("更新信息已完成！");
        }
    }

    static string CalculateModelFaces(GameObject prefab)
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

    static string CalculatePrefabDimensions(GameObject prefab)
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

    void OnDestroy()
    {
        objectsWithoutResourceInfo.Clear();
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}

#endif
