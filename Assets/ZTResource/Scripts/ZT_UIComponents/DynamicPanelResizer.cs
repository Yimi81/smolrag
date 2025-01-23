using UnityEngine;
using UnityEngine.UI;

public class DynamicPanelResizer : MonoBehaviour
{
    public RectTransform mainPanel; // 主Panel
    public RectTransform tagPanel; // Panel_标签
    public RectTransform searchResultsPanel; // Panel_搜索结果

    private float lastTagPanelHeight = -1f; // 存储上一次Panel_标签的高度

    void Update()
    {
        // 检查Panel_标签的高度是否发生了变化
        if (tagPanel.rect.height != lastTagPanelHeight)
        {
            lastTagPanelHeight = tagPanel.rect.height; // 更新存储的高度
            AdjustSearchResultsPanel(); // 调整Panel_搜索结果的大小
        }
    }

    void AdjustSearchResultsPanel()
    {
        // 计算Panel_搜索结果的新高度
        float newHeight = mainPanel.rect.height - tagPanel.rect.height;

        // 设置Panel_搜索结果的高度
        searchResultsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
    }
}