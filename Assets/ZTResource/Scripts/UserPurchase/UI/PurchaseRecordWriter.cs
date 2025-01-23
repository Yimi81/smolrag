using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using PartyIP.AssetExport;
#endif
using System;
using System.Reflection;

public class PurchaseRecordWriter : MonoBehaviour
{
    public Button savePurchaseRecordButton; // 插槽：用于保存购买记录的按钮
    public Transform contentParent; // 插槽：用于获取Content Parent中的资源卡
    public GameObject title; // 插槽：用于显示标题
    public GameObject purchaseFailedUI; // 插槽：用于显示购买失败消息的UI
    public GameObject pathAddFailedUI; // 插槽：用于显示路径添加失败消息的UI

    private const string CsvHeader = "资源ID,资源名称,资源描述,物品高度,预制体路径,缩略图路径,面数,创建时间,更新时间,版本,类型标签,主题标签,功能标签,区域标签,批次标签,属性标签";

    private void Start()
    {
        if (savePurchaseRecordButton != null)
        {
            savePurchaseRecordButton.onClick.AddListener(SavePurchaseRecord);
        }

        if (title != null)
        {
            title.SetActive(false); // 确保初始状态是隐藏的
        }

        if (purchaseFailedUI != null)
        {
            purchaseFailedUI.SetActive(false); // 确保初始状态是隐藏的
        }

        if (pathAddFailedUI != null)
        {
            pathAddFailedUI.SetActive(false); // 确保初始状态是隐藏的
        }

#if UNITY_EDITOR
        // 初始化时打开AssetExportWindow
        OpenAssetExportWindow();
        AssetExportCallbackRegistery.RegisterOnExportFinished(OnAssetExportFinished);
        AssetExportCallbackRegistery.RegisterOnExportFailed(OnAssetExportFailed);
#endif
    }

#if UNITY_EDITOR
    //导出工具返回值
    private void OnAssetExportFailed(string message)
    {
        Debug.Log("FAILED:" + message);
    }
    private void OnAssetExportFinished()
    {
        Debug.Log("FINISHED");
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
            if (assembly.FullName.ToLower().Contains("assetexport"))
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

    private EditorWindow m_ExportWindow = null;
    private Type m_ExportWindowType = null;

    private void OpenAssetExportWindow()
    {
        EditorApplication.delayCall += () =>
        {
            m_ExportWindow = EditorWindow.GetWindow(GetExportWindowType());
        };
    }
#endif

    private void SavePurchaseRecord()
    {
        List<CardInfo> cardInfos = new List<CardInfo>();
        bool hasDuplicate = false;

        foreach (Transform child in contentParent)
        {
            SelectedCardUI cardUI = child.GetComponent<SelectedCardUI>();
            if (cardUI != null)
            {
                // 获取缩略图名称
                string thumbnailName = string.Empty;
                if (cardUI.thumbnailImage.sprite != null)
                {
#if UNITY_EDITOR
                    // 假设缩略图路径存储在sprite.name中
                    string thumbnailPath = AssetDatabase.GetAssetPath(cardUI.thumbnailImage.sprite.texture);
                    thumbnailName = Path.GetFileNameWithoutExtension(thumbnailPath); // 获取文件名，不包含路径和扩展名
#endif
                }

                CardInfo card = new CardInfo(
                    cardUI.idText.text,
                    cardUI.nameText.text,
                    cardUI.descriptionText.text,
                    cardUI.heightText.text,
                    cardUI.prefabPathText.text,
                    thumbnailName,
                    cardUI.modelFacesText.text,
                    cardUI.creationDateText.text,
                    cardUI.updatedDateText.text,
                    cardUI.versionText.text,
                    cardUI.typeTagsText.text,
                    cardUI.themeTagsText.text,
                    cardUI.functionTagsText.text,
                    cardUI.definitionTagsText.text,
                    cardUI.batchTagsText.text,
                    cardUI.propertyTagsText.text
                );
                cardInfos.Add(card);
            }
        }

#if UNITY_EDITOR
        string userKey = PlayerPrefs.GetString("currentUserKey", string.Empty);
        if (!string.IsNullOrEmpty(userKey))
        {
            string folderPath = Path.Combine(Application.dataPath, "ZTResource/UserInfo");
            string fileName = userKey + ".csv";
            string filePath = Path.Combine(folderPath, fileName);

            List<string> existingIds = new List<string>();

            if (File.Exists(filePath))
            {
                existingIds = GetExistingIds(filePath);

                foreach (var card in cardInfos)
                {
                    if (existingIds.Contains(card.ID))
                    {
                        hasDuplicate = true;
                        HighlightExistingCard(card.ID);
                    }
                }
            }

            if (hasDuplicate)
            {
                ShowPurchaseFailed();
                return;
            }

            // 尝试写入记录
            try
            {
                WritePurchaseRecord(cardInfos, filePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"写入记录失败: {e.Message}");
                ShowPurchaseFailed(); // 显示购买失败消息的UI
                return;
            }

            // 强制刷新Unity的资源数据库
            UnityEditor.AssetDatabase.Refresh();

            // 购买成功，调用CustomEventSystem.RaiseAddPrefabPath
            foreach (var card in cardInfos)
            {
                if (!AddPathToExportWindow(card.PrefabPath))
                {
                    ShowPathAddFailed(card.PrefabPath); // 显示路径添加失败消息的UI
                    return;
                }
            }
        }
#endif

        // 不论编辑器还是打包后的版本，都执行路径复制和显示UI
        CopyAllPrefabPathsToClipboard(); // 复制路径到剪贴板

        if (title != null)
        {
            title.SetActive(true); // 显示标题
            StartCoroutine(HideTitleAfterSeconds(3)); // 3秒后隐藏标题
        }
    }




    private List<string> GetExistingIds(string filePath)
    {
        List<string> ids = new List<string>();
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

        for (int i = 3; i < lines.Length; i++) // 从第四行开始读取数据
        {
            string[] values = lines[i].Split(',');
            if (values.Length > 0)
            {
                ids.Add(values[0]);
            }
        }

        return ids;
    }

    private void ShowPurchaseFailed()
    {
        if (purchaseFailedUI != null)
        {
            purchaseFailedUI.SetActive(true); // 显示购买失败消息的UI
        }
    }

    private void HighlightExistingCard(string id)
    {
        foreach (Transform child in contentParent)
        {
            SelectedCardUI cardUI = child.GetComponent<SelectedCardUI>();
            if (cardUI != null && cardUI.idText.text == id)
            {
                cardUI.SetDistinctionUIVisibility(true); // 显示区分UI
            }
        }
    }

    public static void WritePurchaseRecord(List<CardInfo> cardInfos, string filePath)
    {
        if (cardInfos == null || cardInfos.Count == 0) return;

        List<string> lines = new List<string>(File.ReadAllLines(filePath, Encoding.UTF8));

        // 确保第三行是CsvHeader
        if (lines.Count < 3 || lines[2] != CsvHeader)
        {
            if (lines.Count < 3)
            {
                while (lines.Count < 3)
                {
                    lines.Add(string.Empty);
                }
            }
            lines[2] = CsvHeader;
        }

        // 修改时间格式为精确到秒
        string batchTitle = $"批次,,{System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}";
        lines.Add(batchTitle);

        foreach (var card in cardInfos)
        {
            string line = $"{card.ID},{card.Name},{card.Description},{card.Height},{card.PrefabPath},{card.ThumbnailPath},{card.ModelFaces},{card.CreationDate},{card.UpdatedDate},{card.Version},{card.TypeTags},{card.ThemeTags},{card.FunctionTags},{card.DefinitionTags},{card.BatchTags},{card.PropertyTags}";
            lines.Add(line);
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }


    private void CopyAllPrefabPathsToClipboard()
    {
        StringBuilder paths = new StringBuilder();
        // 遍历contentParent下的所有子物体
        foreach (Transform child in contentParent)
        {
            // 获取SelectedCardUI组件
            SelectedCardUI cardUI = child.GetComponent<SelectedCardUI>();
            if (cardUI != null && cardUI.prefabPathText != null) // 确保Prefab Path Text对象存在
            {
                TextMeshProUGUI tmpText = cardUI.prefabPathText.GetComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    paths.AppendLine(tmpText.text); // 添加到StringBuilder
                }
            }
        }

        // 将路径字符串复制到剪贴板
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
}
