#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class ResourceLibraryUpdater : EditorWindow
{
    private List<string> resourcesFolderPaths = new List<string>(); // 资源路径列表
    private string rilFilePath = "Assets/ZTResource/Resources/ZT_TagLibrary/ResourceIndexLibrary.csv"; // RIL的文件路径
    private Vector2 scrollPosition; // 用于滚动视图
    private bool isCancelled = false;
    private static string defaultResourcesFolderPath = "Assets/ArtResource/Scenes"; // 默认资源路径

    [MenuItem("ZTResource/更新-资源库", false, 1)]
    public static void ShowWindow()
    {
        GetWindow<ResourceLibraryUpdater>("更新资源库");
    }

    void OnGUI()
    {
        GUILayout.Label("RIL文件路径", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        rilFilePath = EditorGUILayout.TextField(rilFilePath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFilePanel("选择RIL文件", "", "ResourceIndexLibrary.csv", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                rilFilePath = "Assets" + path.Substring(Application.dataPath.Length); // 转换为相对路径
            }
        }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("更新全部"))
        {
            UpdateResourceIndexLibrary(new List<string> { defaultResourcesFolderPath });
        }
    }

    public void UpdateResourceIndexLibrary(List<string> folderPaths)
    {
        
        File.WriteAllText(rilFilePath, string.Empty);// 清空 CSV 文件

        Dictionary<string, string> existingEntries = new Dictionary<string, string>();
        bool isFileExists = File.Exists(rilFilePath);

        if (isFileExists)
        {
            string[] existingLines = File.ReadAllLines(rilFilePath);
            foreach (string line in existingLines)
            {
                string[] columns = line.Split(',');
                if (columns.Length > 0 && columns[0] != "资源ID")
                {
                    string id = columns[0].Trim('"');
                    existingEntries[id] = line;
                }
            }
        }

        StringBuilder csvContentBuilder = new StringBuilder();
        csvContentBuilder.AppendLine("资源ID,资源名称,资源描述,长宽高,预制体路径,缩略图路径,面数,创建时间,更新时间,版本,类型标签,主题标签,功能标签,区域标签,批次标签,属性标签");

        int totalCount = 0;
        foreach (var path in folderPaths)
        {
            totalCount += Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories).Length;
        }

        int currentIndex = 0;
        isCancelled = false;

        foreach (var folderPath in folderPaths)
        {
            var prefabPaths = Directory.GetFiles(folderPath, "*.prefab", SearchOption.AllDirectories);

            foreach (string prefabPath in prefabPaths)
            {
                if (isCancelled)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogWarning("更新已被用户取消。");
                    return;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    ResourceInfo info = prefab.GetComponent<ResourceInfo>();
                    if (info != null)
                    {
                        string csvRow = ConvertResourceInfoToCsvRow(info, prefabPath);

                        if (existingEntries.ContainsKey(info.id))
                        {
                            existingEntries[info.id] = csvRow;
                        }
                        else
                        {
                            existingEntries.Add(info.id, csvRow);
                        }
                    }
                }

                if (EditorUtility.DisplayCancelableProgressBar("资源库更新", $"正在更新：{prefab?.name}", (float)currentIndex / totalCount))
                {
                    isCancelled = true;
                }

                currentIndex++;
            }
        }

        EditorUtility.ClearProgressBar();

        if (!isCancelled)
        {
            try
            {
                foreach (var entry in existingEntries)
                {
                    csvContentBuilder.AppendLine(entry.Value);
                }

                File.WriteAllText(rilFilePath, csvContentBuilder.ToString(), Encoding.UTF8);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("资源库更新", "RIL更新已完成。", "确定");
            }
            catch (IOException ex)
            {
                if (IsFileLocked(ex))
                {
                    EditorUtility.DisplayDialog("错误", "无法更新资源库，文件可能为只读或已被打开，请关闭文件后重试。", "确定");
                }
                else
                {
                    throw;
                }
            }
        }
    }


    private static string ConvertResourceInfoToCsvRow(ResourceInfo info, string assetPath)
    {
        string thumbnailPath = info.thumbnailPath != null ? info.thumbnailPath : "";
        string modelFaces = info.modelFaces;
        string creationDate = info.creationDate;
        string updatedDate = info.updatedDate;
        string version = info.version;

        return string.Format(
            "\"{0}\",\"{1}\",\"{2}\",\"{3:F1}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\"",
            info.id,
            info.resourceName,
            info.resourceDescription,
            info.itemHeight,
            assetPath,
            thumbnailPath,  // 使用保存的缩略图路径
            modelFaces,
            creationDate,
            updatedDate,
            version,
            string.Join(";", info.typeTags),
            string.Join(";", info.themeTags),
            string.Join(";", info.functionTags),
            string.Join(";", info.definitionTags),
            string.Join(";", info.batchTags),
            string.Join(";", info.propertyTags)
        );
    }


    private static bool IsFileLocked(IOException exception)
    {
        int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(exception) & ((1 << 16) - 1);
        return errorCode == 32 || errorCode == 33;
    }
}

#endif