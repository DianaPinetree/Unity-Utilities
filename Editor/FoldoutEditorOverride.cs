using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using Object = UnityEngine.Object;

namespace Utilities.Attributes
{
    [CustomEditor(typeof(Object), true, isFallback = true)]
	[CanEditMultipleObjects]
	public class EditorOverride : Editor
	{

		Dictionary<string, CacheFoldProp> cacheFolds = new Dictionary<string, CacheFoldProp>();
		List<SerializedProperty> props = new List<SerializedProperty>();
		List<MethodInfo> methods = new List<MethodInfo>();
		bool initialized;

		void OnEnable()
		{
			initialized = false;
		}


		void OnDisable()
		{
			if (target != null)
				foreach (var c in cacheFolds)
				{
					EditorPrefs.SetBool(string.Format($"{c.Value.atr.name}{c.Value.props[0].name}{target.GetInstanceID()}"), c.Value.expanded.value);
					c.Value.Dispose();
				}
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();


			Setup();

			if (props.Count == 0)
			{
				DrawDefaultInspector();
				return;
			}

			Header();
			Body();

			serializedObject.ApplyModifiedProperties();

			void Header()
			{
				using (new EditorGUI.DisabledScope("m_Script" == props[0].propertyPath))
				{
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(props[0], true);
					EditorGUILayout.Space();
				}
			}

			void Body()
			{
				foreach (var pair in cacheFolds)
				{
					Foldout(pair.Value);
					EditorGUI.indentLevel = 0;
				}

				EditorGUILayout.Space();

				for (var i = 1; i < props.Count; i++)
				{
					EditorGUILayout.PropertyField(props[i], true);
				}

				EditorGUILayout.Space();

				if (methods == null) return;
				foreach (MethodInfo memberInfo in methods)
				{
					this.UseButton(memberInfo);
				}
			}

			void Foldout(CacheFoldProp cache)
			{
				
                Rect layout = EditorGUILayout.BeginVertical(StyleFramework.box);
                // Background texture
                GUI.DrawTexture(layout, EditorGUIUtility.whiteTexture, 
                    ScaleMode.StretchToFill, 
                    true, 0, 
                    StyleFramework.mainColor, 0f, 4f);
                
                Rect header = EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(cache.atr.name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(StyleFramework.Foldout(cache.expanded.target));
                if (GUI.Button(header, GUIContent.none, GUIStyle.none))
                {
                    cache.expanded.target = !cache.expanded.target;
                }
                EditorGUILayout.EndHorizontal();

				if (EditorGUILayout.BeginFadeGroup(cache.expanded.faded))
				{
                    EditorGUILayout.Space();

					EditorGUI.indentLevel = 1;

					for (int i = 0; i < cache.props.Count; i++)
					{
						EditorGUILayout.PropertyField(cache.props[i], new GUIContent(ObjectNames.NicifyVariableName(cache.props[i].name)), true);
					}
				}
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndVertical();
            }

			void Setup()
			{
				if (!initialized)
				{
					//	SetupButtons();

					List<FieldInfo>  objectFields;
					FoldoutAttribute prevFold = default;

					var length = EditorTypes.Get(target, out objectFields);

					for (var i = 0; i < length; i++)
					{
						#region FOLDERS

						var           fold = Attribute.GetCustomAttribute(objectFields[i], typeof(FoldoutAttribute)) as FoldoutAttribute;
						CacheFoldProp c;
						if (fold == null)
						{
							if (prevFold != null && prevFold.foldEverything)
							{
								if (!cacheFolds.TryGetValue(prevFold.name, out c))
								{
                                    c = new CacheFoldProp {atr = prevFold, types = new HashSet<string> {objectFields[i].Name}};
                                    c.expanded.valueChanged.AddListener(Repaint);
									cacheFolds.Add(prevFold.name, c);
								}
								else
								{
									c.types.Add(objectFields[i].Name);
								}
							}

							continue;
						}

						prevFold = fold;

						if (!cacheFolds.TryGetValue(fold.name, out c))
						{
							var expanded = EditorPrefs.GetBool(string.Format($"{fold.name}{objectFields[i].Name}{target.GetInstanceID()}"), false);
                            c = new CacheFoldProp {atr = fold, types = new HashSet<string> {objectFields[i].Name}};
							c.expanded.value = expanded;
                            c.expanded.valueChanged.AddListener(Repaint);

                            cacheFolds.Add(fold.name, c);
						}
						else c.types.Add(objectFields[i].Name);

						#endregion
					}

					var property = serializedObject.GetIterator();
					var next     = property.NextVisible(true);
					if (next)
					{
						do
						{
							HandleFoldProp(property);
						} while (property.NextVisible(false));
					}

					initialized = true;
				}
			}
		}

		public void HandleFoldProp(SerializedProperty prop)
		{
			bool shouldBeFolded = false;

			foreach (var pair in cacheFolds)
			{
				if (pair.Value.types.Contains(prop.name))
				{
					var pr = prop.Copy();
					shouldBeFolded = true;
					pair.Value.props.Add(pr);

					break;
				}
			}

			if (shouldBeFolded == false)
			{
				var pr = prop.Copy();
				props.Add(pr);
			}
		}

		class CacheFoldProp
		{
			public HashSet<string> types = new HashSet<string>();
			public List<SerializedProperty> props = new List<SerializedProperty>();
			public FoldoutAttribute atr;
			public AnimBool expanded = new AnimBool(false);

			public void Dispose()
			{
                expanded.valueChanged.RemoveAllListeners();
				props.Clear();
				types.Clear();
				atr = null;
			}
		}
	}

	static class ditorUIHelper
	{
		public static void UseVerticalLayout(this Editor e, Action action, GUIStyle style)
		{
			EditorGUILayout.BeginVertical(style);
			action();
			EditorGUILayout.EndVertical();
		}

		public static void UseButton(this Editor e, MethodInfo m)
		{
			if (GUILayout.Button(m.Name))
			{
				m.Invoke(e.target, null);
			}
		}
	}


	static class StyleFramework
	{
        public static GUIStyle box;
        private static GUIContent foldoutArrowOpen;
        private static GUIContent foldoutArrowClosed;
        public static Color mainColor;
        public static Color headerColor;
		static StyleFramework()
		{
            var pro = EditorGUIUtility.isProSkin;
            mainColor = pro ? new Color(0.1f, 0.1f, 0.1f, 0.5f) : new Color(0.2f, 0.2f, 0.2f, 0.5f);
            headerColor = new Color(0.3f, 0.3f, 0.3f, 1f);

            foldoutArrowClosed = EditorGUIUtility.IconContent("d_icon dropdown");
            foldoutArrowOpen = EditorGUIUtility.IconContent("d_forward");

            box = new GUIStyle();
            box.padding = new RectOffset(8, 8, 8, 8);
            box.margin = new RectOffset(0, 5, 0, 0);
        }

	    public static GUIContent Foldout(bool state)
        {
            return state ? foldoutArrowClosed : foldoutArrowOpen;
        }

		public static IList<Type> GetTypeTree(this Type t)
		{
			var types = new List<Type>();
			while (t.BaseType != null)
			{
				types.Add(t);
				t = t.BaseType;
			}

			return types;
		}
	}

	static class EditorTypes
	{
		public static Dictionary<int, List<FieldInfo>> fields = new Dictionary<int, List<FieldInfo>>(FastComparable.Default);

		public static int Get(Object target, out List<FieldInfo> objectFields)
		{
			var t    = target.GetType();
			var hash = t.GetHashCode();

			if (!fields.TryGetValue(hash, out objectFields))
			{
				var typeTree = t.GetTypeTree();
				objectFields = target.GetType()
						.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic)
						.OrderByDescending(x => typeTree.IndexOf(x.DeclaringType))
						.ToList();
				fields.Add(hash, objectFields);
			}

			return objectFields.Count;
		}
	}


	class FastComparable : IEqualityComparer<int>
	{
		public static FastComparable Default = new FastComparable();

		public bool Equals(int x, int y)
		{
			return x == y;
		}

		public int GetHashCode(int obj)
		{
			return obj.GetHashCode();
		}
	}
}