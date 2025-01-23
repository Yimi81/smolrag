using UnityEngine;
using TMPro;

public class PropertyInstanceManager : MonoBehaviour
{
    public GameObject LODInstance; // LDO
    public GameObject ColorChangeInstance; // 变色
    public GameObject WallMountInstance; // 壁挂    
    public GameObject DynamicInstance; // 动态
    public GameObject HookPointInstance; // 挂点
    public GameObject OldEffectInstance; // 做旧
    public TMP_Text Property_Text;

    // 添加实例的方法
    public void AddLODInstance(GameObject instance)
    {
        LODInstance = instance;
    }

    public void AddColorChangeInstance(GameObject instance)
    {
        ColorChangeInstance = instance;
    }

    public void AddWallMountInstance(GameObject instance)
    {
        WallMountInstance = instance;
    }

    public void AddHookPointInstance(GameObject instance)
    {
        HookPointInstance = instance;
    }

    public void AddDynamicInstance(GameObject instance)
    {
        DynamicInstance = instance;
    }

    public void AddOldEffectInstance(GameObject instance)
    {
        OldEffectInstance = instance;
    }

    // 添加Property_Text实例的方法
    public void AddPropertyText(TMP_Text text)
    {
        Property_Text = text;
    }

    // 更新实例显示状态的方法
    public void UpdateInstanceVisibility()
    {
        if (Property_Text == null) return;

        string text = Property_Text.text;

        LODInstance?.SetActive(text.Contains("LOD"));
        ColorChangeInstance?.SetActive(text.Contains("变色"));
        WallMountInstance?.SetActive(text.Contains("壁挂"));
        HookPointInstance?.SetActive(text.Contains("挂点"));
        DynamicInstance?.SetActive(text.Contains("动态"));
        OldEffectInstance?.SetActive(text.Contains("做旧"));
    }
}
