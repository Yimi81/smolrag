using UnityEngine;

// 放在一个通用的命名空间中
[CreateAssetMenu(fileName = "NewTagLibrary", menuName = "TagSystem/TagLibrary", order = 0)]
public class TagLibrary : ScriptableObject, ITagLibrary
{
    public string category;
    public string[] tags;

    public string[] Tags => tags;
}
