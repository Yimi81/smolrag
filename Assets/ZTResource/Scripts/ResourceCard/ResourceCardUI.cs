using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceCardUI : MonoBehaviour
{
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI heightText;
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
    public TextMeshProUGUI versionText;

    public SelectedCardSpawner selectedCardSpawner;
    public ItemDetailsDisplay itemDetailsDisplay;

    public GameObject targetParent;
    public Button addButton;

    private bool isCardSelected = false;
    private static ResourceCardUI currentlySelectedCard;

    public void SetupCard(
        string id,
        string name,
        string description,
        string height,
        string prefabPath,
        string thumbnailPath,
        string modelFaces,
        string creationDate,
        string updatedDate,
        string version,
        string typeTags,
        string themeTags,
        string functionTags,
        string definitionTags,
        string batchTags,
        string propertyTags
    )
    {
        idText.text = id;
        nameText.text = name;
        descriptionText.text = description;
        heightText.text = height;
        prefabPathText.text = prefabPath;

        string resourcePath = "ZT_IconTextures/" + thumbnailPath;
        Texture2D thumbnail = Resources.Load<Texture2D>(resourcePath);

        if (thumbnail != null)
        {
            thumbnailImage.sprite = Sprite.Create(thumbnail, new Rect(0.0f, 0.0f, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        else
        {
            Debug.LogWarning($"无法加载缩略图资源：{resourcePath}");
        }

        modelFacesText.text = modelFaces;
        creationDateText.text = creationDate;
        updatedDateText.text = updatedDate;
        versionText.text = version;
        typeTagsText.text = typeTags;
        themeTagsText.text = themeTags;
        functionTagsText.text = functionTags;
        definitionTagsText.text = definitionTags;
        batchTagsText.text = batchTags;
        propertyTagsText.text = propertyTags;

        if (addButton != null)
        {
            addButton.gameObject.SetActive(false);
        }

        // 获取当前卡片上的 PropertyInstanceManager 组件
        var propertyInstanceManager = GetComponentInChildren<PropertyInstanceManager>();
        if (propertyInstanceManager != null)
        {
            // 直接调用 PropertyInstanceManager 的更新方法
            propertyInstanceManager.AddPropertyText(propertyTagsText);
            propertyInstanceManager.UpdateInstanceVisibility(); // 手动触发更新
        }
    }

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
        if (!string.IsNullOrEmpty(prefabPathText.text))
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

        if (currentlySelectedCard != null && currentlySelectedCard != this)
        {
            currentlySelectedCard.DeselectCard();
        }

        isCardSelected = true;
        if (addButton != null)
        {
            addButton.gameObject.SetActive(true);
            addButton.onClick.AddListener(OnAddButtonClicked);
        }

        currentlySelectedCard = this;

        if (targetParent != null)
        {
            AddOrReplacePrefab();
        }
    }

    private void DeselectCard()
    {
        isCardSelected = false;
        if (addButton != null)
        {
            addButton.gameObject.SetActive(false);
            addButton.onClick.RemoveListener(OnAddButtonClicked);
        }
    }

    private void OnAddButtonClicked()
    {
        if (isCardSelected)
        {
            string thumbnailPath = GetThumbnailPath();
            selectedCardSpawner.SpawnSelectedCard(
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

            isCardSelected = false;
            if (addButton != null)
            {
                addButton.gameObject.SetActive(false);
                addButton.onClick.RemoveListener(OnAddButtonClicked);
            }
        }
        else
        {
            Debug.LogWarning("Card is not selected, aborting SpawnSelectedCard.");
        }
    }

    private string GetThumbnailPath()
    {
        return thumbnailImage.sprite.texture.name;
    }

    private void AddOrReplacePrefab()
    {
        if (targetParent != null)
        {
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
            else
            {
                Debug.LogError("Prefab未找到，请检查路径：" + resourcePath);
            }
#endif
        }
    }
}
