using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;[CustomEditor(typeof(Character))]public class CharacterEditor : Editor{
    private Character Target;
    private SerializedProperty property;
    bool showChildren;

    public void OnEnable()
    {
        Target = (Character)target;

        property = serializedObject.GetIterator();
    }

    public override void OnInspectorGUI()
    {
        if (!Target) return;

        // Script fields
        EditorGUI.indentLevel = 0;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUILayout.ObjectField("Editor Script", MonoScript.FromScriptableObject(this), this.GetType(), false);

        // Iterate through properties and draw them like it normally would
        property.Reset();
        property.Next(true);
        do
        {
            if (property.name.StartsWith("m_")) continue; // Ignore Unity properties
            showChildren = EditorGUILayout.PropertyField(property); // Draw property
            
        }
        while (property.NextVisible(showChildren));

        if (Application.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.Separator();
            EditorGUILayout.Toggle("Airborne", Target.isAirborne);
            GUI.enabled = true;
        }

        // Apply changes to the property
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfDirtyOrScript();
    }}