#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class HistoryCardUI : MonoBehaviour
{
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI heightText;
    public TextMeshProUGUI versionText;
    public TextMeshProUGUI totalVersionText; // 新增总表版本号
    public TextMeshProUGUI prefabPathText;
    public Image thumbnailImage;
    public TextMeshProUGUI typeTagsText;
    public TextMeshProUGUI themeTagsText;
    public TextMeshProUGUI functionTagsText;
    public TextMeshProUGUI definitionTagsText;
    public TextMeshProUGUI batchTagsText;
    public TextMeshProUGUI propertyTagsText;
    public TextMeshProUGUI modelFacesText;
    public TextMeshProUGUI creationDateText;
    public TextMeshProUGUI updatedDateText;
    public Button deleteButton;
    public Button addButton;

    // 新增用于显示更新提示的UI实例
    public GameObject updateNotification;

    public GameObject targetParent;
    public ItemDetailsDisplay itemDetailsDisplay;
    public CSVParser cSVParser;
    public ResourceManager resourceManager; // 需要引用 ResourceManager

    private static HistoryCardUI currentSelectedCard;

    private string thumbnailPath;
    public string csvFilePath;

    public void SetupCard(
        string id,
        string name,
        string description,
        string height,
        string version,
        string prefabPath,
        string thumbnailPath,
        string typeTags,
        string themeTags,
        string functionTags,
        string definitionTags,
        string batchTags,
        string propertyTags,
        string modelFaces,
        string creationDate,
        string updatedDate,
        string csvFilePath
    )
    {
        idText.text = id;
        nameText.text = name;
        descriptionText.text = description;
        heightText.text = height;
        versionText.text = version;
        prefabPathText.text = prefabPath;
        typeTagsText.text = typeTags;
        themeTagsText.text = themeTags;
        functionTagsText.text = functionTags;
        definitionTagsText.text = definitionTags;
        batchTagsText.text = batchTags;
        propertyTagsText.text = propertyTags;
        modelFacesText.text = modelFaces;
        creationDateText.text = creationDate;
        updatedDateText.text = updatedDate;

        this.thumbnailPath = thumbnailPath;
        this.csvFilePath = csvFilePath;

#if UNITY_EDITOR
        thumbnailImage.sprite = LoadSpriteFromPath(thumbnailPath);
#endif

        deleteButton.gameObject.SetActive(false);
        addButton.gameObject.SetActive(false);

        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        addButton.onClick.AddListener(OnAddButtonClicked);

        // 调用版本对比方法
        CheckForUpdates(id, version);
    }

    private void CheckForUpdates(string id, string currentVersion)
    {
        if (resourceManager == null)
        {
            Debug.LogError("ResourceManager reference is missing.");
            return;
        }

        var resource = resourceManager.GetResourceById(id);
        if (resource == null)
        {
            Debug.LogWarning($"Resource with ID {id} not found in the total resource list.");
            return;
        }

        // 设置总表版本号
        totalVersionText.text = resource.Version;

        if (resource.Version != currentVersion)
        {
            // 版本不匹配，显示更新信息            
            versionText.color = Color.red; // 设置文本颜色为红色以强调
            updateNotification.SetActive(true); // 显示更新提示UI实例
        }
        else
        {
            updateNotification.SetActive(false); // 隐藏更新提示UI实例
        }
    }

#if UNITY_EDITOR
    private Sprite LoadSpriteFromPath(string thumbnailPath)
    {
        // 使用新路径并直接使用传递的图片名称
        string pathForLoading = "Assets/ZTResource/Resources/ZT_IconTextures/" + thumbnailPath + ".png"; // 确保包括文件扩展名
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(pathForLoading);
        if (texture == null)
        {
            Debug.LogError("Failed to load texture at path: " + pathForLoading);
            return null;
        }
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
#endif

    public void OnCardClicked()
    {
        itemDetailsDisplay.DisplayItemDetails(
            idText.text,
            nameText.text,
            descriptionText.text,
            heightText.text,
            modelFacesText.text,
            creationDateText.text,
            updatedDateText.text,
            versionText.text,
            propertyTagsText.text
        );

#if UNITY_EDITOR
        if (prefabPathText != null && !string.IsNullOrEmpty(prefabPathText.text))
        {
            string assetPath = prefabPathText.text;
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                Debug.LogError("Cannot find asset at path: " + assetPath);
            }
        }
#endif

        if (targetParent == null)
        {
            return;
        }

        bool isAlreadyAdded = false;
        foreach (Transform child in targetParent.transform)
        {
            if (child.name == prefabPathText.text)
            {
                isAlreadyAdded = true;
                break;
            }
        }

        if (isAlreadyAdded)
        {
            return;
        }

        foreach (Transform child in targetParent.transform)
        {
            Destroy(child.gameObject);
        }

#if UNITY_EDITOR
        string resourcePath = prefabPathText.text;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(resourcePath);
        if (prefab != null)
        {
            GameObject instantiatedPrefab = Instantiate(prefab, targetParent.transform.position, Quaternion.identity, targetParent.transform);
            instantiatedPrefab.name = nameText.text;
        }
#endif

        if (currentSelectedCard != null)
        {
            currentSelectedCard.deleteButton.gameObject.SetActive(false);
            currentSelectedCard.addButton.gameObject.SetActive(false);
        }

        currentSelectedCard = this;
        deleteButton.gameObject.SetActive(true);
        addButton.gameObject.SetActive(true);
    }

    private void OnDeleteButtonClicked()
    {
        string cardId = idText.text;
        CSVParser.DeleteResource(csvFilePath, cardId);

        CardInfo cardToRemove = SelectedCardSpawner.Instance.ExistingCards.FirstOrDefault(card => card.ID == cardId);
        if (cardToRemove != null)
        {
            SelectedCardSpawner.Instance.RemoveCardInfo(cardToRemove);
        }

        Destroy(gameObject);
        currentSelectedCard = null;
    }

    public void OnAddButtonClicked()
    {
        SelectedCardSpawner.Instance.SpawnSelectedCard(
            idText.text,
            nameText.text,
            descriptionText.text,
            heightText.text,
            versionText.text,
            prefabPathText.text,
            thumbnailPath,
            typeTagsText.text,
            themeTagsText.text,
            functionTagsText.text,
            definitionTagsText.text,
            batchTagsText.text,
            propertyTagsText.text,
            modelFacesText.text,
            creationDateText.text,
            updatedDateText.text
        );
    }
}
