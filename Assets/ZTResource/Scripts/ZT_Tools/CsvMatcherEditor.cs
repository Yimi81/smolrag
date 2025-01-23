#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 通过项目已用资源记录表，生成新CSV，方便添加到购物记录里。
/// </summary>

public class CsvMatcherEditor : EditorWindow
{
    private Object extractedResourcesFile;
    private Object resourceIndexLibraryFile;
    private string outputPath = "Assets/MatchedResources.csv";

    [MenuItem("ZTResource/Tools/已用资源合并到购物记录", false, 8)]
    public static void ShowWindow()
    {
        GetWindow<CsvMatcherEditor>("CSV Matcher");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV Matcher Settings", EditorStyles.boldLabel);

        extractedResourcesFile = EditorGUILayout.ObjectField("Extracted Resources CSV", extractedResourcesFile, typeof(Object), false);
        resourceIndexLibraryFile = EditorGUILayout.ObjectField("Resource Index Library CSV", resourceIndexLibraryFile, typeof(Object), false);
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        if (GUILayout.Button("Match CSVs"))
        {
            MatchCSVs();
        }
    }

    private void MatchCSVs()
    {
        if (extractedResourcesFile == null || resourceIndexLibraryFile == null)
        {
            Debug.LogError("请指定有效的CSV文件。");
            return;
        }

        string extractedResourcesPath = AssetDatabase.GetAssetPath(extractedResourcesFile);
        string resourceIndexLibraryPath = AssetDatabase.GetAssetPath(resourceIndexLibraryFile);

        var extractedResources = ReadCsv(extractedResourcesPath);
        var resourceIndexLibrary = ReadCsv(resourceIndexLibraryPath);

        var matchedResources = new List<string[]>();

        foreach (var extracted in extractedResources)
        {
            var matchingRow = resourceIndexLibrary.FirstOrDefault(ri => ri[0].Contains(extracted[0])); // 使用Contains进行部分匹配
            if (matchingRow != null)
            {
                matchedResources.Add(matchingRow);
            }
        }

        WriteCsv(outputPath, matchedResources);
        Debug.Log("匹配完成，结果已保存至 " + outputPath);
    }

    private List<string[]> ReadCsv(string path)
    {
        var lines = File.ReadAllLines(path);
        return lines.Select(line => line.Split(',')).ToList();
    }

    private void WriteCsv(string path, List<string[]> records)
    {
        var lines = records.Select(record => string.Join(",", record)).ToList();
        File.WriteAllLines(path, lines, Encoding.UTF8);
    }
}
#endif
