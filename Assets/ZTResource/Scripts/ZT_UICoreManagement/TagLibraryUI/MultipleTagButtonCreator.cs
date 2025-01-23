using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class MultipleTagButtonCreator : MonoBehaviour
{
    // 六个TagLibrary的引用，每个都对应一个资源文件
    public TagLibrary typeTagLibrary;
    public TagLibrary themeTagLibrary;
    public TagLibrary functionTagLibrary;
    public TagLibrary batchTagLibrary;
    public TagLibrary propertyTagLibrary;
    public TagLibrary definitionTagLibrary;

    // 六种类型的按钮预制体
    public GameObject typeButtonPrefab;
    public GameObject themeButtonPrefab;
    public GameObject functionButtonPrefab;
    public GameObject batchButtonPrefab;
    public GameObject propertyButtonPrefab;
    public GameObject definitionButtonPrefab;

    // 六个按钮父对象的引用，每个都对应一个UI面板
    public Transform typeButtonParent;
    public Transform themeButtonParent;
    public Transform functionButtonParent;
    public Transform batchButtonParent;
    public Transform propertyButtonParent;
    public Transform definitionButtonParent;

    // ResourceFilterResult的引用
    public ResourceFilterResult filterResult;

    // 用于更改按钮颜色的字段
    public Color selectedColor = Color.green;

    // 存储所有标签按钮的字典
    private Dictionary<string, (Button button, Color originalColor)> tagButtons = new Dictionary<string, (Button, Color)>();

    private void Start()
    {
        if (filterResult == null)
        {
            Debug.LogError("ResourceFilterResult 引用未设置。");
            return;
        }

        // 动态创建按钮
        CreateButtonsForLibrary(typeTagLibrary, typeButtonPrefab, typeButtonParent, "type");
        CreateButtonsForLibrary(themeTagLibrary, themeButtonPrefab, themeButtonParent, "theme");
        CreateButtonsForLibrary(functionTagLibrary, functionButtonPrefab, functionButtonParent, "function");
        CreateButtonsForLibrary(batchTagLibrary, batchButtonPrefab, batchButtonParent, "batch");
        CreateButtonsForLibrary(propertyTagLibrary, propertyButtonPrefab, propertyButtonParent, "property");
        CreateButtonsForLibrary(definitionTagLibrary, definitionButtonPrefab, definitionButtonParent, "definition");
    }

    // 新增方法：根据给定的标签集合更新按钮的可见性
    public void UpdateTagButtonVisibility(HashSet<string> visibleTags)
    {
        foreach (KeyValuePair<string, (Button button, Color originalColor)> tagButtonPair in tagButtons)
        {
            // 如果visibleTags包含这个标签，那么按钮应该显示，否则隐藏
            tagButtonPair.Value.button.gameObject.SetActive(visibleTags.Contains(tagButtonPair.Key));
        }
    }

    // 对指定的TagLibrary创建按钮的方法
    private void CreateButtonsForLibrary(TagLibrary tagLibrary, GameObject buttonPrefab, Transform buttonParent, string tagType)
    {
        if (tagLibrary == null || buttonPrefab == null || buttonParent == null)
        {
            Debug.LogError("请确保所有引用都已正确设置。");
            return;
        }

        // 清除旧的按钮
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        // 为每个标签创建按钮
        foreach (var tag in tagLibrary.Tags)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonParent);
            var textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            var button = buttonObj.GetComponent<Button>();
            var buttonImage = buttonObj.GetComponent<Image>();
            if (textComponent != null && button != null)
            {
                textComponent.text = tag;
                Color originalColor = buttonImage.color;  // 保存初始颜色
                button.onClick.AddListener(() => ToggleTagAndFilter(tag, buttonImage, originalColor, tagType));

                // 保存按钮状态
                tagButtons[tag] = (button, originalColor);
            }
            else
            {
                Debug.LogError("预制体上没有找到TextMeshProUGUI或Button组件。");
            }
        }
    }

    // 切换标签并更新筛选结果的方法
    private void ToggleTagAndFilter(string tag, Image buttonImage, Color originalColor, string tagType)
    {
        List<string> updatedTags = new List<string>();

        // 根据传递的tagType选择正确的标签数组
        switch (tagType)
        {
            case "type":
                updatedTags = filterResult.typeTag.ToList();
                break;
            case "theme":
                updatedTags = filterResult.themeTag.ToList();
                break;
            case "function":
                updatedTags = filterResult.functionTag.ToList();
                break;
            case "batch":
                updatedTags = filterResult.batchTag.ToList();
                break;
            case "property":
                updatedTags = filterResult.propertyTag.ToList();
                break;
            case "definition":
                updatedTags = filterResult.definitionTag.ToList();
                break;
        }

        // 添加或移除标签，并更新按钮颜色
        if (!updatedTags.Contains(tag))
        {
            updatedTags.Add(tag);
            buttonImage.color = selectedColor;
        }
        else
        {
            updatedTags.Remove(tag);
            buttonImage.color = originalColor; // 恢复到原始颜色
        }

        // 将更新后的标签数组保存回对应的标签字段
        switch (tagType)
        {
            case "type":
                filterResult.typeTag = updatedTags.ToArray();
                break;
            case "theme":
                filterResult.themeTag = updatedTags.ToArray();
                break;
            case "function":
                filterResult.functionTag = updatedTags.ToArray();
                break;
            case "batch":
                filterResult.batchTag = updatedTags.ToArray();
                break;
            case "property":
                filterResult.propertyTag = updatedTags.ToArray();
                break;
            case "definition":
                filterResult.definitionTag = updatedTags.ToArray();
                break;
        }

        filterResult.FilterAndPrintResources();
    }

    // 重置所有标签按钮状态的方法
    public void ResetAllTagButtonStates()
    {
        foreach (var tagButtonPair in tagButtons)
        {
            tagButtonPair.Value.button.interactable = true;
            tagButtonPair.Value.button.image.color = tagButtonPair.Value.originalColor;
        }
    }
}


