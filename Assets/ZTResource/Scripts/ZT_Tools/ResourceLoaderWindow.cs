#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ResourceLoaderWindow : EditorWindow
{
    string prefabPaths = ""; // 存储多个预制体路径
    Vector2 scrollPosition; // 用于滚动视图

    // 添加菜单项用于打开窗口
    [MenuItem("ZTResource/资源导入场景", false, 3)]
    public static void ShowWindow()
    {
        // 显示现有窗口实例。如果没有，就创建一个。
        EditorWindow.GetWindow(typeof(ResourceLoaderWindow), true, "资源加载");
    }

    void OnGUI()
    {
        // 界面布局开始
        GUILayout.Label("粘贴预制体路径 (每行一个):", EditorStyles.boldLabel);

        // 创建一个滚动视图
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        // 使用TextArea接收多行输入
        prefabPaths = EditorGUILayout.TextArea(prefabPaths, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        // 载入按钮
        if (GUILayout.Button("加载资源"))
        {
            LoadPrefabsIntoScene(prefabPaths);
        }
    }

    // 加载多个预制体到场景中的函数
    void LoadPrefabsIntoScene(string paths)
    {
        // 分割输入的多个路径
        string[] allPaths = paths.Split('\n');
        List<GameObject> instantiatedPrefabs = new List<GameObject>();

        foreach (string path in allPaths)
        {
            // 清除空白字符
            string trimmedPath = path.Trim();
            if (!string.IsNullOrEmpty(trimmedPath))
            {
                // 加载并实例化预制体
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(trimmedPath);
                if (prefab != null)
                {
                    GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    instantiatedPrefabs.Add(instantiatedPrefab); // 添加到列表
                    Debug.Log("预制体加载成功：" + trimmedPath);
                }
                else
                {
                    Debug.LogError("无法加载预制体，检查路径是否正确：" + trimmedPath);
                }
            }
        }

        // 选中所有实例化的预制体
        if (instantiatedPrefabs.Count > 0)
        {
            Selection.objects = instantiatedPrefabs.ToArray();
        }
    }
}
#endif