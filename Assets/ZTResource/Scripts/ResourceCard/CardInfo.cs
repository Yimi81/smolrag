using System;
public class CardInfo
{
    public string ID; // 新增ID字段
    public string Name;
    public string Description;
    public string Height;
    public string PrefabPath;
    public string ThumbnailPath;
    public string ModelFaces; // 新增模型面数字段
    public string CreationDate; // 新增创建日期字段
    public string UpdatedDate; // 新增更新日期字段
    public string Version; // 新增版本号字段
    public string TypeTags;
    public string ThemeTags;
    public string FunctionTags;
    public string DefinitionTags;
    public string BatchTags;
    public string PropertyTags; // 新增属性标签字段

    // 在这里添加构造函数以方便创建实例
    public CardInfo(
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
        ID = id;
        Name = name;
        Description = description;
        Height = height;
        PrefabPath = prefabPath;
        ThumbnailPath = thumbnailPath;
        ModelFaces = modelFaces;
        CreationDate = creationDate;
        UpdatedDate = updatedDate;
        Version = version;
        TypeTags = typeTags;
        ThemeTags = themeTags;
        FunctionTags = functionTags;
        DefinitionTags = definitionTags;
        BatchTags = batchTags;
        PropertyTags = propertyTags;
    }

    // 重写Equals方法和GetHashCode方法
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var other = (CardInfo)obj;
        return ID == other.ID
            && Name == other.Name
            && Description == other.Description
            && Height == other.Height
            && Version == other.Version
            && PrefabPath == other.PrefabPath
            && ThumbnailPath == other.ThumbnailPath
            && TypeTags == other.TypeTags
            && ThemeTags == other.ThemeTags
            && FunctionTags == other.FunctionTags
            && DefinitionTags == other.DefinitionTags
            && BatchTags == other.BatchTags
            && PropertyTags == other.PropertyTags
            && ModelFaces == other.ModelFaces
            && CreationDate == other.CreationDate
            && UpdatedDate == other.UpdatedDate;
    }

    public override int GetHashCode()
    {
        // 使用所有字段计算哈希码以确保唯一性
        return HashCode.Combine(ID, Name, Description, Height, Version, PrefabPath, ThumbnailPath, TypeTags);
    }
}
