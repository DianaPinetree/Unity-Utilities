using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace EditorUtils
{
    public class HeaderSettings : ScriptableObject
    {
        private const string settingsPath = "Assets/HeaderSettings.asset";
        [SerializeField]
        private Header.HeaderType headerType;
        [SerializeField]
        private Header.HeaderAlignment alignment;
        [SerializeField]
        private string customPrefix;
        [SerializeField]
        private int maxLength;
        [SerializeField]
        private int minLength;
        
        private static HeaderSettings instance;
        public static Header.HeaderType HeaderType 
        {
            get 
            {
                if (instance == null) instance = GetOrCreateSettings();

                return instance.headerType;
            }
        } 
        public static Header.HeaderAlignment Alignment
        {
            get 
            {
                if (instance == null) instance = GetOrCreateSettings();

                return instance.alignment;
            }
        }

        public static int MaxLength 
        {
            get
            {
                if (instance == null) instance = GetOrCreateSettings();

                return instance.maxLength;
            }
        }

        public static int MinLength 
        {
            get
            {
                if (instance == null) instance = GetOrCreateSettings();

                return instance.minLength;
            }
        }

        public static string CustomPrefix 
        {
            get
            {
                if (instance == null) instance = GetOrCreateSettings();

                return instance.customPrefix;
            }
        }


        internal static HeaderSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<HeaderSettings>(settingsPath);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<HeaderSettings>();
                settings.hideFlags = HideFlags.HideInInspector;
                settings.customPrefix = "#";
                settings.alignment = Header.HeaderAlignment.Center;
                settings.maxLength = 5;
                settings.minLength = 5;
                AssetDatabase.CreateAsset(settings, settingsPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    public class HeaderSettingsProviderRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateHeaderSettingsRegister()
        {
            SettingsProvider provider = new SettingsProvider("Project/Hierarchy Headers", SettingsScope.Project)
            {
                label = "Hierarchy Headers",

                guiHandler = (ctx) =>
                {
                    var settings = HeaderSettings.GetSerializedSettings();
                    settings.Update();
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.LabelField("Header Display Settings");
                    EditorGUILayout.PropertyField(settings.FindProperty("headerType"));
                    if ((Header.HeaderType)settings.FindProperty("headerType").enumValueIndex == Header.HeaderType.Custom)
                    {
                        EditorGUILayout.PropertyField(settings.FindProperty("customPrefix"));
                    }
                    EditorGUILayout.PropertyField(settings.FindProperty("alignment"));
                    EditorGUILayout.PropertyField(settings.FindProperty("maxLength"));
                    EditorGUILayout.PropertyField(settings.FindProperty("minLength"));

                    if (EditorGUI.EndChangeCheck())
                    {
                        settings.ApplyModifiedProperties();
                    }
                },

                keywords = new HashSet<string>(new []{"Header", "Hierarchy", "Editor", "Tool"}),
            };
            
            return provider;
        }
    }
}