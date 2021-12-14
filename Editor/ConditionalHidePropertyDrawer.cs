using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utilities.Attributes 
{
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalHideAttribute cond = (ConditionalHideAttribute)attribute;
    
            bool enabled = GetConditionalHideAttributeResult(cond, property);
    
            bool wasEnabled = GUI.enabled;
            GUI.enabled = enabled;
    
            if (!cond.hideInInspector || enabled)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
    
            GUI.enabled = wasEnabled;
        }
    
        private bool GetConditionalHideAttributeResult(ConditionalHideAttribute cond, SerializedProperty property)
        {
            bool enabled = true;
            //Look for the sourcefield within the object that the property belongs to
            string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
            string conditionPath = propertyPath.Replace(property.name, cond.sourceField); //changes the path to the conditionalsource property path
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
    
            if (sourcePropertyValue != null)
            {
                enabled = sourcePropertyValue.boolValue;
            }
            else
            {
                Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + cond.sourceField);
            }
    
            return enabled;
        }
    } 
}
