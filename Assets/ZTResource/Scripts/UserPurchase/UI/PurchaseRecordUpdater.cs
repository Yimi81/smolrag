#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using PartyIP.AssetExport;

/// <summary>
/// 购买记录里的更新逻辑，完成二次导出用的。放在了购物记录的导出按钮上
/// </summary>
public class PurchaseRecordUpdater : MonoBehaviour
{
    public Button savePurchaseRecordButton; // 插槽：用于保存购买记录的按钮
    public Transform contentParent; // 插槽：用于获取Content Parent中的资源卡
    public GameObject title; // 插槽：用于显示标题
    public ResourceManager resourceManager; // 插槽：用于获取ResourceManager实例

    private const string CsvHeader = "资源ID,资源名称,资源描述,物品高度,预制体路径,缩略图路径,面数,创建时间,更新时间,版本,类型标签,主题标签,功能标签,区域标签,批次标签,属性标签";
    private EditorWindow m_ExportWindow = null;
    private Type m_ExportWindowType = null;
    public GameObject pathAddFailedUI; // 插槽：用于显示路径添加失败消息的UI

    private void Start()
    {
        if (savePurchaseRecordButton != null)
        {
            savePurchaseRecordButton.onClick.AddListener(UpdatePurchaseRecord);
        }

        if (title != null)
        {
            title.SetActive(false); // 确保初始状态是隐藏的
        }

        // 初始化时打开AssetExportWindow（仅在编辑器中）
#if UNITY_EDITOR
        OpenAssetExportWindow();
#endif
    }

#if UNITY_EDITOR
    private void OpenAssetExportWindow()
    {
        EditorApplication.delayCall += () =>
        {
            m_ExportWindow = EditorWindow.GetWindow(GetExportWindowType());
        };
    }

    private Type GetExportWindowType()
    {
        if (m_ExportWindowType != null)
        {
            return m_ExportWindowType;
        }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Assembly target = null;
        foreach (var assembly in assemblies)
        {
            string name = assembly.FullName.ToLower().Replace("-", "").Replace(".", "");
            if (name.Contains("assetexport"))
            {
                target = assembly;
                break;
            }
        }

        if (target != null)
        {
            m_ExportWindowType = target.GetType("PartyIP.AssetExport.AssetExportWindow");
        }

        return m_ExportWindowType;
    }

    private bool AddPathToExportWindow(string path)
    {
        if (m_ExportWindow == null)
        {
            Debug.LogError("导出窗口没打开");
            return false;
        }

        var IAddPathFunc = m_ExportWindowType.GetMethod("AddPath");
        try
        {
            string error = (string)IAddPathFunc.Invoke((object)m_ExportWindow, new object[] { path, true, true });
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log("路径添加成功");
                return true;
            }
            else
            {
                Debug.LogError($"路径添加失败: {error}");
                ShowPathAddFailed(path); // 显示路径添加失败消息的UI
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"报错: {e.InnerException.Message}");
            ShowPathAddFailed(path); // 显示路径添加失败消息的UI
            return false;
        }
    }
#endif

    private void UpdatePurchaseRecord()
    {
        List<CardInfo> cardInfos = new List<CardInfo>();

        foreach (Transform child in contentParent)
        {
            SelectedCardUI cardUI = child.GetComponent<SelectedCardUI>();
            if (cardUI != null)
            {
                // 获取资源信息
                ResourceCardData resourceData = resourceManager.GetResourceById(cardUI.idText.text);
                if (resourceData != null)
                {
                    CardInfo card = new CardInfo(
                        resourceData.ID,
                        resourceData.Name,
                        resourceData.Description,
                        resourceData.Height,
                        resourceData.PrefabPath,
                        resourceData.ThumbnailPath,
                        resourceData.ModelFaces,
                        resourceData.CreationDate,
                        resourceData.UpdatedDate,
                        resourceData.Version,
                        string.Join(";", resourceData.TypeTags),
                        string.Join(";", resourceData.ThemeTags),
                        string.Join(";", resourceData.FunctionTags),
                        string.Join(";", resourceData.DefinitionTags),
                        string.Join(";", resourceData.BatchTags),
                        string.Join(";", resourceData.PropertyTags)
                    );
                    cardInfos.Add(card);
                }
            }
        }

        string userKey = PlayerPrefs.GetString("currentUserKey", string.Empty);
        if (!string.IsNullOrEmpty(userKey))
        {
            string folderPath = Path.Combine(Application.dataPath, "ZTResource/UserInfo");
            string fileName = userKey + ".csv";
            string filePath = Path.Combine(folderPath, fileName);

            UpdatePurchaseRecordFile(cardInfos, filePath);

            if (title != null)
            {
                title.SetActive(true); // 显示标题
                StartCoroutine(HideTitleAfterSeconds(3)); // 3秒后隐藏标题
            }

            // 强制刷新Unity的资源数据库（仅在编辑器中）
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            // 购买成功，调用CustomEventSystem.RaiseAddPrefabPath（仅在编辑器中）
#if UNITY_EDITOR
            foreach (var card in cardInfos)
            {
                if (!AddPathToExportWindow(card.PrefabPath))
                {
                    ShowPathAddFailed(card.PrefabPath); // 显示路径添加失败消息的UI
                    return;
                }
            }
#endif

            CopyAllPrefabPathsToClipboard(); // 复制路径到剪贴板
        }
    }

    private void UpdatePurchaseRecordFile(List<CardInfo> cardInfos, string filePath)
    {
        if (cardInfos == null || cardInfos.Count == 0) return;

        List<string> lines = new List<string>(File.ReadAllLines(filePath, Encoding.UTF8));
        Dictionary<string, string> updatedRecords = new Dictionary<string, string>();

        foreach (var card in cardInfos)
        {
            string line = $"{card.ID},{card.Name},{card.Description},{card.Height},{card.PrefabPath},{card.ThumbnailPath},{card.ModelFaces},{card.CreationDate},{card.UpdatedDate},{card.Version},{card.TypeTags},{card.ThemeTags},{card.FunctionTags},{card.DefinitionTags},{card.BatchTags},{card.PropertyTags}";
            updatedRecords[card.ID] = line;
        }

        for (int i = 3; i < lines.Count; i++) // 从第四行开始读取数据
        {
            string[] values = lines[i].Split(',');
            if (values.Length > 0 && updatedRecords.ContainsKey(values[0]))
            {
                lines[i] = updatedRecords[values[0]];
                updatedRecords.Remove(values[0]);
            }
        }

        // 添加新的记录
        foreach (var record in updatedRecords.Values)
        {
            lines.Add(record);
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    private void CopyAllPrefabPathsToClipboard()
    {
        StringBuilder paths = new StringBuilder();
        foreach (Transform child in contentParent)
        {
            SelectedCardUI cardUI = child.GetComponent<SelectedCardUI>();
            if (cardUI != null && cardUI.prefabPathText != null)
            {
                TextMeshProUGUI tmpText = cardUI.prefabPathText.GetComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    paths.AppendLine(tmpText.text);
                }
            }
        }
        GUIUtility.systemCopyBuffer = paths.ToString();
    }

    IEnumerator HideTitleAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (title != null)
        {
            title.SetActive(false);
        }
    }

    private void ShowPathAddFailed(string failedPath)
    {
        foreach (Transform child in contentParent)
        {
            SelectedCardUI cardUI = child.GetComponent<SelectedCardUI>();
            if (cardUI != null && cardUI.prefabPathText.text == failedPath)
            {
                cardUI.SetPathErrorUIVisibility(true); // 显示路径错误UI
            }
        }

        if (pathAddFailedUI != null)
        {
            pathAddFailedUI.SetActive(true); // 显示路径添加失败消息的UI
        }
    }
}
#endif