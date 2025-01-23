using UnityEngine;
using TMPro;

public class ItemDetailsDisplay : MonoBehaviour
{
    public TextMeshProUGUI idText; // 新增ID显示
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI heightText;
    public TextMeshProUGUI creationDateText;
    public TextMeshProUGUI modelFacesText;
    public TextMeshProUGUI updatedDateText;
    public TextMeshProUGUI versionText; // 新增版本号显示
    public TextMeshProUGUI propertyTagsText; // 新增属性标签显示

    // 用于显示详情的方法
    public void DisplayItemDetails(
        string id, // 新增ID参数
        string name,
        string description,
        string height,
        string modelFaces,
        string creationDate,
        string updatedDate,
        string version, // 新增版本号参数
        string propertyTags // 新增属性标签参数
    )
    {
        idText.text = id; // 更新ID文本
        nameText.text = name;
        descriptionText.text = description;
        heightText.text = height; // 假设高度以米为单位
        modelFacesText.text = modelFaces; // 更新模型面数文本
        creationDateText.text = creationDate; // 更新创建日期文本
        updatedDateText.text = updatedDate; // 更新时间文本
        versionText.text = version; // 更新版本号文本
        propertyTagsText.text = propertyTags; // 更新属性标签文本
    }

    // 你可能需要一个方法来清理或隐藏UI，当没有选中任何道具时
    public void ClearDetails()
    {
        idText.text = ""; // 清空ID文本
        nameText.text = "";
        descriptionText.text = "";
        heightText.text = "";
        creationDateText.text = "";
        modelFacesText.text = "";
        updatedDateText.text = "";
        versionText.text = ""; // 清空版本号文本
        propertyTagsText.text = ""; // 清空属性标签文本
    }
}