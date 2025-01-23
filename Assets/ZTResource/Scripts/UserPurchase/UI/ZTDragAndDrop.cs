using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ZTDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private bool isDragging = false;
    private float holdTime = 0.2f;  // 按住0.2秒
    private Coroutine holdCoroutine;
    private HistoryCardUI historyCardUI;
    private BatchUI originalBatchUI;
    private BatchUI targetBatchUI;

    void Start()
    {
        // 获取Canvas组件
        canvas = GetComponentInParent<Canvas>();

        // 确保对象上有CanvasGroup组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 获取HistoryCardUI组件
        historyCardUI = GetComponent<HistoryCardUI>();
        // 获取原始批次的BatchUI组件
        originalBatchUI = GetComponentInParent<BatchUI>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        holdCoroutine = StartCoroutine(StartHoldTimer());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    private IEnumerator StartHoldTimer()
    {
        yield return new WaitForSeconds(holdTime);
        isDragging = true;
        originalParent = transform.parent;
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 仅当isDragging为true时才开始拖动
        if (!isDragging)
        {
            eventData.pointerDrag = null;
            return;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
        transform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        GameObject dropZone = GetDropZoneUnderPointer(eventData);

        if (dropZone != null && dropZone.CompareTag("DropZone"))
        {
            targetBatchUI = dropZone.GetComponentInParent<BatchUI>();
            if (targetBatchUI != null && historyCardUI != null)
            {
                string resourceId = historyCardUI.idText.text;
                string targetBatchTime = targetBatchUI.batchTimeText.text;

                string csvFilePath = historyCardUI.csvFilePath;

                // 直接移动资源到目标批次
                CSVParser.MoveResourceToBatch(csvFilePath, resourceId, targetBatchTime);

                // 将资源卡UI移动到新的批次中
                transform.SetParent(dropZone.transform, false);
            }
        }
        else
        {
            transform.SetParent(originalParent, false);
        }
    }

    private GameObject GetDropZoneUnderPointer(PointerEventData eventData)
    {
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject.CompareTag("DropZone"))
            {
                return result.gameObject;
            }
        }

        return null;
    }
}
