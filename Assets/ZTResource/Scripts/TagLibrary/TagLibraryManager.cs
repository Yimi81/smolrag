#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TagGroup
{
    public string groupName;
    public List<TagLibrary> tagLibraries;
}

[CreateAssetMenu(fileName = "TagLibraryManager", menuName = "TagSystem/TagLibraryManager", order = 1)]
public class TagLibraryManager : ScriptableObject
{
    public List<TagGroup> tagGroups;
}


#endif