#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

public class ScriptEncodingConverter : EditorWindow
{
    private List<string> filePaths = new List<string>(); // 存储拖拽的文件路径
    private HashSet<string> directories = new HashSet<string>(); // 存储拖拽的文件夹路径

    [MenuItem("ZTResource/Tools/其他/**批量修改编码**")]
    public static void ShowWindow()
    {
        GetWindow<ScriptEncodingConverter>("Script Encoding Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量修改脚本编码", EditorStyles.boldLabel);

        // 显示拖拽区域
        GUILayout.Label("拖拽文件夹或文件到这里:");
        HandleDragAndDrop();

        // 显示当前已添加的文件夹和文件
        GUILayout.Label("已添加的文件夹:");
        DisplayPathsWithRemoveButton(directories.ToList(), directories);

        GUILayout.Label("已添加的文件:");
        DisplayPathsWithRemoveButton(filePaths, null);

        EditorGUILayout.Space();

        if (GUILayout.Button("转换为 UTF-8"))
        {
            if (EditorUtility.DisplayDialog("确认", "确定要将所有脚本文件转换为UTF-8编码吗？此操作将覆盖源文件。", "确定", "取消"))
            {
                ConvertFilesToUTF8();
            }
        }
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "拖拽文件夹或文件到这里");

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
                            directories.Add(draggedObject);
                        }
                        else if (File.Exists(draggedObject) && Path.GetExtension(draggedObject).Equals(".cs", StringComparison.OrdinalIgnoreCase))
                        {
                            filePaths.Add(draggedObject);
                        }
                    }
                }
                Event.current.Use();
                break;
        }
    }

    private void ConvertFilesToUTF8()
    {
        try
        {
            List<string> allFiles = new List<string>(filePaths);

            // 获取所有文件夹中的 .cs 文件
            foreach (string directory in directories)
            {
                allFiles.AddRange(Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories)
                                           .Where(f => !Path.GetFileName(f).StartsWith("._")));
            }

            if (allFiles.Count == 0)
            {
                Debug.LogError("未选择任何文件。");
                return;
            }

            // 使用 UTF-8 编码写入文件
            foreach (string file in allFiles)
            {
                string content = File.ReadAllText(file, Encoding.GetEncoding("GB2312"));
                File.WriteAllText(file, content, new UTF8Encoding(false)); // 不包含 BOM
                Debug.Log($"已转换文件: {file}");
            }

            AssetDatabase.Refresh();
            Debug.Log($"文件已转换为 UTF-8 编码。转换了 {allFiles.Count} 个 .cs 文件。");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("转换文件编码时出错: " + ex.Message);
        }
    }

    private void DisplayPathsWithRemoveButton(List<string> paths, HashSet<string> pathSet)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(paths[i]);
            if (GUILayout.Button("移除", GUILayout.Width(60)))
            {
                if (pathSet != null)
                {
                    pathSet.Remove(paths[i]);
                }
                else
                {
                    filePaths.RemoveAt(i);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif
