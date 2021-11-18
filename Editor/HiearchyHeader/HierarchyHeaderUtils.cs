using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
    [InitializeOnLoad]
    public static class HierarchyHeaderUtils
    {
        static HierarchyHeaderUtils()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemGUI;
        }

        private static void HierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            if (HeaderSettings.HeaderType == Header.HeaderType.Filled)
            {
                GameObject header = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

                EditorUtils.Header cmp = null;
                if (header != null && header.TryGetComponent<EditorUtils.Header>(out cmp))
                {
                    GUI.DrawTexture(selectionRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, false, 0, cmp.fillColor, 0, 7);
                    EditorGUI.DropShadowLabel(selectionRect, cmp.title.ToUpperInvariant());
                }
            }
        }

        [MenuItem("GameObject/Create Header", false, 0)]
        private static void CreateHeader()
        {
            GameObject header = new GameObject();
            header.tag = "EditorOnly";
            Header instance = header.AddComponent<Header>();
            header.transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
            instance.fillColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            HeaderEditor.UpdateHeader(instance);

            // Undo
            Undo.RegisterCreatedObjectUndo(header, "Create Header");

            Selection.activeGameObject = header;
        }
    }
}