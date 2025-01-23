using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceFilter : MonoBehaviour
{
    private ResourceManager resourceManager;

    private void Awake()
    {
        resourceManager = GetComponent<ResourceManager>();
    }

    public List<ResourceCardData> FilterResources(string[] typeTags, string[] themeTags, string[] functionTags, string[] batchTags, string[] definitionTags, string[] propertyTags, string searchTerm, List<string> idList)
    {
        var allResources = resourceManager.GetAllResources();

        var filteredResources = allResources.Where(resource =>
            (batchTags == null || batchTags.Length == 0 || batchTags.All(tag => resource.BatchTags.Contains(tag))) &&
            (functionTags == null || functionTags.Length == 0 || functionTags.All(tag => resource.FunctionTags.Contains(tag))) &&
            (themeTags == null || themeTags.Length == 0 || themeTags.All(tag => resource.ThemeTags.Contains(tag))) &&
            (typeTags == null || typeTags.Length == 0 || typeTags.All(tag => resource.TypeTags.Contains(tag))) &&
            (definitionTags == null || definitionTags.Length == 0 || definitionTags.All(tag => resource.DefinitionTags.Contains(tag))) &&
            (propertyTags == null || propertyTags.Length == 0 || propertyTags.All(tag => resource.PropertyTags.Contains(tag)))
        ).ToList();

        // 搜索词筛选：包括名称、描述、ID、缩略图路径和所有类型的标签
        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            filteredResources = filteredResources.Where(resource =>
                resource.Name.ToLower().Contains(searchTerm) ||
                resource.Description.ToLower().Contains(searchTerm) ||
                resource.ID.ToLower().Contains(searchTerm) || // 新增ID字段筛选
                resource.ThumbnailPath.ToLower().Contains(searchTerm) || // 新增缩略图路径筛选
                resource.TypeTags.Any(tag => tag.ToLower().Contains(searchTerm)) ||
                resource.ThemeTags.Any(tag => tag.ToLower().Contains(searchTerm)) ||
                resource.FunctionTags.Any(tag => tag.ToLower().Contains(searchTerm)) ||
                resource.BatchTags.Any(tag => tag.ToLower().Contains(searchTerm)) ||
                resource.DefinitionTags.Any(tag => tag.ToLower().Contains(searchTerm)) ||
                resource.PropertyTags.Any(tag => tag.ToLower().Contains(searchTerm))
            ).ToList();
        }

        // ID 列表筛选
        if (idList != null && idList.Count > 0)
        {
            filteredResources = filteredResources.Where(resource => idList.Contains(resource.ID)).ToList();
        }

        return filteredResources; // 返回过滤后的资源列表
    }

    // 新增方法：根据资源ID列表获取资源
    public List<ResourceCardData> GetResourcesByIds(List<string> ids)
    {
        var allResources = resourceManager.GetAllResources();
        return allResources.Where(resource => ids.Contains(resource.ID)).ToList();
    }

    // 打印符合条件的资源信息
    public void PrintFilteredResources(List<ResourceCardData> filteredResources)
    {
        if (filteredResources == null || filteredResources.Count == 0)
        {
            return;
        }
        foreach (var resource in filteredResources)
        {
            // Debug.Log($"Name: {resource.Name}, Description: {resource.Description}, Height: {resource.Height}," +
            //     $"Type Tags: {string.Join(", ", resource.TypeTags)}, Theme Tags: {string.Join(", ", resource.ThemeTags)}," +
            //     $"Function Tags: {string.Join(", ", resource.FunctionTags)}, Definition Tags: {string.join(", ", resource.DefinitionTags)}," +
            //     $"Batch Tags: {string.Join(", ", resource.BatchTags)}, Property Tags: {string.Join(", ", resource.PropertyTags)}," +
            //     $"Prefab Path: {resource.PrefabPath}, Thumbnail Path: {resource.ThumbnailPath}");
        }
    }
}
