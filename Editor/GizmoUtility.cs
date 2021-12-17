using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Multiple functions to draw utility gizmos
public static class GizmoUtility
{
    public static void DrawWireCapsule(Vector3 pos, Quaternion rot, float radius, float height, Color color = default(Color))
    {
        if (color != default(Color))
            Handles.color = color;
        Matrix4x4 angleMatrix = Matrix4x4.TRS(pos, rot, Handles.matrix.lossyScale);
        using (new Handles.DrawingScope(angleMatrix))
        {
            var pointOffset = (height - (radius * 2)) / 2;

            //draw sideways
            Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, radius);
            Handles.DrawLine(new Vector3(0, pointOffset, -radius), new Vector3(0, -pointOffset, -radius));
            Handles.DrawLine(new Vector3(0, pointOffset, radius), new Vector3(0, -pointOffset, radius));
            Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, radius);
            //draw frontways
            Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, radius);
            Handles.DrawLine(new Vector3(-radius, pointOffset, 0), new Vector3(-radius, -pointOffset, 0));
            Handles.DrawLine(new Vector3(radius, pointOffset, 0), new Vector3(radius, -pointOffset, 0));
            Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, radius);
            //draw center
            Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, radius);
            Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, radius);
        }
    }

    public static void DrawArrow(Vector3 start, Vector3 end, Color color = default(Color))
    {
        Vector3[] arrowHead = new Vector3[3];
        Vector3[] arrowLine = new Vector3[2];

        Vector3 forward = (end - start).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        float size = HandleUtility.GetHandleSize(end);
        float width = size * 0.1f;
        float height = size * 0.3f;

        arrowHead[0] = end;
        arrowHead[1] = end - forward * height + right * width;
        arrowHead[2] = end - forward * height - right * width;

        arrowLine[0] = start;
        arrowLine[1] = end - forward * height;

        using (new Handles.DrawingScope(color))
        {
            Handles.DrawAAPolyLine(arrowLine);
            Handles.DrawAAConvexPolygon(arrowHead);
        }
    }

    public static Vector3 SceneMousePosition(out Vector3 normal)
    {
        Vector3 mousePosition = Event.current.mousePosition;
        var placeObject = HandleUtility.PlaceObject(mousePosition, out var newPosition, out normal);
        return placeObject ? newPosition : HandleUtility.GUIPointToWorldRay(mousePosition).GetPoint(10);
    }

    public static Vector3 SceneMousePosition()
    {
        Vector3 mousePosition = Event.current.mousePosition;
        var placeObject = HandleUtility.PlaceObject(mousePosition, out var newPosition, out var normal);
        return placeObject ? newPosition : HandleUtility.GUIPointToWorldRay(mousePosition).GetPoint(10);
    }
}

public static class CustomHandles
{
    // Internal State Variables
    private static Vector2 handleMouseStart;
    private static Vector2 handleMouseCurrent;
    private static Vector3 handleWorldStart;
    private static float handleClickTime = 0;
    private static int handleClickID;
    private static float handleDoubleClickInterval = 0.5f;
    private static bool handleHasMoved;
    public enum HandleEventResult
    {
        none = 0,
        LMBPress,
        LMBClick,
        LMBDoubleClick,
        LMBDrag,
        LMBRelease,

        RMBPress,
        RMBClick,
        RMBDoubleClick,
        RMBDrag,
        RMBRelease,
    };

    public static int lastHandleID;

    public static bool ButtonHandle(int controlID, Vector3 position, float handleSize, Handles.CapFunction cap)
    {
        bool held = false;
        Event evt = Event.current;
        switch (evt.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if ((HandleUtility.nearestControl == controlID && evt.button == 0) && GUIUtility.hotControl == 0)
                {
                    // Grab focus of control
                    GUIUtility.hotControl = controlID;
                    held = true;
                    evt.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
                {
                    GUIUtility.hotControl = 0;
                    held = false;
                    evt.Use();
                    EditorGUIUtility.SetWantsMouseJumping(0);
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    held = true;
                    GUI.changed = true;
                    evt.Use();
                }
                break;
            case EventType.MouseMove:
                if (GUIUtility.hotControl == controlID)
                    HandleUtility.Repaint();
                break;
            case EventType.Repaint:
                // Selected
                if (GUIUtility.hotControl == controlID)
                {
                    Handles.color = Handles.selectedColor;
                }
                // Hovered
                else if (controlID == HandleUtility.nearestControl && GUIUtility.hotControl == 0)
                {
                    Handles.color = Handles.preselectionColor;
                }

                cap(controlID, position, Quaternion.identity, handleSize, EventType.Repaint);
                Handles.color = Color.white;

                break;
            case EventType.Layout:
                cap(controlID, position, Quaternion.identity, handleSize, EventType.Layout);
                break;
        }

        return held;
    }

    public static Vector3 DragHandle(int controlID, Vector3 position, float handleSize,
        Handles.CapFunction cap, Color handleColor)
    {

        Event evt = Event.current;
        switch (evt.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if ((HandleUtility.nearestControl == controlID && evt.button == 0) && GUIUtility.hotControl == 0)
                {
                    // Grab focus of control
                    GUIUtility.hotControl = controlID;
                    handleMouseCurrent = handleMouseStart = evt.mousePosition;
                    handleWorldStart = position;
                    evt.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
                {
                    GUIUtility.hotControl = 0;
                    evt.Use();
                    EditorGUIUtility.SetWantsMouseJumping(0);
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    handleMouseCurrent += new Vector2(evt.delta.x, -evt.delta.y);
                    Vector3 tempPos = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(handleWorldStart))
                        + (Vector3)(handleMouseCurrent - handleMouseStart);

                    position = Handles.matrix.inverse.MultiplyPoint(Camera.current.ScreenToWorldPoint(tempPos));

                    if (Camera.current.transform.forward == Vector3.forward || Camera.current.transform.forward == -Vector3.forward)
                        position.z = handleWorldStart.z;
                    if (Camera.current.transform.forward == Vector3.up || Camera.current.transform.forward == -Vector3.up)
                        position.y = handleWorldStart.y;
                    if (Camera.current.transform.forward == Vector3.right || Camera.current.transform.forward == -Vector3.right)
                        position.x = handleWorldStart.x;
                    
                    GUI.changed = true;
                    evt.Use();
                }
                break;
            case EventType.MouseMove:
                if (GUIUtility.hotControl == controlID)
                    HandleUtility.Repaint();
                break;
            case EventType.Repaint:
                // Selected
                if (GUIUtility.hotControl == controlID)
                {
                    Handles.color = Handles.selectedColor;
                }
                // Hovered
                else if (controlID == HandleUtility.nearestControl && GUIUtility.hotControl == 0)
                {
                    Handles.color = Handles.preselectionColor;
                }

                cap(controlID, position, Quaternion.identity, handleSize, EventType.Repaint);
                Handles.color = Color.white;

                break;
            case EventType.Layout:
                cap(controlID, position, Quaternion.identity, handleSize, EventType.Layout);
                break;
        }

        return position;
    }
}
