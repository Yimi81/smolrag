using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ResourceCardSpawner : MonoBehaviour
{
    public GameObject resourceCardPrefab; // 资源卡的预制体
    public Transform cardsParent; // 卡片生成的父对象
    private List<ResourceCardData> initialResources; // 初始资源数据
    private List<ResourceCardData> currentFilteredResources; // 当前筛选后的资源数据
    private int currentPage = 0; // 当前页码
    public int cardsPerPage = 5; // 每页显示的卡片数量

    public TMP_InputField pageInputField; // 分页数值显示的TMP_InputField
    public TMP_InputField cardsPerPageInputField; // 每页卡片数量的TMP_InputField
    public Button prevButton; // 上一页按钮
    public Button nextButton; // 下一页按钮

    public TextMeshProUGUI totalResourcesText;  // 显示总资源数量的

    //排列变量
    public enum SortType
    {
        Default,
        ModelFaces,
        Height,
        CreationDate
    }

    private SortType currentSortType = SortType.Default;
    private bool sortDescending = false;

    public Button defaultSortButton;
    public Button modelFacesSortButton;
    public Button heightSortButton;
    public Button creationDateSortButton;
    //筛选箭头表示
    public Image modelFacesArrowUp;
    public Image modelFacesArrowDown;
    public Image heightArrowUp;
    public Image heightArrowDown;
    public Image creationDateArrowUp;
    public Image creationDateArrowDown;

    void Start()
    {
        defaultSortButton.onClick.AddListener(() => SetSortType(SortType.Default));
        modelFacesSortButton.onClick.AddListener(() => SetSortType(SortType.ModelFaces));
        heightSortButton.onClick.AddListener(() => SetSortType(SortType.Height));
        creationDateSortButton.onClick.AddListener(() => SetSortType(SortType.CreationDate));

        // 初始化pageInputField
        pageInputField.onEndEdit.AddListener(OnPageInputChanged);

        // 初始化每页卡片数量输入框，并显示当前每页的卡片数量
        cardsPerPageInputField.text = cardsPerPage.ToString();
        cardsPerPageInputField.onEndEdit.AddListener(OnCardsPerPageInputChanged);
    }

    private void OnPageInputChanged(string input)
    {
        int newPage;
        if (int.TryParse(input, out newPage))
        {
            newPage = Mathf.Clamp(newPage - 1, 0, Mathf.CeilToInt(currentFilteredResources.Count / (float)cardsPerPage) - 1);
            if (newPage != currentPage)
            {
                currentPage = newPage;
                UpdateCardsOnPage();
            }
        }
        else
        {
            // 如果输入无效，重置为当前页码
            pageInputField.text = (currentPage + 1).ToString();
        }
    }

    private void OnCardsPerPageInputChanged(string input)
    {
        int newCardsPerPage;
        if (int.TryParse(input, out newCardsPerPage) && newCardsPerPage > 0)
        {
            cardsPerPage = newCardsPerPage;
            currentPage = 0; // 重置到第一页
            UpdateCardsOnPage();
        }
        else
        {
            // 如果输入无效，重置为当前每页卡片数量
            cardsPerPageInputField.text = cardsPerPage.ToString();
        }
    }

    public void SetSortType(SortType sortType)
    {
        if (currentSortType == sortType)
        {
            sortDescending = !sortDescending; // 如果已经在当前排序类型下，改变排序方向
        }
        else
        {
            currentSortType = sortType;
            sortDescending = false; // 默认为升序
        }

        SortResources();
    }

    public void ResetSort()
    {
        currentSortType = SortType.Default;
        sortDescending = false;
        SortResources();
    }

    // 接收筛选后的资源数据并更新UI
    public void SetFilteredResources(List<ResourceCardData> filteredResources)
    {
        initialResources = new List<ResourceCardData>(filteredResources); // 保存初始资源数据
        currentFilteredResources = new List<ResourceCardData>(filteredResources);
        currentPage = 0; // 重置到第一页

        currentSortType = SortType.Default;
        UpdateCardsOnPage();
        UpdateTotalResourcesText();
        //Debug.Log($"筛选后的资源总数: {currentFilteredResources.Count}"); // 添加打印日志
    }

    private void UpdateTotalResourcesText()
    {
        totalResourcesText.text = $"筛选总数: {currentFilteredResources.Count}";
    }

    // 更新当前页面上的卡片
    private void UpdateCardsOnPage()
    {
        ClearCards();

        if (currentFilteredResources == null || currentFilteredResources.Count == 0) return;

        int start = currentPage * cardsPerPage;
        int end = Mathf.Min(start + cardsPerPage, currentFilteredResources.Count);

        for (int i = start; i < end; i++)
        {
            var resourceData = currentFilteredResources[i];
            // 去除Name字段中的双引号
            string nameWithoutQuotes = resourceData.Name.Replace("\"", "");
            string descriptionWithoutQuotes = resourceData.Description.Replace("\"", "");
            var newCard = Instantiate(resourceCardPrefab, cardsParent);
            var cardUI = newCard.GetComponent<ResourceCardUI>();

            cardUI?.SetupCard(
                resourceData.ID,
                nameWithoutQuotes, // 使用去掉引号的名称
                descriptionWithoutQuotes,
                resourceData.Height,
                resourceData.PrefabPath,
                resourceData.ThumbnailPath,
                resourceData.ModelFaces,
                resourceData.CreationDate,
                resourceData.UpdatedDate,
                resourceData.Version,
                string.Join(", ", resourceData.TypeTags),
                string.Join(", ", resourceData.ThemeTags),
                string.Join(", ", resourceData.FunctionTags),
                string.Join(", ", resourceData.DefinitionTags),
                string.Join(", ", resourceData.BatchTags),
                string.Join(", ", resourceData.PropertyTags)
            );
        }

        UpdateUI();
    }

    // 清除当前页面的所有卡片
    private void ClearCards()
    {
        foreach (Transform child in cardsParent)
        {
            Destroy(child.gameObject);
        }
    }

    // 翻页方法，可供按钮等 UI 元素调用
    public void NextPage()
    {
        if (currentPage * cardsPerPage < currentFilteredResources.Count - cardsPerPage)
        {
            currentPage++;
            UpdateCardsOnPage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateCardsOnPage();
        }
    }

    // 更新UI组件，如分页文本和按钮状态
    private void UpdateUI()
    {
        int totalPages = Mathf.CeilToInt(currentFilteredResources.Count / (float)cardsPerPage);
        pageInputField.text = $"{currentPage + 1}/{totalPages}"; // 显示为“当前页/总页数”
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage * cardsPerPage < currentFilteredResources.Count - cardsPerPage;

        // 隐藏所有箭头图标
        modelFacesArrowUp.gameObject.SetActive(false);
        modelFacesArrowDown.gameObject.SetActive(false);
        heightArrowUp.gameObject.SetActive(false);
        heightArrowDown.gameObject.SetActive(false);
        creationDateArrowUp.gameObject.SetActive(false);
        creationDateArrowDown.gameObject.SetActive(false);

        // 根据当前的排序类型和方向显示箭头图标
        switch (currentSortType)
        {
            case SortType.ModelFaces:
                if (sortDescending)
                    modelFacesArrowDown.gameObject.SetActive(true);
                else
                    modelFacesArrowUp.gameObject.SetActive(true);
                break;
            case SortType.Height:
                if (sortDescending)
                    heightArrowDown.gameObject.SetActive(true);
                if (sortDescending)
                    heightArrowDown.gameObject.SetActive(true);
                else
                    heightArrowUp.gameObject.SetActive(true);
                break;
            case SortType.CreationDate:
                if (sortDescending)
                    creationDateArrowDown.gameObject.SetActive(true);
                else
                    creationDateArrowUp.gameObject.SetActive(true);
                break;
            default:
                // 默认排序，没有箭头显示
                break;
        }
    }

    private void SortResources()
    {
        if (currentFilteredResources == null) return;

        switch (currentSortType)
        {
            case SortType.ModelFaces:
                currentFilteredResources = sortDescending
                    ? currentFilteredResources.OrderByDescending(res => res.ModelFaces).ToList()
                    : currentFilteredResources.OrderBy(res => res.ModelFaces).ToList();
                break;
            case SortType.Height:
                currentFilteredResources = sortDescending
                    ? currentFilteredResources.OrderByDescending(res => res.Height).ToList()
                    : currentFilteredResources.OrderBy(res => res.Height).ToList();
                break;
            case SortType.CreationDate:
                currentFilteredResources = sortDescending
                    ? currentFilteredResources.OrderByDescending(res => res.CreationDate).ToList()
                    : currentFilteredResources.OrderBy(res => res.CreationDate).ToList();
                break;
            case SortType.Default:
                currentFilteredResources = sortDescending
                    ? initialResources.OrderByDescending(res => initialResources.IndexOf(res)).ToList()
                    : new List<ResourceCardData>(initialResources); // 恢复初始顺序
                break;
        }

        currentPage = 0;
        UpdateCardsOnPage();
    }
}