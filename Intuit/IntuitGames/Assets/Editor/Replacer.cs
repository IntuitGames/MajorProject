using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;
using System;
using System.Reflection;
using CustomExtensions;public class Replacer : EditorWindow{
    GameObject replacementObject;
    string[] selectionModes = { "Current Selection", "Starts With...", "Contains Component" };

    string startsWith;
    string[] componentTypes;
    string targetComponent;

    bool keepParent = true;
    bool keepPosition = true;
    bool keepRotation = true;
    bool keepScale = true;
    bool keepName = false;
    bool indexNames = true;
    bool linkPrefab = true;

    GameObject[] targetObjects;
    bool targetObjectFoldout;

    int selectionModeIndex;
    int componentTypeIndex;    [MenuItem("Tools/Replacer")]    static void Init()
    {
        EditorWindow.GetWindow<Replacer>().Show();
        EditorWindow.GetWindow<Replacer>().Repaint();
    }    void OnEnable()
    {
        try
        {
            selectionModeIndex = EditorPrefs.GetInt("Replacer-SelectionMode", 0);
            componentTypeIndex = EditorPrefs.GetInt("Replacer-ComponentType", 0);
            startsWith = EditorPrefs.GetString("Replacer-StartsWith", "");
            componentTypes = Assembly.GetAssembly(typeof(Character)).GetTypes().Where(type => type.IsSubclassOf(typeof(MonoBehaviour))).ConvertValid<Type, string>(type => type.Name).ToArray();
            keepParent = EditorPrefs.GetBool("Replacer-KeepParent", true);
            keepPosition = EditorPrefs.GetBool("Replacer-KeepPosition", true);
            keepRotation = EditorPrefs.GetBool("Replacer-KeepRotation", true);
            keepScale = EditorPrefs.GetBool("Replacer-KeepScale", true);
            keepName = EditorPrefs.GetBool("Replacer-KeepName", false);
            indexNames = EditorPrefs.GetBool("Replacer-IndexNames", true);
            linkPrefab = EditorPrefs.GetBool("Replacer-LinkPrefab", true);
            targetObjectFoldout = EditorPrefs.GetBool("Replacer-ShowTargets", false);
        }
        catch (Exception e)
        {
            EditorWindow.GetWindow<Replacer>().Close();
            Debug.LogException(e);
        }
    }    void OnGUI()
    {
        // SETTINGS
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        keepParent = EditorGUILayout.Toggle("Attach To Parent", keepParent);
        keepPosition = EditorGUILayout.Toggle("Keep Position", keepPosition);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        keepRotation = EditorGUILayout.Toggle("Keep Rotation", keepRotation);
        keepScale = EditorGUILayout.Toggle("Keep Scale", keepScale);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        keepName = EditorGUILayout.Toggle("Keep Name", keepName);
        indexNames = EditorGUILayout.Toggle("Index Names", indexNames);
        EditorGUILayout.EndHorizontal();
        linkPrefab = EditorGUILayout.Toggle("Link Prefab", linkPrefab);
        EditorGUILayout.EndVertical();

        // SELECTION
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
        selectionModeIndex = EditorGUILayout.Popup("Selection Mode", selectionModeIndex, selectionModes);

        switch (selectionModeIndex)
        {
            // Current Selection
            case 0:
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("New"))
                    TargetAdd(Selection.gameObjects.Where(go => go.activeInHierarchy).ToArray(), false);
                else if (GUILayout.Button("Add"))
                    TargetAdd(Selection.gameObjects.Where(go => go.activeInHierarchy).ToArray(), true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Replacer will target all objects that are currently selected.", MessageType.Info);
                break;

            // Starts With...
            case 1:
                startsWith = EditorGUILayout.TextField("Starts With...", startsWith);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("New"))
                    TargetAdd(targetObjects = GameObject.FindObjectsOfType<Transform>().Where(go => go.name.StartsWith(startsWith))
                        .ConvertValid<Transform, GameObject>(trans => trans.gameObject).ToArray(), false);
                else if (GUILayout.Button("Add"))
                    TargetAdd(targetObjects = GameObject.FindObjectsOfType<Transform>().Where(go => go.name.StartsWith(startsWith))
                        .ConvertValid<Transform, GameObject>(trans => trans.gameObject).ToArray(), true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Replacer will target all objects whose name starts with what is specified above.", MessageType.Info);
                break;

            // Contain Component
            case 2:
                componentTypeIndex = EditorGUILayout.Popup("Component Type", componentTypeIndex, componentTypes);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("New"))
                    TargetAdd(GameObject.FindObjectsOfType<Transform>().Where(go =>
                        {
                            try
                            { return go.GetComponent(componentTypes[componentTypeIndex]); }
                            catch
                            { return false; }
                        }).ConvertValid<Transform, GameObject>(trans => trans.gameObject).ToArray(), false);
                else if (GUILayout.Button("Add"))
                    TargetAdd(GameObject.FindObjectsOfType<Transform>().Where(go =>
                    {
                        try
                        { return go.GetComponent(componentTypes[componentTypeIndex]); }
                        catch
                        { return false; }
                    }).ConvertValid<Transform, GameObject>(trans => trans.gameObject).ToArray(), true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Replacer will target all objects that contain the specified component.", MessageType.Info);
                break;
        }
        EditorGUILayout.EndVertical();

        // TARGETS
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Targets", EditorStyles.boldLabel);
        GUI.enabled = !targetObjects.IsNullOrEmpty();
        if (GUILayout.Button("Clear"))
            targetObjects = null;
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (targetObjects.IsNullOrEmpty())
            EditorGUILayout.HelpBox("No objects are targeted to be replaced.", MessageType.Warning);
        else
        {
            targetObjectFoldout = EditorGUILayout.Foldout(targetObjectFoldout, string.Format("Target Objects ({0})", targetObjects.Length));

            if (targetObjectFoldout)
            {
                GUI.enabled = false;
                EditorGUI.indentLevel++;
                for (int i = 0; i < targetObjects.Length; i++)
                    EditorGUILayout.ObjectField("Object " + i, targetObjects[i], typeof(GameObject), true);
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }
        }

        EditorGUILayout.Separator();
        Color normalColor = GUI.backgroundColor;
        GUI.backgroundColor = replacementObject ? new Color(0.3f, 1, 0.3f) : new Color(1, 0.3f, 0.3f);
        replacementObject = (GameObject)EditorGUILayout.ObjectField("Replacement Prefab", replacementObject, typeof(GameObject), false);
        GUI.backgroundColor = normalColor;
        EditorGUILayout.Separator();

        GUI.enabled = !targetObjects.IsNullOrEmpty() && replacementObject;
        if (GUILayout.Button("Replace", GUILayout.Height(30)) && !targetObjects.IsNullOrEmpty() && replacementObject)
        {
            Replace(targetObjects, replacementObject, keepParent, keepPosition, keepRotation, keepScale, keepName, indexNames, linkPrefab);
        }
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }    void OnDisable()
    {
        try
        {
            EditorPrefs.SetInt("Replacer-SelectionMode", selectionModeIndex);
            EditorPrefs.SetInt("Replacer-ComponentType", componentTypeIndex);
            EditorPrefs.SetString("Replacer-StartsWith", startsWith);
            EditorPrefs.SetBool("Replacer-KeepParent", keepParent);
            EditorPrefs.SetBool("Replacer-KeepPosition", keepPosition);
            EditorPrefs.SetBool("Replacer-KeepRotation", keepRotation);
            EditorPrefs.SetBool("Replacer-KeepScale", keepScale);
            EditorPrefs.SetBool("Replacer-KeepName", keepName);
            EditorPrefs.SetBool("Replacer-IndexNames", indexNames);
            EditorPrefs.SetBool("Replacer-LinkPrefab", linkPrefab);
            EditorPrefs.SetBool("Replacer-ShowTargets", targetObjectFoldout);
        }
        catch (Exception e)
        {
            EditorWindow.GetWindow<Replacer>().Close();
            Debug.LogException(e);
        }
    }    private void TargetAdd(GameObject[] newObjects, bool add)
    {
        if (!add || targetObjects.IsNullOrEmpty())
            targetObjects = newObjects;
        else
            targetObjects = newObjects.Concat(targetObjects).Distinct().ToArray();
    }    private void Replace(GameObject[] objectsToReplace, GameObject cloneObject, bool parent, bool position, bool rotation, bool scale, bool names, bool indexNames, bool link)
    {
        if (EditorUtility.DisplayDialog("Are you sure?", string.Format("You are about to replace {0} gameobjects. {1}{1}This action can NOT be undone!", objectsToReplace.Length, Environment.NewLine), "Replace", "Cancel"))
        {
            int indexSuccess = 1;
            for (int i = 0; i < objectsToReplace.Length; i++)
            {
                // Ignore nulls (In case a parent was destroyed in the process)
                if (!objectsToReplace[i]) continue;

                Transform newObject = link ?
                    (PrefabUtility.InstantiatePrefab(cloneObject) as GameObject).transform :
                    Instantiate(cloneObject).transform;

                // Transform settings
                if (parent) newObject.SetParent(objectsToReplace[i].transform.parent);

                if (position) newObject.localPosition = objectsToReplace[i].transform.localPosition;
                else newObject.localPosition = cloneObject.transform.localPosition;

                if (rotation) newObject.localRotation = objectsToReplace[i].transform.localRotation;
                else newObject.localRotation = cloneObject.transform.localRotation;

                if (scale) newObject.localScale = objectsToReplace[i].transform.localScale;
                else newObject.localScale = cloneObject.transform.localScale;

                if (names) newObject.name = objectsToReplace[i].name;
                else newObject.name = cloneObject.name;

                if (indexNames) newObject.name += " " + indexSuccess++;

                // Destroy object
                DestroyImmediate(objectsToReplace[i], false);

                // Reset targets
                targetObjects = null;
            }
        }
    }}