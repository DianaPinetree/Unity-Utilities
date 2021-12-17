using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEditor.EditorTools;

[CustomEditor(typeof(Transform), true)]
[CanEditMultipleObjects]
public class TransformEditorOverride : Editor
{
    private readonly string[] alignAxisOptions = { "X", "-X", "Y", "-Y", "Z", "-Z" };
    Editor transformDefaultEditor;
    Transform transform;
    Type lastActiveTool;
    [SerializeField] private int alignAxisSelected = 2;
    private bool stickUpright;
    private bool alignUpright;
    private bool stick, align;
    private bool cameraInControl;
    private void OnEnable()
    {
        transformDefaultEditor = Editor.CreateEditor(targets, Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
        transform = target as Transform;
        transform.hasChanged = false;
    }

    private void OnDisable()
    {
        //When OnDisable is called, the default editor we created should be destroyed to avoid memory leakage.
        //Also, make sure to call any required methods like OnDisable
        MethodInfo disableMethod = transformDefaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (disableMethod != null)
            disableMethod.Invoke(transformDefaultEditor, null);
        DestroyImmediate(transformDefaultEditor);
    }

    public override void OnInspectorGUI()
    {
        // Show default transform inspector
        transformDefaultEditor.OnInspectorGUI();
        // Horizontal buttons
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

        stick = GUILayout.Toggle(stick, "Stick",
            EditorStyles.miniButtonLeft);

        stickUpright = GUILayout.Toggle(stickUpright, "Upright",
            EditorStyles.miniButtonRight);

        align = GUILayout.Toggle(align, "Align",
            EditorStyles.miniButtonLeft);

        alignAxisSelected = EditorGUILayout.Popup(alignAxisSelected, alignAxisOptions,
            EditorStyles.miniButtonMid, GUILayout.MaxWidth(23));

        alignUpright = GUILayout.Toggle(alignUpright, "Upright",
            EditorStyles.miniButtonRight);
        EditorGUILayout.EndHorizontal();

        if (stick || align)
        {
            if (ToolManager.activeToolType != typeof(AlignMoveTool))
            {
                lastActiveTool = ToolManager.activeToolType;
                ToolManager.SetActiveTool<AlignMoveTool>();

                AlignMoveTool.SetMode(stick, align, stickUpright, alignAxisSelected);
            }
        }
        else if (ToolManager.activeToolType == typeof(AlignMoveTool))
        {
            ToolManager.SetActiveTool(lastActiveTool);
        }
    }

    private void OnSceneGUI()
    {
        Event current = Event.current;
        if (current.type == EventType.MouseDown)
        {
            if (current.button == 1)
                cameraInControl = true;
        }
        else if (current.type == EventType.MouseUp)
        {
            if (current.button == 1)
            {
                cameraInControl = false;
            }
        }

        if (cameraInControl || current.modifiers != EventModifiers.None) return;
        switch (current.type)
        {
            case EventType.KeyDown:
                if (current.keyCode == KeyCode.S)
                {
                    stick = true;
                    AlignMoveTool.SetMode(stick, align, stickUpright, alignAxisSelected);
                    current.Use();
                    Repaint();
                }
                else if (current.keyCode == KeyCode.A)
                {
                    align = true;
                    AlignMoveTool.SetMode(stick, align, stickUpright, alignAxisSelected);
                    current.Use();
                    Repaint();
                }
                break;
            case EventType.KeyUp:
                if (current.keyCode == KeyCode.S)
                {
                    stick = false;
                    AlignMoveTool.SetMode(stick, align, stickUpright, alignAxisSelected);
                    current.Use();
                    Repaint();
                }
                if (current.keyCode == KeyCode.A)
                {
                    align = false;
                    AlignMoveTool.SetMode(stick, align, stickUpright, alignAxisSelected);
                    current.Use();
                    Repaint();
                }
                break;
        }

    }
}
