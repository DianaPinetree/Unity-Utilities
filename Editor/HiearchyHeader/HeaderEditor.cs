using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace EditorUtils
{
    [CustomEditor(typeof(Header))]
    public class HeaderEditor : Editor
    {
        private bool titleChanged;
        private double lastChangedTime;
        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        } 
        private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;
        public void OnUndoRedo() => UpdateHeader(target as Header, null, true);
        
        public static void UpdateAllHeaders()
        {
            var targetType = HeaderSettings.HeaderType;
            var targetAlignment = HeaderSettings.Alignment;
            var allHeader = GameObject.FindObjectsOfType<Header>();
            foreach (var header in allHeader)
            {
                header.type = targetType;
                header.alignment = targetAlignment;

                HeaderEditor.UpdateHeader(header, null, true);
            }
        }

        public static void UpdateHeader(Header header, string title = null, bool markAsDirty = false)
        {
            var targetTitle = title == null ? header.title : title;

            header.name = GetFormattedTitle(targetTitle);
            
            if (markAsDirty)
            {
                EditorUtility.SetDirty(header);
            }
        }

        public static string GetSimpleTitle(string prefix, string title)
        {
            if (prefix == null || prefix.Length <= 0) return title;
            return GetSimpleTitle(prefix[0], title);
        }

        private static string GetSimpleTitle(char prefix, string title)
        {
            int maxCharLength = HeaderSettings.MaxLength;
            int charLength = maxCharLength - title.Length;

            int leftSize = 0;
            int rightSize = 0;
            switch (HeaderSettings.Alignment)
            {
                case Header.HeaderAlignment.Start:
                    leftSize = HeaderSettings.MinLength;
                    rightSize = charLength - leftSize;
                    break;
                case Header.HeaderAlignment.End:
                    rightSize = HeaderSettings.MinLength;
                    leftSize = charLength - rightSize;
                    break;
                case Header.HeaderAlignment.Center:
                    leftSize = charLength / 2;
                    rightSize = charLength / 2;
                    break;
            }

            string left = leftSize > 0 ? new string(prefix, leftSize) : "";
            string right = rightSize > 0 ? new string(prefix, rightSize) : "";

            var builder = new StringBuilder();
            builder.Append(left);
            builder.Append(" ");
            builder.Append(title.ToUpper());
            builder.Append(" ");
            builder.Append(right);

            return builder.ToString();
        }

        private static string GetFormattedTitle(string title)
        {
            switch (HeaderSettings.HeaderType)
            {
                case Header.HeaderType.Dotted:
                    return GetSimpleTitle('-', title);
                case Header.HeaderType.Custom:
                    return GetSimpleTitle(HeaderSettings.CustomPrefix, title);
                case Header.HeaderType.Filled:
                    return ">";
            }
            return GetSimpleTitle('━', title);
        }

        public override void OnInspectorGUI()
        {
            var type = serializedObject.FindProperty("type");
            var header = target as Header;
            serializedObject.Update();

            var titleProp = serializedObject.FindProperty("title");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(titleProp);
            if (HeaderSettings.HeaderType == Header.HeaderType.Filled)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fillColor"));
            }
            if(EditorGUI.EndChangeCheck())
            {
                UpdateHeader(header, titleProp.stringValue, false);
                EditorApplication.RepaintHierarchyWindow();
            }

            if((Header.HeaderType)type.enumValueIndex != HeaderSettings.HeaderType)
            {
                type.enumValueIndex = (int)HeaderSettings.HeaderType;

                UpdateHeader(header, null, false);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                if(GUILayout.Button("Options"))
                {
                    SettingsService.OpenProjectSettings("Project/Tools/Hierarchy Header");
                }
                
                if(GUILayout.Button("Refresh"))
                {
                    UpdateAllHeaders();
                }
                
                if(GUILayout.Button("Create Empty"))
                {
                    GameObject obj = new GameObject("New Empty");
                    obj.transform.SetSiblingIndex(header.transform.GetSiblingIndex() + 1);
                    Undo.RegisterCreatedObjectUndo(obj, "Hierarchy Header/Create Empty");
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        
    }
}
