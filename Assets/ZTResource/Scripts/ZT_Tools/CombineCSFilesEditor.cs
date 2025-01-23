#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

public class CombineCSFilesEditor : EditorWindow
{
    private List<string> filePaths = new List<string>();
    private HashSet<string> directories = new HashSet<string>();
    private string outputPath = "Assets/GPT/ZTResource.txt";
    private int selectedEncodingIndex = 0;
    private int maxCharsPerFile = 200000;
    private Vector2 scrollPosition;

    private static readonly Dictionary<string, Encoding> Encodings = new Dictionary<string, Encoding>
    {
        {"UTF-8 (no BOM)", new UTF8Encoding(false)},
        {"UTF-8 (with BOM)", new UTF8Encoding(true)},
        {"ASCII", Encoding.ASCII},
        {"Unicode", Encoding.Unicode},
        {"BigEndianUnicode", Encoding.BigEndianUnicode},
        {"UTF-32", Encoding.UTF32},
        {"GB2312", Encoding.GetEncoding("GB2312")},
        {"Default", Encoding.Default}
    };

    [MenuItem("ZTResource/Tools/其他/合并CS文件")]
    public static void ShowWindow()
    {
        GetWindow<CombineCSFilesEditor>("合并 CS 文件");
    }

    private void OnGUI()
    {
        GUILayout.Label("合并 CS 文件", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        HandleDragAndDrop();

        EditorGUILayout.Space();
        GUILayout.Label("已添加内容:");
        DisplayPathsWithRemoveButton(directories.ToList(), directories, "目录");
        DisplayPathsWithRemoveButton(filePaths, null, "文件");

        EditorGUILayout.Space();
        GUILayout.Label("输出设置:", EditorStyles.boldLabel);
        outputPath = EditorGUILayout.TextField("输出路径:", outputPath).Replace('\\', '/');
        selectedEncodingIndex = EditorGUILayout.Popup("文件编码:", selectedEncodingIndex, Encodings.Keys.ToArray());
        maxCharsPerFile = EditorGUILayout.IntField("单个文件最大字符数:", maxCharsPerFile);

        EditorGUILayout.Space();
        if (GUILayout.Button("生成合并文件", GUILayout.Height(30)))
        {
            CombineFiles(outputPath, Encodings.Keys.ToArray()[selectedEncodingIndex]);
        }

        EditorGUILayout.EndScrollView();
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "拖拽文件夹或文件到这里", EditorStyles.helpBox);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition)) return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (string path in DragAndDrop.paths)
                    {
                        string normalizedPath = path.Replace('\\', '/');
                        if (Directory.Exists(normalizedPath))
                        {
                            directories.Add(normalizedPath);
                        }
                        else if (File.Exists(normalizedPath) && Path.GetExtension(normalizedPath).Equals(".cs", StringComparison.OrdinalIgnoreCase))
                        {
                            filePaths.Add(normalizedPath);
                        }
                    }
                }
                Event.current.Use();
                break;
        }
    }

    private void CombineFiles(string output, string encodingName)
    {
        try
        {
            output = output.Replace('\\', '/');
            List<string> allFiles = new List<string>(filePaths);

            foreach (string directory in directories)
            {
                allFiles.AddRange(Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories)
                    .Select(f => f.Replace('\\', '/'))
                    .Where(f => !Path.GetFileName(f).StartsWith("._")));
            }

            if (allFiles.Count == 0)
            {
                Debug.LogError("未选择任何文件");
                return;
            }

            Encoding encoding = Encodings[encodingName];
            string directoryPath = Path.GetDirectoryName(output).Replace('\\', '/');
            string fileName = Path.GetFileNameWithoutExtension(output);
            string extension = Path.GetExtension(output);

            int fileIndex = 1;
            StringBuilder contentBuilder = new StringBuilder();
            int totalProcessed = 0;

            foreach (string file in allFiles)
            {
                string relativePath = GetRelativeProjectPath(file);
                string content = $"// ==========================\n" +
                                $"// Path: {relativePath}\n" +
                                $"// ==========================\n\n" +
                                $"{ReadFileWithEncoding(file, encoding)}\n\n";
                content = NormalizeLineEndings(content, Environment.NewLine);

                if (content.Length > maxCharsPerFile * 0.5f)
                {
                    if (contentBuilder.Length > 0)
                    {
                        WriteFile(directoryPath, fileName, extension, fileIndex++, contentBuilder.ToString(), encoding);
                        contentBuilder.Clear();
                    }
                    WriteFile(directoryPath, fileName, extension, fileIndex++, content, encoding);
                    continue;
                }

                if (contentBuilder.Length + content.Length > maxCharsPerFile)
                {
                    WriteFile(directoryPath, fileName, extension, fileIndex++, contentBuilder.ToString(), encoding);
                    contentBuilder.Clear();
                }

                contentBuilder.Append(content);
                totalProcessed++;

                EditorUtility.DisplayProgressBar("合并进度",
                    $"正在处理文件 ({totalProcessed}/{allFiles.Count})",
                    (float)totalProcessed / allFiles.Count);
            }

            if (contentBuilder.Length > 0)
                WriteFile(directoryPath, fileName, extension, fileIndex, contentBuilder.ToString(), encoding);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            Debug.Log($"合并完成！共生成 {fileIndex} 个文件");
        }
        catch (Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"合并失败: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void WriteFile(string directory, string baseName, string extension, int index, string content, Encoding encoding)
    {
        string path = Path.Combine(directory, $"{baseName}_{index}{extension}").Replace('\\', '/');
        File.WriteAllText(path, content, encoding);
        Debug.Log($"文件已生成：{GetRelativeProjectPath(path)} ({content.Length} 字符)");
    }

    private string ReadFileWithEncoding(string path, Encoding encoding)
    {
        return File.ReadAllText(path, encoding);
    }

    private string NormalizeLineEndings(string text, string newLine)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", newLine);
    }

    private string GetRelativeProjectPath(string absolutePath)
    {
        string projectPath = Application.dataPath.Replace("/Assets", "").Replace('\\', '/');
        absolutePath = absolutePath.Replace('\\', '/');

        if (absolutePath.StartsWith(projectPath))
        {
            return absolutePath.Substring(projectPath.Length + 1);
        }
        return absolutePath;
    }

    private void DisplayPathsWithRemoveButton(List<string> paths, HashSet<string> pathSet, string label)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label($"{label} ({paths.Count}):");

        for (int i = 0; i < paths.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GetRelativeProjectPath(paths[i]), EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                if (pathSet != null) pathSet.Remove(paths[i]);
                else filePaths.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}
#endif