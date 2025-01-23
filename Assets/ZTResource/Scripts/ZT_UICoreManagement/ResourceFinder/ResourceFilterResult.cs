using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceFilterResult : MonoBehaviour
{
    public string[] typeTag = new string[0];
    public string[] themeTag = new string[0];
    public string[] functionTag = new string[0];
    public string[] batchTag = new string[0];
    public string[] definitionTag = new string[0];
    public string[] propertyTag = new string[0];

    public string SearchTerm = "";
    private ResourceFilter resourceFilter;
    public ResourceCardSpawner resourceCardSpawner;
    public MultipleTagButtonCreator multipleTagButtonCreator;

    public TMP_InputField searchInputField;
    public Button searchButton;
    public Button clearButton;

    public RectTransform panel;
    public Button toggleWidthButton;
    public float defaultWidth = 1621f;
    public float fullScreenWidth = 2806f;
    private bool isFullScreen = false;

    public AiSearch aiSearch;

    private List<string> idList = new List<string>();
    private List<string> searchHistory = new List<string>();

    private void Start()
    {
        resourceFilter = FindObjectOfType<ResourceFilter>();
        FilterAndPrintResources();
        searchButton.onClick.AddListener(PerformSearch);

        clearButton.onClick.AddListener(ClearAllFilters);
        clearButton.gameObject.SetActive(false);

        if (panel != null)
        {
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultWidth);
        }

        if (aiSearch != null)
        {
            aiSearch.OnQueryResultReceived += UpdateIdList;
        }
    }

    private void UpdateIdList(List<string> ids)
    {
        idList = ids;
        SearchTerm = ""; // 在更新 idList 时清空 SearchTerm
        FilterAndPrintResources();
    }


    private void PerformSearch()
    {
        if (searchInputField != null)
        {
            SearchTerm = searchInputField.text;

            AddSearchHistory(SearchTerm);

            idList.Clear(); // 在新搜索前清空 idList

            FilterAndPrintResources();
        }
    }

    public List<string> GetSearchHistory()
    {
        return searchHistory;
    }

    public void AddSearchHistory(string searchTerm)
    {
        if (!string.IsNullOrEmpty(searchTerm) && !searchHistory.Contains(searchTerm))
        {
            searchHistory.Add(searchTerm);

            SearchHistoryDropdownHandler dropdownHandler = FindObjectOfType<SearchHistoryDropdownHandler>();
            if (dropdownHandler != null)
            {
                dropdownHandler.UpdateDropdownOptions();
            }
        }
    }

    public void FilterAndPrintResources()
    {
        if (resourceFilter != null)
        {
            var filteredResources = resourceFilter.FilterResources(typeTag, themeTag, functionTag, batchTag, definitionTag, propertyTag, SearchTerm, idList);

            List<ResourceCardData> sortedResources;
            if (idList.Count > 0)
            {
                sortedResources = new List<ResourceCardData>();
                foreach (var id in idList)
                {
                    var resource = filteredResources.Find(r => r.ID == id);
                    if (resource != null)
                    {
                        sortedResources.Add(resource);
                    }
                }
            }
            else
            {
                sortedResources = filteredResources;
            }

            resourceFilter.PrintFilteredResources(sortedResources);

            if (resourceCardSpawner != null)
            {
                resourceCardSpawner.SetFilteredResources(sortedResources);
            }

            UpdateTagButtonVisibility(sortedResources);
        }

        UpdateClearButtonVisibility();
    }

    private void UpdateTagButtonVisibility(List<ResourceCardData> filteredResources)
    {
        if (multipleTagButtonCreator != null)
        {
            HashSet<string> visibleTags = ExtractTagsFromResources(filteredResources);

            multipleTagButtonCreator.UpdateTagButtonVisibility(visibleTags);
        }
    }

    private HashSet<string> ExtractTagsFromResources(List<ResourceCardData> resources)
    {
        HashSet<string> tags = new HashSet<string>();
        foreach (var resource in resources)
        {
            foreach (var tag in resource.TypeTags) tags.Add(tag);
            foreach (var tag in resource.ThemeTags) tags.Add(tag);
            foreach (var tag in resource.FunctionTags) tags.Add(tag);
            foreach (var tag in resource.DefinitionTags) tags.Add(tag);
            foreach (var tag in resource.BatchTags) tags.Add(tag);
            foreach (var tag in resource.PropertyTags) tags.Add(tag);
        }
        return tags;
    }

    void ToggleWidth()
    {
        if (isFullScreen)
        {
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultWidth);
        }
        else
        {
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullScreenWidth);
        }
        isFullScreen = !isFullScreen;
    }

    public void ClearAllFilters()
    {
        typeTag = new string[0];
        themeTag = new string[0];
        functionTag = new string[0];
        batchTag = new string[0];
        definitionTag = new string[0];
        propertyTag = new string[0];
        SearchTerm = "";
        idList.Clear();

        if (searchInputField != null)
        {
            searchInputField.text = "";
        }

        SearchHistoryDropdownHandler dropdownHandler = FindObjectOfType<SearchHistoryDropdownHandler>();
        if (dropdownHandler != null && dropdownHandler.searchHistoryDropdown != null)
        {
            dropdownHandler.searchHistoryDropdown.onValueChanged.RemoveListener(dropdownHandler.OnSearchHistorySelected);
            dropdownHandler.searchHistoryDropdown.value = 0;
            dropdownHandler.searchHistoryDropdown.onValueChanged.AddListener(dropdownHandler.OnSearchHistorySelected);
        }

        ResetTagButtonStates();

        FilterAndPrintResources();
    }

    private void ResetTagButtonStates()
    {
        if (multipleTagButtonCreator != null)
        {
            multipleTagButtonCreator.ResetAllTagButtonStates();
        }
    }

    private void UpdateClearButtonVisibility()
    {
        bool hasFilters = typeTag.Length > 0 || themeTag.Length > 0 || functionTag.Length > 0 || batchTag.Length > 0 || definitionTag.Length > 0 || propertyTag.Length > 0 || !string.IsNullOrEmpty(SearchTerm) || idList.Count > 0;
        clearButton.gameObject.SetActive(hasFilters);
    }
}