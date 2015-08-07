using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;[CustomEditor(typeof(Character))]public class CharacterEditor : Editor{
    private Character Target;
    private SerializedProperty property;
    private const SerializedPropertyType nonEnterChildrenTypes = SerializedPropertyType.Vector2 | SerializedPropertyType.Vector3 | SerializedPropertyType.Vector4;

    public void OnEnable()
    {
        Target = (Character)target;

        property = serializedObject.GetIterator();
    }

    public override void OnInspectorGUI()
    {
        if (!Target) return;

        // Script fields
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUILayout.ObjectField("Editor Script", MonoScript.FromScriptableObject(this), this.GetType(), false);
        //EditorGUILayout.Separator();

        // Is player 1 shown as popup instead of bool
        //serializedObject.FindProperty("_isPlayerOne").boolValue = EditorGUILayout.Popup("Player", Target.isPlayerOne ? 0 : 1, new string[2] { "Player 1", "Player 2" }) == 0;

        // Iterate through properties and draw them like it normally would
        property.Reset();
        property.Next(true);
        do
        {
            if (property.name.StartsWith("m_")) continue; // Ignore Unity properties
            EditorGUILayout.PropertyField(property); // Draw property
        }
        while (property.NextVisible((property.propertyType & nonEnterChildrenTypes) == nonEnterChildrenTypes)); // Move to the next property if possible

        // Apply changes to the property
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfDirtyOrScript();
    }}