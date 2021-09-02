using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class SelectedObject
{
    public bool locked = false;
    public UnityEngine.Object theObject = null;
}

[System.Serializable]
public class SelectGroup
{
    public string name;
    //public int[] selectionIDs;
    public UnityEngine.Object[] selectionObjs;
}

public class SSHistory : EditorWindow
{
    [SerializeField]
    List<SelectedObject> selectedObjects = new List<SelectedObject>();
    [SerializeField]
    List<SelectedObject> favoriteObjects = new List<SelectedObject>();

    [SerializeField]
    Vector2 scrollPosition = Vector2.zero;

    [SerializeField]
    int maxHistory = 20;

    //---- Group Vars
    [SerializeField]
    string newGroupName = "";

    [SerializeField]
    List<SelectGroup> selectionGroups = new List<SelectGroup>();


    [System.NonSerialized]
    GUIStyle lockButtonStyle;

    [SerializeField]
    int toolbarInt = 0;
    [SerializeField]
    string[] toolbarStrings = { "Favorites", "History", "Groups" };

    [MenuItem("Tools/SSHistory")]
    public static void ShowWindow()
    {
        //show existing window instance, if one doesn't exist, make one
        EditorWindow.GetWindow<SSHistory>("Selection History");
    }

    void OnSelectionChange()
    {
        UpdateSelection();
    }

    void OnGUI()
    {
        lockButtonStyle = "IN LockButton";

        //GUILayout.Space(10);
        toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);

        TextAnchor defaultAnchor = GUI.skin.label.alignment;

        if (toolbarInt == 0)
        {
            //------ FAVORITES

            GUILayout.Space(10);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            /*GUILayout.BeginHorizontal();                    
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Favorites", GUILayout.ExpandWidth(false));
            GUI.skin.label.alignment = defaultAnchor;        
            GUILayout.EndHorizontal();*/

            //GUILayout.Space(5);

            if (favoriteObjects.Count > 0)
            {
                for (int i = 0; i < favoriteObjects.Count; i++)
                {
                    LayoutSelectionItem(i, favoriteObjects[i]);
                }
            }

            EditorGUILayout.EndScrollView();
        }
        else if (toolbarInt == 1)
        {
            //------ HISTORY

            GUILayout.Space(10);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginHorizontal();

            //GUILayout.Label("History", GUILayout.ExpandWidth(false));
            //GUILayout.FlexibleSpace();        

            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            float defaultLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 30;
            maxHistory = EditorGUILayout.IntField("Max", maxHistory, GUILayout.ExpandWidth(false));
            EditorGUIUtility.labelWidth = defaultLabelWidth;
            GUI.skin.label.alignment = defaultAnchor;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.MaxWidth(120)))
            {
                selectedObjects.Clear();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (selectedObjects.Count > 0)
            {
                for (int i = 0; i < selectedObjects.Count; i++)
                {
                    LayoutSelectionItem(i, selectedObjects[i]);
                }
            }

            EditorGUILayout.EndScrollView();
        }
        else if (toolbarInt == 2)
        {
            //----- GROUPS

            GUILayout.Space(10);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginHorizontal();

            //GUILayout.Label("History", GUILayout.ExpandWidth(false));
            //GUILayout.FlexibleSpace();        

            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            float defaultLabelWidth = EditorGUIUtility.labelWidth;
            float fieldDefaultWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.labelWidth = 90;
            EditorGUIUtility.fieldWidth = 120;
            newGroupName = EditorGUILayout.TextField("Group Name", newGroupName, GUILayout.ExpandWidth(false));
            EditorGUIUtility.labelWidth = defaultLabelWidth;
            GUI.skin.label.alignment = defaultAnchor;
            EditorGUIUtility.fieldWidth = fieldDefaultWidth;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Group", EditorStyles.miniButton, GUILayout.MaxWidth(120)))
            {
                AddSelectionGroup();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (selectionGroups.Count > 0)
            {
                for (int i = 0; i < selectionGroups.Count; i++)
                {
                    LayoutGroupElement(i, selectionGroups[i]);
                }
            }

            EditorGUILayout.EndScrollView();
        }

    }

    private void AddSelectionGroup()
    {
        SelectGroup sel = new SelectGroup();
        sel.name = (newGroupName == "") ? ("Group " + selectionGroups.Count) : newGroupName;
        newGroupName = "";
        //sel.selectionIDs = Selection.instanceIDs;
        //sel.name = sel.name + "  (" + sel.selectionIDs.Length + ")";
        sel.selectionObjs = Selection.objects;
        sel.name = sel.name + "  (" + sel.selectionObjs.Length + ")";

        selectionGroups.Add(sel);
    }

    private void LayoutSelectionItem(int i, SelectedObject p)
    {
        if (p != null)
        {
            if (p.theObject == null) return;

            GUILayout.BeginHorizontal();

            p.locked = EditorGUILayout.Toggle(p.locked, lockButtonStyle, GUILayout.MaxWidth(20));

            if (p.locked)
            {
                if (!favoriteObjects.Contains(p))
                {
                    favoriteObjects.Add(p);
                }
                else if (selectedObjects.Contains(p))
                {
                    selectedObjects.RemoveAt(selectedObjects.IndexOf(p));
                }
            }
            else
            {
                if (favoriteObjects.Contains(p))
                {
                    favoriteObjects.Remove(p);
                }
            }

            // Use "right arrow" unicode character  \u25B6
            //up     U+25B2
            //down   U+25BC

            if (GUILayout.Button("\u25B2", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(20)))
            {
                if (favoriteObjects.Contains(p))
                {
                    int index = favoriteObjects.IndexOf(p);
                    if (index != 0)
                    {
                        favoriteObjects.RemoveAt(index);
                        favoriteObjects.Insert(index - 1, p);
                    }
                }
                else
                {
                    int index = selectedObjects.IndexOf(p);
                    if (index != 0)
                    {
                        selectedObjects.RemoveAt(index);
                        selectedObjects.Insert(index - 1, p);
                    }
                }
            }

            if (GUILayout.Button(p.theObject.name, EditorStyles.miniButtonMid))
            {
                Selection.activeObject = p.theObject;
                EditorGUIUtility.PingObject(p.theObject);
            }

            // Use "right arrow" unicode character
            if (GUILayout.Button("Ping", EditorStyles.miniButtonRight, GUILayout.MaxWidth(50)))
            {
                EditorGUIUtility.PingObject(p.theObject);
            }

            GUILayout.EndHorizontal();
        }
    }

    private void LayoutGroupElement(int i, SelectGroup s)
    {
        if (s != null)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("\u25B2", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(20)))
            {
                int index = selectionGroups.IndexOf(s);
                selectionGroups.RemoveAt(index);
                selectionGroups.Insert(index - 1, s);
            }

            if (GUILayout.Button(s.name, EditorStyles.miniButtonMid))
            {
                Selection.objects = s.selectionObjs;
                //EditorGUIUtility.PingObject(p.theObject);
            }

            // Use "right arrow" unicode character
            if (GUILayout.Button("\u2716", EditorStyles.miniButtonRight, GUILayout.MaxWidth(30)))
            {
                //EditorGUIUtility.PingObject(p.theObject);
                selectionGroups.RemoveAt(selectionGroups.IndexOf(s));
            }

            GUILayout.EndHorizontal();
        }
    }

    private void UpdateSelection()
    {
        SelectedObject sel = new SelectedObject();
        sel.theObject = Selection.activeObject;

        foreach (SelectedObject obj in selectedObjects)
        {
            if (obj.theObject == sel.theObject) return;
        }

        selectedObjects.Insert(0, sel);

        if (selectedObjects.Count > maxHistory)
        {
            selectedObjects.RemoveAt(selectedObjects.Count - 1);
        }

        Repaint();
    }
}
