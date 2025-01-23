#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class CsvDuplicateRemover : ScriptableObject
{
    private static string rilFilePath = "Assets/ZTResource/Resources/ZT_TagLibrary/ResourceIndexLibrary.csv"; // RIL的文件路径

    [MenuItem("ZTResource/Tools/CSV查重-资源库", false, 6)]
    public static void RemoveDuplicateEntries()
    {
        try
        {
            // 读取CSV文件内容
            string[] lines = File.ReadAllLines(rilFilePath, Encoding.UTF8);
            if (lines.Length == 0)
            {
                Debug.LogWarning("CSV文件为空。");
                return;
            }

            // 创建一个字典来存储唯一的资源条目
            Dictionary<string, string> uniqueEntries = new Dictionary<string, string>();
            List<string> headers = new List<string>();

            // 遍历CSV文件内容
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    // 保留表头
                    headers.Add(lines[i]);
                    continue;
                }

                string[] columns = lines[i].Split(',');
                string resourceId = columns[0].Trim('\"');

                // 如果字典中不包含该资源ID，则添加
                if (!uniqueEntries.ContainsKey(resourceId))
                {
                    uniqueEntries.Add(resourceId, lines[i]);
                }
            }

            // 重新生成CSV内容
            StringBuilder csvContentBuilder = new StringBuilder();
            foreach (var header in headers)
            {
                csvContentBuilder.AppendLine(header);
            }
            foreach (var entry in uniqueEntries.Values)
            {
                csvContentBuilder.AppendLine(entry);
            }

            // 写回CSV文件
            File.WriteAllText(rilFilePath, csvContentBuilder.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("查重处理", "CSV查重处理已完成。", "确定");
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

    private static bool IsFileLocked(IOException exception)
    {
        int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(exception) & ((1 << 16) - 1);
        return errorCode == 32 || errorCode == 33;
    }
}

#endif
