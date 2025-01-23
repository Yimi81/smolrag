#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using System.IO;

public class BatchScreenshotEditor : EditorWindow
{
    private Vector3 cameraDefaultRotation = new Vector3(15f, -45f, 0f); // 默认视图相机角度
    private Vector3 cameraFrontRotation = new Vector3(15f, 0f, 0f); // 正视图相机角度
    private Vector3 cameraTopRotation = new Vector3(45f, 0f, 0f); // 顶视图相机角度

    [MenuItem("ZTResource/批量截图", false, 2)]
    public static void ShowWindow()
    {
        GetWindow<BatchScreenshotEditor>("批量截图");
    }

    private void OnGUI()
    {
        GUILayout.Label("截图设置", EditorStyles.boldLabel);

        cameraDefaultRotation = EditorGUILayout.Vector3Field("默认视图角度 (XYZ)", cameraDefaultRotation);
        cameraFrontRotation = EditorGUILayout.Vector3Field("正视图角度 (XYZ)", cameraFrontRotation);
        cameraTopRotation = EditorGUILayout.Vector3Field("顶视图角度 (XYZ)", cameraTopRotation);

        if (GUILayout.Button("开始截图"))
        {
            StartCaptureScreenshots();
        }
    }

    private void StartCaptureScreenshots()
    {
        GameObject parentObject = GameObject.Find("Target"); // 替换为实际父物体名称
        if (parentObject == null)
        {
            Debug.LogError("Parent object not found. Please ensure the parent object name is correct.");
            return;
        }

        EditorCoroutineUtility.StartCoroutine(CaptureScreenshotsCoroutine(parentObject), this);
    }

    private IEnumerator CaptureScreenshotsCoroutine(GameObject parentObject)
    {
        // 查找并隐藏 reference_role 物体
        GameObject referenceRole = GameObject.Find("reference_role");
        if (referenceRole != null)
        {
            referenceRole.SetActive(false);
        }
        else
        {
            Debug.LogError("reference_role not found.");
        }

        // 获取 Main_Camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main_Camera not found.");
            yield break;
        }

        // 保存相机的原始 Field of View、位置和旋转
        float originalFOV = cam.fieldOfView;
        Vector3 originalPosition = cam.transform.position;
        Quaternion originalRotation = cam.transform.rotation;

        // 设置相机 Field of View 为 14
        cam.fieldOfView = 14;

        int width = 512;
        int height = 512;
        int totalChildren = parentObject.transform.childCount;

        SetAllChildrenActive(parentObject, false);

        for (int i = 0; i < totalChildren; i++)
        {
            GameObject child = parentObject.transform.GetChild(i).gameObject;
            child.SetActive(true);

            yield return new WaitForEndOfFrame();

            MeshRenderer[] meshRenderers = child.GetComponentsInChildren<MeshRenderer>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = child.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (meshRenderers.Length == 0 && skinnedMeshRenderers.Length == 0)
            {
                Debug.LogError($"子物体 {child.name} 没有MeshRenderer或SkinnedMeshRenderer组件。");
                child.SetActive(false);
                continue;
            }

            // 批量截图
            RenderAndSaveBatchScreenshot(child, cam, width, height);

            // 三视图截图
            string prefabFolder = CreateFolderForPrefab(child);
            RenderAndSaveScreenshot(child, cam, width, height, cameraDefaultRotation, Path.Combine(prefabFolder, $"{child.name}_Default.png"));
            RenderAndSaveScreenshot(child, cam, width, height, cameraFrontRotation, Path.Combine(prefabFolder, $"{child.name}_Front.png"));
            RenderAndSaveScreenshot(child, cam, width, height, cameraTopRotation, Path.Combine(prefabFolder, $"{child.name}_Top.png"));

            child.SetActive(false);

            float progress = (float)i / totalChildren;
            EditorUtility.DisplayProgressBar("截图进度", "正在截图: " + child.name, progress);

            yield return new WaitForEndOfFrame();
        }

        EditorUtility.ClearProgressBar();

        // 恢复 reference_role 物体
        if (referenceRole != null)
        {
            referenceRole.SetActive(true);
        }

        // 恢复相机的 Field of View、位置和旋转
        cam.fieldOfView = originalFOV;
        cam.transform.position = originalPosition;
        cam.transform.rotation = originalRotation;

        EditorUtility.DisplayDialog("截图完毕", "所有截图已完成！", "确定");

        Debug.Log("Screenshots taken for all children under " + parentObject.name);
    }



    private void RenderAndSaveBatchScreenshot(GameObject child, Camera cam, int width, int height)
    {
        // 设置相机默认角度
        cam.transform.rotation = Quaternion.Euler(cameraDefaultRotation);

        // 设置相机背景为黑色（非透明）
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black; // 黑色背景

        Bounds bounds = GetTotalBounds(child);
        float distance = bounds.extents.magnitude / Mathf.Sin(Mathf.Deg2Rad * cam.fieldOfView / 2f);
        Vector3 cameraPosition = bounds.center - cam.transform.forward * distance;

        cam.transform.position = cameraPosition;
        cam.transform.LookAt(bounds.center);

        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false); // 使用RGB24格式，不带透明通道
        cam.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        byte[] bytes = screenshot.EncodeToPNG();

        // 获取Prefab路径
        string prefabPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(child));
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError($"未找到子物体 {child.name} 的Prefab路径。");
            return;
        }

        // 去除路径中的 "Assets/ArtResource/Scenes/Standard/" 部分，并删除 ".prefab" 后缀
        string relativePath = prefabPath.Replace("Assets/ArtResource/Scenes/Standard/", "").Replace(".prefab", "").Replace("/", "");

        // 设置保存文件路径
        string filename = string.Format("Assets/ZTResource/Resources/ZT_IconTextures/{0}.png", relativePath);
        System.IO.File.WriteAllBytes(filename, bytes);

        // 设置图片导入属性为Sprite
        SetTextureImporterSettings(filename);

        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
    }

    private void RenderAndSaveScreenshot(GameObject child, Camera cam, int width, int height, Vector3 rotation, string filepath)
    {
        cam.transform.rotation = Quaternion.Euler(rotation);

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0); // 设置背景为透明

        Bounds bounds = GetTotalBounds(child);
        float distance = bounds.extents.magnitude / Mathf.Sin(Mathf.Deg2Rad * cam.fieldOfView / 2f);
        Vector3 cameraPosition = bounds.center - cam.transform.forward * distance;

        cam.transform.position = cameraPosition;
        cam.transform.LookAt(bounds.center);

        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false); // 使用RGBA32格式，带透明通道
        cam.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filepath, bytes);

        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
    }

    private string CreateFolderForPrefab(GameObject child)
    {
        string prefabName = child.name;
        string folderPath = $"Assets/ZTResource/Resources/Al_NeedsTextures/{prefabName}";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        return folderPath;
    }

    private void SetTextureImporterSettings(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
        }
    }

    private Bounds GetTotalBounds(GameObject go)
    {
        MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();

        if (meshRenderers.Length == 0 && skinnedMeshRenderers.Length == 0)
        {
            return new Bounds(go.transform.position, Vector3.zero);
        }

        Bounds bounds = new Bounds(go.transform.position, Vector3.zero);

        foreach (var renderer in meshRenderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        foreach (var skinnedRenderer in skinnedMeshRenderers)
        {
            bounds.Encapsulate(skinnedRenderer.bounds);
        }

        return bounds;
    }

    private void SetAllChildrenActive(GameObject parent, bool state)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(state);
        }
    }
}
#endif