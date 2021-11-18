using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorUtils 
{
    public class Header : MonoBehaviour
    {
        public enum HeaderType
        {
            Default, Dotted, Filled, Custom
        }

        public enum HeaderAlignment
        {
            Start, Center, End
        }

        public string title = "Header";
        [HideInInspector] public HeaderType type;
        [HideInInspector] public HeaderAlignment alignment;
        [HideInInspector] public Color fillColor;
 
        private void OnDrawGizmos() 
        {
            transform.position = Vector3.zero;
        }

        
    } 
}
