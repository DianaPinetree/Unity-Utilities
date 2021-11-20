using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Utilities.Attributes 
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class ConditionalHideAttribute : PropertyAttribute
    {
        public string sourceField = "";
        public bool hideInInspector = false;
    
        public ConditionalHideAttribute(string sourceField, bool hideInInspector = false)
        {
            this.sourceField = sourceField;
            this.hideInInspector = hideInInspector;
        }
    } 
}
