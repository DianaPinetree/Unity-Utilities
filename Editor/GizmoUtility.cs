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
}
