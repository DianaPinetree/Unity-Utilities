using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

public class AlignMoveTool : EditorTool
{
    private static bool stick;
    private static bool align;
    private static bool stickUpright;
    private static int alignMode;

    private RaycastHit hit;
    bool drawTargetNormal = false;
    Vector3 newPos;
    Vector3 normal = Vector3.zero;
    Vector3 scenePos = Vector3.zero;
    private void OnEnable()
    {

    }

    public override GUIContent toolbarIcon => base.toolbarIcon;

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView))
            return;

        if (!ToolManager.IsActiveTool(this))
            return;

        if (Selection.activeGameObject == null)
            return;

        Transform t = Selection.activeGameObject.transform;
        Handles.color = Color.blue;
        Handles.DrawWireArc(t.position, t.up, t.right, 360, 0.25f);
        Handles.color = Color.red;
        Handles.DrawLine(t.position, t.right * 0.35f + t.position);

        newPos = t.position;

        // While this handle is being held down
        if (CustomHandles.ButtonHandle(t.GetHashCode(), t.position, 0.2f, Handles.SphereHandleCap))
        {
            // Get scene mouse data
            scenePos = GizmoUtility.SceneMousePosition(out normal);

            if (stick)
            {
                newPos = scenePos;
            }
            if (align)
            {
                switch (alignMode)
                {
                    case 0:
                        t.right = normal;
                        break;
                    case 1:
                        t.right = -normal;
                        break;
                    case 2:
                        t.up = normal;
                        break;
                    case 3:
                        t.up = -normal;
                        break;
                    case 4:
                        t.forward = normal;
                        break;
                    case 5:
                        t.forward = -normal;
                        break;
                }
            }
        }

        t.position = newPos;

        // if (align)
        // {
        //     Handles.color = Color.yellow;
        //     Handles.DrawWireArc(scenePos, normal, t.right, 360, 0.1f);
        //     Handles.DrawLine(scenePos, scenePos + normal * 0.1f);
        // }

        if (stickUpright)
        {
            t.up = Vector3.up;
        }
    }

    public static void SetMode(bool shouldStick, bool shouldAlign, bool upright, int alignAxis)
    {
        stick = shouldStick;
        align = shouldAlign;
        stickUpright = upright;
        alignMode = alignAxis;
    }
}
