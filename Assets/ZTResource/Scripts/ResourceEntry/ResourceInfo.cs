#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]

//作用：定义了资源信息的数据结构，通常附加于游戏内的资源（如预制体）上。它包含了资源名称、描述、缩略图以及各种标签（如类型、主题、功能等）。
public class ResourceInfo : MonoBehaviour
{
    public string id;//资源ID
    public string resourceName;//资源名称
    public string resourceDescription;//资源介绍
    public string thumbnailPath;//缩略图路径
    public string modelFaces;//资源面数
    public string itemHeight; // 物品长宽高                   
    public Texture2D resourceThumbnail;//缩略图本身
    public string prefabPath;//prefab路径
    public string creationDate;//资源创建时间  
    public string updatedDate;//资源更新时间
    public string version;//资源版本
    public List<string> typeTags;//类型标签
    public List<string> themeTags;//主题标签
    public List<string> functionTags;//功能标签
    public List<string> definitionTags;//定义标签
    public List<string> batchTags;//批次标签
    public List<string> propertyTags;//属性标签
    // 可以根据需要添加更多字段
}
#endif