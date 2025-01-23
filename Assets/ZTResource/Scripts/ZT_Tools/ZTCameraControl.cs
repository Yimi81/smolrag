using UnityEngine;
using UnityEngine.EventSystems;

public class ZTCameraControl : MonoBehaviour
{
    // 添加比较用的人物模型引用
    public GameObject comparisonObject;
    public GameObject targetObject; // 观察的目标物体
    public RectTransform uiPanel; // 用于调整相机Viewport的UI Panel
    public float rotateSpeed = 5.0f; // 旋转速度
    public float panSpeed = 0.5f; // 平移速度
    public float zoomSpeed = 2.0f; // 缩放速度
    public float focusMultiplier = 1.0f; // 默认倍数为1

    private Vector3 targetCenter; // 目标的视觉中心
    private float distanceToTarget; // 相机到目标的距离

    private int lastChildCount = -1; // 用于存储上一次检查时的子物体数量

    void Start()
    {
        FocusOnTarget();
        AdjustCameraViewport();
    }

    void Update()
    {
        // 检查目标对象是否存在
        if (targetObject == null) return;

        // 检测子物体数量是否有变化
        int currentChildCount = targetObject.transform.childCount;
        if (currentChildCount != lastChildCount)
        {
            FocusOnTarget();
            lastChildCount = currentChildCount; // 更新记录的子物体数量
        }

        // 每帧调整相机视口
        AdjustCameraViewport();
    }

    void LateUpdate()
    {
        if (targetObject == null) return;

        // 检查当前是否有UI元素在处理事件，以防止在UI交互时控制相机
        if (EventSystem.current.IsPointerOverGameObject()) return; // 对于鼠标输入
                                                                   // 对于触摸输入，您需要检查所有的触摸点
        foreach (Touch touch in Input.touches)
        {
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
        }

        // 左键拖动 - 旋转
        if (Input.GetMouseButton(0))
        {
            float horizontal = Input.GetAxis("Mouse X") * rotateSpeed;
            float vertical = Input.GetAxis("Mouse Y") * rotateSpeed;

            transform.RotateAround(targetCenter, Vector3.up, horizontal);
            transform.RotateAround(targetCenter, transform.right, -vertical);
        }

        // 右键拖动 - 平移
        if (Input.GetMouseButton(1))
        {
            float h = Input.GetAxis("Mouse X") * panSpeed;
            float v = Input.GetAxis("Mouse Y") * panSpeed;

            transform.Translate(-h, -v, 0);
        }

        // 滚轮滑动 - 缩放

        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        // 使用已声明的变量来处理正交相机和透视相机的逻辑
        if (Camera.main.orthographic)
        {
            Camera.main.orthographicSize -= scroll;
            Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize, 0.1f);
        }
        else
        {
            transform.Translate(0, 0, scroll, Space.Self);
        }

        // 按F聚焦到目标
        if (Input.GetKeyDown(KeyCode.F))
        {
            FocusOnTarget();
        }
    }

    public void ToggleCameraMode()
    {
        // 检查相机当前的投影模式，并切换到另一种模式
        if (Camera.main.orthographic)
        {
            // 从正交模式切换到透视模式
            Camera.main.orthographic = false;
            Camera.main.fieldOfView = 27; // 或者任何适合您场景的FOV值
        }
        else
        {
            // 从透视模式切换到正交模式
            Camera.main.orthographic = true;
            Vector3 currentCameraPosition = Camera.main.transform.position;
            Vector3 currentCameraRotation = Camera.main.transform.eulerAngles;

            // 计算正交相机大小
            float distance = (targetCenter - Camera.main.transform.position).magnitude;
            Camera.main.orthographicSize = distance * Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2);

            // 调整相机位置和角度以保持视角不变
            Camera.main.transform.position = currentCameraPosition;
            Camera.main.transform.rotation = Quaternion.Euler(currentCameraRotation);
        }
    }

    public void FocusOnTarget()
    {
        if (targetObject == null)
        {
            return;
        }

        // 获取目标物体及其所有子物体中带有MeshRenderer组件的物体
        MeshRenderer[] meshRenderers = targetObject.GetComponentsInChildren<MeshRenderer>(true);
        if (meshRenderers.Length == 0)
        {
            return;
        }

        // 初始化bounds以计算视觉中心
        Bounds bounds = new Bounds();
        bool hasBounds = false; // 用于标记是否找到了第一个有效的bounds

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            if (hasBounds)
            {
                bounds.Encapsulate(meshRenderer.bounds);
            }
            else
            {
                bounds = meshRenderer.bounds;
                hasBounds = true;
            }
        }

        if (!hasBounds)
        {
            Debug.LogError("Unable to find bounds for Target Object.");
            return;
        }

        targetCenter = bounds.center;

        // 根据视觉中心的位置和大小调整相机的位置
        float objectSize = bounds.extents.magnitude;
        distanceToTarget = objectSize / Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2) * focusMultiplier;
        transform.position = targetCenter - transform.forward * distanceToTarget;

        if (comparisonObject != null)
        {
            // 计算并更新人物模型在X和Z轴上的位置，保持Y轴位置不变
            Vector3 edgePosition = new Vector3(
                bounds.max.x, // 或者bounds.min.x，取决于您希望人物模型站在物体的哪一边
                comparisonObject.transform.position.y, // 保持当前的Y轴位置
                targetCenter.z // 您可以选择保持在物体中心的Z位置，或者根据需要调整
            );

            comparisonObject.transform.position = edgePosition;
        }

        // 根据相机是否为正交相机，应用不同的逻辑
        if (Camera.main.orthographic)
        {
            // 正交相机模式下聚焦
            Camera.main.orthographicSize = objectSize * focusMultiplier;
            //transform.position = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);

        }
        else
        {
            // 透视相机模式下聚焦
            distanceToTarget = objectSize / Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2) * focusMultiplier;
            //transform.position = targetCenter - transform.forward * distanceToTarget;
        }
        transform.LookAt(targetCenter);
    }

    private void AdjustCameraViewport()
    {
        if (uiPanel == null || Camera.main == null) return;

        // 获取UIPanel在屏幕空间的矩形
        Vector3[] worldCorners = new Vector3[4];
        uiPanel.GetWorldCorners(worldCorners);

        // 获取屏幕尺寸
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 计算Viewport Rect的参数
        float x = worldCorners[0].x / screenWidth;
        float y = worldCorners[0].y / screenHeight;
        float width = (worldCorners[2].x - worldCorners[0].x) / screenWidth;
        float height = (worldCorners[2].y - worldCorners[0].y) / screenHeight;

        // 设置相机的Viewport Rect
        Camera.main.rect = new Rect(x, y, width, height);
    }
}