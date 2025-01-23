using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SelectedCardUI : MonoBehaviour
{
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI heightText;
    public TextMeshProUGUI versionText;
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

    public GameObject targetParent;

    public GameObject distinctionUIInstance;
    public GameObject pathErrorUIInstance;

    public ItemDetailsDisplay itemDetailsDisplay;

    private static SelectedCardUI currentSelectedCard;

    void Start()
    {
        if (distinctionUIInstance != null)
        {
            distinctionUIInstance.SetActive(false);
        }
        if (pathErrorUIInstance != null)
        {
            pathErrorUIInstance.SetActive(false);
        }
    }

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
        string updatedDate
    )
    {
        idText.text = id;
        nameText.text = name;
        descriptionText.text = description;
        heightText.text = $"{height:F2}";
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

        // 在运行时和编辑器中使用不同的方式加载图片
        thumbnailImage.sprite = LoadSpriteFromPath(thumbnailPath);

        deleteButton.gameObject.SetActive(false);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);

        if (pathErrorUIInstance != null)
        {
            pathErrorUIInstance.SetActive(false);
        }
    }

    private Sprite LoadSpriteFromPath(string thumbnailPath)
    {
#if UNITY_EDITOR
        string pathForLoading = "Assets/ZTResource/Resources/ZT_IconTextures/" + thumbnailPath + ".png";
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(pathForLoading);
        if (texture == null)
        {
            Debug.LogError("Failed to load texture at path: " + pathForLoading);
            return null;
        }
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
#else
        string resourcePath = "ZT_IconTextures/" + thumbnailPath;
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogError("Failed to load texture from Resources: " + resourcePath);
            return null;
        }
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
#endif
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
                ShowPathErrorUI();
            }
        }
#endif

        if (targetParent == null)
        {
            return;
        }

        bool isAlreadyAdded = targetParent.transform.Cast<Transform>().Any(child => child.name == prefabPathText.text);

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
        }

        currentSelectedCard = this;
        deleteButton.gameObject.SetActive(true);
    }

    private void OnDeleteButtonClicked()
    {
        CardInfo cardToRemove = SelectedCardSpawner.Instance.ExistingCards.FirstOrDefault(card => card.Name == nameText.text);

        if (cardToRemove != null)
        {
            SelectedCardSpawner.Instance.RemoveCardInfo(cardToRemove);
        }

        Destroy(gameObject);
        currentSelectedCard = null;
    }

    public void SetDistinctionUIVisibility(bool isVisible)
    {
        if (distinctionUIInstance != null)
        {
            distinctionUIInstance.SetActive(isVisible);
        }
    }

    public void SetPathErrorUIVisibility(bool isVisible)
    {
        if (pathErrorUIInstance != null)
        {
            pathErrorUIInstance.SetActive(isVisible);
        }
    }

    public void ShowPathErrorUI()
    {
        if (pathErrorUIInstance != null)
        {
            pathErrorUIInstance.SetActive(true);
        }
    }

    public void HidePathErrorUI()
    {
        if (pathErrorUIInstance != null)
        {
            pathErrorUIInstance.SetActive(false);
        }
    }
}
