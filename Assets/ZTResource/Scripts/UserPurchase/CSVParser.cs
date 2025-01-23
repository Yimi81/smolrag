using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVParser : MonoBehaviour
{
    public static (string userName, string userAvatar, List<(string batchName, string batchTime, List<CardInfo> cardInfos)> batches) ParseCSV(string filePath)
    {
        List<(string batchName, string batchTime, List<CardInfo> cardInfos)> batchList = new List<(string batchName, string batchTime, List<CardInfo> cardInfos)>();
        string userName = string.Empty;
        string userAvatar = string.Empty;

        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found: " + filePath);
            return (userName, userAvatar, batchList);
        }

        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

        // 解析用户名称和用户头像
        if (lines.Length > 1)
        {
            string[] userInfo = lines[1].Split(',');
            if (userInfo.Length >= 2)
            {
                userName = userInfo[0];
                userAvatar = userInfo[1];
            }
        }

        string currentBatchName = string.Empty;
        string currentBatchTime = string.Empty;
        List<CardInfo> currentBatchCards = new List<CardInfo>();

        for (int i = 3; i < lines.Length; i++)
        {
            string[] data = lines[i].Split(',');

            if (data[0].StartsWith("批次"))
            {
                // 如果已有当前批次信息，添加到列表中
                if (!string.IsNullOrEmpty(currentBatchName) || !string.IsNullOrEmpty(currentBatchTime) || currentBatchCards.Count > 0)
                {
                    batchList.Add((currentBatchName, currentBatchTime, currentBatchCards));
                }

                if (data.Length > 2)
                {
                    currentBatchName = data[1]; // 批次名称在"批次"后面一格，允许为空
                    currentBatchTime = data[2]; // 批次时间在第三格
                }
                currentBatchCards = new List<CardInfo>();
            }
            else if (data.Length >= 16)
            {
                CardInfo card = new CardInfo(
                    data[0], // ID
                    data[1], // Name
                    data[2], // Description
                    data[3], // Height
                    data[4], // PrefabPath
                    data[5], // ThumbnailPath
                    data[6], // ModelFaces
                    data[7], // CreationDate
                    data[8], // UpdatedDate
                    data[9], // Version
                    data[10], // TypeTags
                    data[11], // ThemeTags
                    data[12], // FunctionTags
                    data[13], // DefinitionTags
                    data[14], // BatchTags
                    data[15]  // PropertyTags
                );

                currentBatchCards.Add(card);
            }
            else
            {
                Debug.LogWarning("Incorrect data format in line: " + i);
            }
        }

        // 处理最后一个批次
        if (!string.IsNullOrEmpty(currentBatchName) || !string.IsNullOrEmpty(currentBatchTime) || currentBatchCards.Count > 0)
        {
            batchList.Add((currentBatchName, currentBatchTime, currentBatchCards));
        }

        return (userName, userAvatar, batchList);
    }



    public static void DeleteResource(string filePath, string resourceId)
    {
        List<string> lines = new List<string>(File.ReadAllLines(filePath, Encoding.UTF8));
        lines.RemoveAll(line => line.Contains(resourceId));
        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    public static void DeleteBatch(string filePath, string batchTime)
    {
        List<string> lines = new List<string>(File.ReadAllLines(filePath, Encoding.UTF8));
        bool inBatch = false;
        bool batchFound = false;

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith("批次") && lines[i].Contains(batchTime))
            {
                inBatch = true;
                batchFound = true;
            }

            if (inBatch)
            {
                lines.RemoveAt(i);
                i--; // 调整索引以便正确删除连续的行

                // 检查是否是下一个批次的开始，如果是则停止删除
                if (i + 1 < lines.Count && lines[i + 1].StartsWith("批次"))
                {
                    inBatch = false;
                }
            }
        }

        if (batchFound)
        {
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }
        else
        {
            Debug.LogWarning("Batch not found: " + batchTime);
        }
    }


    public static void RenameBatch(string filePath, string oldBatchTime, string newBatchName)
    {
        List<string> lines = new List<string>(File.ReadAllLines(filePath, Encoding.UTF8));

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith("批次") && lines[i].Contains(oldBatchTime))
            {
                string[] parts = lines[i].Split(',');
                if (parts.Length > 2 && parts[2] == oldBatchTime)
                {
                    parts[1] = newBatchName;
                    lines[i] = string.Join(",", parts);
                }
            }
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }
    public static bool ResourceExistsInBatch(string filePath, string resourceId, string targetBatchTime)
    {
        List<string> lines = new List<string>(File.ReadAllLines(filePath, Encoding.UTF8));
        bool inTargetBatch = false;

        foreach (string line in lines)
        {
            if (line.StartsWith("批次") && line.Contains(targetBatchTime))
            {
                inTargetBatch = true;
            }
            else if (line.StartsWith("批次"))
            {
                inTargetBatch = false;
            }

            if (inTargetBatch && line.Contains(resourceId))
            {
                return true;
            }
        }

        return false;
    }
    public static void DeleteResourceInBatch(string filePath, string resourceId, string batchTime)
    {
        List<string> lines = new List<string>(File.ReadAllLines(filePath, Encoding.UTF8));
        bool inTargetBatch = false;

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith("批次") && lines[i].Contains(batchTime))
            {
                inTargetBatch = true;
            }
            else if (lines[i].StartsWith("批次"))
            {
                inTargetBatch = false;
            }

            if (inTargetBatch && lines[i].Contains(resourceId))
            {
                lines.RemoveAt(i);
                break;
            }
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }
    public static void MoveResourceToBatch(string filePath, string resourceId, string targetBatchTime)
    {
        List<string> lines = new List<string>(File.ReadAllLines(filePath, Encoding.UTF8));
        string resourceLine = lines.Find(line => line.Contains(resourceId) && !line.StartsWith("批次"));
        lines.Remove(resourceLine);

        int targetBatchIndex = lines.FindIndex(line => line.StartsWith("批次") && line.Contains(targetBatchTime));
        if (targetBatchIndex != -1)
        {
            int insertIndex = targetBatchIndex + 1;
            while (insertIndex < lines.Count && !lines[insertIndex].StartsWith("批次"))
            {
                insertIndex++;
            }
            lines.Insert(insertIndex, resourceLine);
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

}