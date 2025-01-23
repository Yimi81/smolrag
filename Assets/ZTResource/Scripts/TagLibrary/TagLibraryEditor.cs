#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TagLibrary))]
public class TagLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TagLibrary tagLibrary = (TagLibrary)target;

        tagLibrary.category = EditorGUILayout.TextField("Category", tagLibrary.category);

        SerializedProperty tagsProperty = serializedObject.FindProperty("tags");
        EditorGUILayout.PropertyField(tagsProperty, new GUIContent("Tags"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
