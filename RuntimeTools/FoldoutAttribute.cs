using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Attributes 
{
    public class FoldoutAttribute : PropertyAttribute
    {
        // Foldout group name
        public string name;
        // Forces all properties into the foldout
        public bool foldEverything;

        public FoldoutAttribute(string name, bool foldEverything = false)
        {
            this.foldEverything = foldEverything;
            this.name = name;
        }
    } 
}
