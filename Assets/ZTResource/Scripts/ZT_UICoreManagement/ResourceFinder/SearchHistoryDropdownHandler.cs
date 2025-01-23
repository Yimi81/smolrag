using UnityEngine;
using TMPro;

public class SearchHistoryDropdownHandler : MonoBehaviour
{
    public TMP_Dropdown searchHistoryDropdown;
    public ResourceFilterResult resourceFilterResult;

    private void Start()
    {
        if (searchHistoryDropdown != null)
        {
            searchHistoryDropdown.onValueChanged.AddListener(OnSearchHistorySelected);
        }

        UpdateDropdownOptions();
    }

    public void UpdateDropdownOptions()
    {
        if (searchHistoryDropdown != null && resourceFilterResult != null)
        {
            searchHistoryDropdown.onValueChanged.RemoveListener(OnSearchHistorySelected);

            var searchHistory = resourceFilterResult.GetSearchHistory();

            searchHistoryDropdown.ClearOptions();
            searchHistoryDropdown.options.Add(new TMP_Dropdown.OptionData(""));

            searchHistoryDropdown.AddOptions(searchHistory);
            searchHistoryDropdown.value = 0;

            searchHistoryDropdown.onValueChanged.AddListener(OnSearchHistorySelected);
        }
    }

    public void OnSearchHistorySelected(int index)
    {
        if (index > 0 && resourceFilterResult != null)
        {
            var searchHistory = resourceFilterResult.GetSearchHistory();
            if (index - 1 < searchHistory.Count)
            {
                string selectedTerm = searchHistory[index - 1];

                // 将选中的记录填写到Search Input Field和Dropdown的Caption Text中
                resourceFilterResult.searchInputField.text = selectedTerm;
                searchHistoryDropdown.captionText.text = selectedTerm;
            }
        }
    }
}
