using UnityEngine;
using UnityEngine.UI;

public class ZTGridScaler : MonoBehaviour
{
    public GridLayoutGroup gridLayoutGroup;  // 指向Grid Layout Group的引用
    public Scrollbar scrollbar;              // 指向Scrollbar的引用
    public float minWidth = 200f;            // 宽度的最小值
    public float maxWidth = 500f;            // 宽度的最大值
    public float minHeight = 200f;           // 高度的最小值
    public float maxHeight = 500f;           // 高度的最大值

    void Update()
    {
        if (gridLayoutGroup != null && scrollbar != null)
        {
            // 根据Scrollbar的值插值计算当前宽度和高度
            float currentWidth = Mathf.Lerp(minWidth, maxWidth, scrollbar.value);
            float currentHeight = Mathf.Lerp(minHeight, maxHeight, scrollbar.value);
            // 设置GridLayout的Cell Size
            gridLayoutGroup.cellSize = new Vector2(currentWidth, currentHeight);
        }
    }
}