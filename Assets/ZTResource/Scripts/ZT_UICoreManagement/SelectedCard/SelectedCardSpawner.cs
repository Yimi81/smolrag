

using System.Collections.Generic;
using UnityEngine;

public class SelectedCardSpawner : MonoBehaviour
{
    public GameObject selectedCardPrefab;
    public Transform contentParent;
    private List<CardInfo> existingCards = new List<CardInfo>();

    public static SelectedCardSpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<CardInfo> ExistingCards => existingCards;

    public void SpawnSelectedCard(
        string id,
        string name,
        string description,
        string height,
        string version,
        string prefabPath,
        string thumbnailPath, // 确保传递缩略图路径
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

        CardInfo newCard = new CardInfo(
            id,
            name,
            description,
            height,
            version,
            prefabPath,
            thumbnailPath,
            typeTags,
            themeTags,
            functionTags,
            definitionTags,
            batchTags,
            propertyTags,
            modelFaces,
            creationDate,
            updatedDate
        );

        // 检查是否已经存在完全相同的卡片
        if (existingCards.Contains(newCard))
        {
            return;
        }

        // 不存在完全相同的卡片，进行实例化和设置
        GameObject selectedCard = Instantiate(selectedCardPrefab, contentParent);
        SelectedCardUI selectedCardUI = selectedCard.GetComponent<SelectedCardUI>();
        if (selectedCardUI != null)
        {
            selectedCardUI.SetupCard(
                id,
                name,
                description,
                height,
                version,
                prefabPath,
                thumbnailPath, // 确保传递缩略图路径
                typeTags,
                themeTags,
                functionTags,
                definitionTags,
                batchTags,
                propertyTags,
                modelFaces,
                creationDate,
                updatedDate
            );
            existingCards.Add(newCard); // 添加到已存在卡片列表中
        }
    }


    public void RemoveCardInfo(CardInfo cardInfo)
    {
        if (existingCards.Contains(cardInfo))
        {
            existingCards.Remove(cardInfo);
        }
    }

    public void DeleteAllCards()
    {
        // 遍历contentParent的所有子物体并销毁它们
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 清空维护已存在卡片的列表
        existingCards.Clear();
    }
}
