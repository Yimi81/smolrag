#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionBox : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform selectionBox;
    public RectTransform content;

    private Vector2 startPoint;

    void Start()
    {
        selectionBox.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startPoint = eventData.position;
        selectionBox.gameObject.SetActive(true);
        UpdateSelectionBox(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateSelectionBox(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        selectionBox.gameObject.SetActive(false);
        SelectInstances();
    }

    private void UpdateSelectionBox(Vector2 currentPoint)
    {
        Vector2 size = currentPoint - startPoint;
        selectionBox.anchoredPosition = startPoint;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));

        if (size.x < 0)
            selectionBox.anchoredPosition += new Vector2(size.x, 0);
        if (size.y < 0)
            selectionBox.anchoredPosition += new Vector2(0, size.y);
    }

    private void SelectInstances()
    {
        Rect selectionRect = new Rect(selectionBox.anchoredPosition, selectionBox.sizeDelta);

        foreach (Transform child in content)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(selectionBox, childRect.position))
            {
                Debug.Log("Selected: " + child.name);
                // 对选中的实例进行操作，比如高亮显示或添加到选中列表中
            }
        }
    }
}
#endif