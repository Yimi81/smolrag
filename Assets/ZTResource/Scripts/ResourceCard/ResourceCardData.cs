using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//作用：定义了一个资源卡片的数据结构，用于在UI中展示资源信息，如名称、描述、高度和标签等。
public class ResourceCardData
{
    public string ID; // 新增ID字段
    public string Name;
    public string Description;
    public string Height;
    public List<string> TypeTags;
    public List<string> ThemeTags;
    public List<string> FunctionTags;
    public List<string> DefinitionTags;
    public List<string> BatchTags;
    public List<string> PropertyTags; // 新增属性标签字段
    public string PrefabPath;
    public string ThumbnailPath;
    public string ModelFaces;
    public string CreationDate;
    public string UpdatedDate;
    public string Version; // 新增版本号字段

    public ResourceCardData(string id, string name, string description, string height, List<string> typeTags, List<string> themeTags, List<string> functionTags, List<string> definitionTags, List<string> batchTags, List<string> propertyTags, string prefabPath, string thumbnailPath, string modelFaces, string creationDate, string updatedDate, string version)
    {
        ID = id;
        Name = name;
        Description = description;
        Height = height;
        TypeTags = typeTags;
        ThemeTags = themeTags;
        FunctionTags = functionTags;
        DefinitionTags = definitionTags;
        BatchTags = batchTags;
        PropertyTags = propertyTags;
        PrefabPath = prefabPath;
        ThumbnailPath = thumbnailPath;
        ModelFaces = modelFaces;
        CreationDate = creationDate;
        UpdatedDate = updatedDate;
        Version = version;
    }
}