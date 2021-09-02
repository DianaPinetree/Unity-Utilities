using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinMaxSliderAttribute minMaxAttrib = (MinMaxSliderAttribute)attribute;
        var propertyType = property.propertyType;

        label.tooltip = minMaxAttrib.Min.ToString("F2") + " to " + minMaxAttrib.Max.ToString("F2");

        Rect controlRect = EditorGUI.PrefixLabel(position, label);

        Rect[] splittedRect = SplitRect(controlRect, 3);

        if (propertyType == SerializedPropertyType.Vector2)
        {
            EditorGUI.BeginChangeCheck();

            Vector2 vector = property.vector2Value;
            float minVal = vector.x;
            float maxVal = vector.y;

            minVal = EditorGUI.FloatField(splittedRect[0], float.Parse(minVal.ToString("F2")));
            maxVal = EditorGUI.FloatField(splittedRect[2], float.Parse(maxVal.ToString("F2")));

            EditorGUI.MinMaxSlider(splittedRect[1], ref minVal, ref maxVal,
                minMaxAttrib.Min, minMaxAttrib.Max);

            if (minVal < minMaxAttrib.Min)
            {
                minVal = minMaxAttrib.Min;
            }

            if (minVal > minMaxAttrib.Max)
            {
                minVal = minMaxAttrib.Max;
            }

            vector = new Vector2(minVal > maxVal ? maxVal : minVal, maxVal);

            if (EditorGUI.EndChangeCheck())
            {
                property.vector2Value = vector;
            }
        } 
        else if (propertyType == SerializedPropertyType.Vector2Int)
        {

            EditorGUI.BeginChangeCheck();

            Vector2Int vector = property.vector2IntValue;
            
            float minVal = vector.x;
            float maxVal = vector.y;

            minVal = EditorGUI.FloatField(splittedRect[0], float.Parse(minVal.ToString("F2")));
            maxVal = EditorGUI.FloatField(splittedRect[2], float.Parse(maxVal.ToString("F2")));

            EditorGUI.MinMaxSlider(splittedRect[1], ref minVal, ref maxVal,
                minMaxAttrib.Min, minMaxAttrib.Max);

            if (minVal < minMaxAttrib.Min)
            {
                minVal = minMaxAttrib.Min;
            }

            if (minVal > minMaxAttrib.Max)
            {
                minVal = minMaxAttrib.Max;
            }

            vector = new Vector2Int(Mathf.FloorToInt(minVal > maxVal ? maxVal : minVal), Mathf.FloorToInt(maxVal));

            if (EditorGUI.EndChangeCheck())
            {
                property.vector2IntValue = vector;
            }
        }
    }

    private Rect[] SplitRect(Rect rectToSplit, int n)
    {
        Rect[] rects = new Rect[n];
        
        for (int i = 0; i < n; i++)
        {
            rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n), rectToSplit.position.y, rectToSplit.width / n, rectToSplit.height);
        }

        int padding = (int)rects[0].width - 40;
        int space = 5;

        rects[0].width -= padding + space;
        rects[2].width -= padding + space;

        rects[1].x -= padding;
        rects[1].width += padding * 2;

        rects[2].x += padding + space;

        return rects;
    }
}
