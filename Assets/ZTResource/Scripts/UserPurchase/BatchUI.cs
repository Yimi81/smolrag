using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BatchUI : MonoBehaviour
{
    public TextMeshProUGUI batchNameText;
    public TextMeshProUGUI batchTimeText;
    public Transform contentParent;
    public GameObject cardPrefab;
    public float cardHeightIncrement;  // 这个高度值由你来设定
    public float defaultHeight; // 默认高度
    public TMP_InputField batchNameInputField; // 用于修改批次名称的输入字段
    public Button saveBatchNameButton;  // 用于保存批次名称的按钮
    public Button deleteBatchButton; // 用于删除批次的按钮
    public Button addAllToCartButton; // 新增按钮用于将所有资源添加到购物车
    public GameObject slotObject; // 新增的插槽对象
    public Button collapseButton; // 折叠/展开按钮
    private bool isCollapsed = false; // 用于标记当前批次是否折叠

    private string csvFilePath; // 新增存储CSV文件路径的字段

    private List<CardInfo> savedCardInfos = new List<CardInfo>(); // 保存卡片数据的列表


    public RectTransform rectTransform;

    private void OnEnable()
    {
        UpdateHeight(contentParent.childCount - 1); // 减去一个BatchRecord

        // 初始化时隐藏插槽中的对象
        slotObject.SetActive(false);

        // 订阅InputField的事件
        batchNameInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        batchNameInputField.onEndEdit.AddListener(OnInputFieldEndEdit);

        // 添加新的按钮事件监听器
        addAllToCartButton.onClick.AddListener(OnAddAllToCartButtonClicked);
        collapseButton.onClick.AddListener(ToggleCollapse); // 添加折叠按钮事件
    }

    // 切换折叠状态的方法
    private void ToggleCollapse()
    {
        isCollapsed = !isCollapsed;

        if (isCollapsed)
        {
            // 折叠时，仅隐藏所有CardPrefab对象
            foreach (Transform child in contentParent)
            {
                if (child.gameObject.CompareTag("CardPrefab"))
                {
                    child.gameObject.SetActive(false); // 隐藏卡片
                }
            }
            // 更新高度为默认高度
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, defaultHeight);
        }
        else
        {
            // 展开时，仅显示所有隐藏的CardPrefab对象
            foreach (Transform child in contentParent)
            {
                if (child.gameObject.CompareTag("CardPrefab"))
                {
                    child.gameObject.SetActive(true); // 显示卡片
                }
            }
            // 更新高度为重新显示的Card数量的高度
            UpdateHeight(contentParent.childCount - 1); // 恢复高度
        }

        // 通过旋转按钮来表示折叠/展开状态
        collapseButton.GetComponent<RectTransform>().localRotation = isCollapsed
            ? Quaternion.Euler(0, 0, 0) // 折叠时旋转到0度
            : Quaternion.Euler(0, 0, -90);  // 展开时旋转到-90度
    }




    private void OnTransformChildrenChanged()
    {
        UpdateHeight(contentParent.childCount - 1); // 减去一个BatchRecord
    }


    public void SetupBatch(string batchName, string batchTime, List<CardInfo> cardInfos, string csvFilePath)
    {
        this.csvFilePath = csvFilePath;

        // 允许 batchName 为空
        batchNameText.text = string.IsNullOrEmpty(batchName) ? "可输入批次名" : batchName;
        batchTimeText.text = batchTime;

        // 保存CardInfo数据
        savedCardInfos = cardInfos;

        // 创建新的Card
        foreach (var cardInfo in cardInfos)
        {
            GameObject cardObject = Instantiate(cardPrefab, contentParent);
            cardObject.tag = "CardPrefab"; // 给cardPrefab加上Tag方便管理
            HistoryCardUI cardUI = cardObject.GetComponent<HistoryCardUI>();
            cardUI.SetupCard(
                cardInfo.ID,
                cardInfo.Name,
                cardInfo.Description,
                cardInfo.Height,
                cardInfo.Version,
                cardInfo.PrefabPath,
                cardInfo.ThumbnailPath,
                cardInfo.TypeTags,
                cardInfo.ThemeTags,
                cardInfo.FunctionTags,
                cardInfo.DefinitionTags,
                cardInfo.BatchTags,
                cardInfo.PropertyTags,
                cardInfo.ModelFaces,
                cardInfo.CreationDate,
                cardInfo.UpdatedDate,
                csvFilePath // 传递CSV文件路径
            );
        }

        // 添加按钮事件监听
        saveBatchNameButton.onClick.AddListener(SaveBatchName);
        deleteBatchButton.onClick.AddListener(DeleteBatch);
    }


    // 动态更新高度的方法
    public void UpdateHeight(int cardCount)
    {
        if (rectTransform != null)
        {
            float newHeight = defaultHeight + (cardCount * cardHeightIncrement);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
        }
    }

    // 保存批次名称的方法
    public void SaveBatchName()
    {
        string newBatchName = batchNameInputField.text;
        string oldBatchTime = batchTimeText.text;
        CSVParser.RenameBatch(csvFilePath, oldBatchTime, newBatchName);
        batchNameText.text = newBatchName; // 更新UI显示的批次名称
    }

    // 删除批次的方法
    public void DeleteBatch()
    {
        string batchTime = batchTimeText.text;
        CSVParser.DeleteBatch(csvFilePath, batchTime);
        Destroy(gameObject); // 删除自身实例
    }

    // 移除Card并更新高度的方法
    public void RemoveCard(GameObject cardObject)
    {
        Destroy(cardObject);
        UpdateHeight(contentParent.childCount - 2); // 减去一个BatchRecord和将要移除的Card
    }

    // 增加Card并更新高度的方法
    public void AddCard(CardInfo cardInfo)
    {

        GameObject cardObject = Instantiate(cardPrefab, contentParent);
        HistoryCardUI cardUI = cardObject.GetComponent<HistoryCardUI>();
        cardUI.SetupCard(
            cardInfo.ID,
            cardInfo.Name,
            cardInfo.Description,
            cardInfo.Height,
            cardInfo.Version,
            cardInfo.PrefabPath,
            cardInfo.ThumbnailPath,
            cardInfo.TypeTags,
            cardInfo.ThemeTags,
            cardInfo.FunctionTags,
            cardInfo.DefinitionTags,
            cardInfo.BatchTags,
            cardInfo.PropertyTags,
            cardInfo.ModelFaces,
            cardInfo.CreationDate,
            cardInfo.UpdatedDate,
            csvFilePath // 传递CSV文件路径
        );

        UpdateHeight(contentParent.childCount - 1); // 减去一个BatchRecord

    }

    // 输入框内容改变时显示插槽中的对象
    private void OnInputFieldValueChanged(string value)
    {
        if (!slotObject.activeSelf)
        {
            slotObject.SetActive(true);
        }
    }

    // 输入框失去焦点时隐藏插槽中的对象
    private void OnInputFieldEndEdit(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            slotObject.SetActive(false);
        }
    }

    // 将所有资源添加到购物车的方法
    private void OnAddAllToCartButtonClicked()
    {
        foreach (Transform child in contentParent)
        {
            HistoryCardUI cardUI = child.GetComponent<HistoryCardUI>();
            if (cardUI != null)
            {
                cardUI.OnAddButtonClicked();
            }
        }
    }
}