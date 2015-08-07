using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;[CustomEditor(typeof(PlayerController))]public class PlayerControllerEditor : Editor{
    PlayerController Target;
    SerializedProperty property;

    public enum PlayerNum { Player1, Player2 };
    public PlayerNum Player;

    public void OnEnable()
    {
        Target = (PlayerController)target;

        property = serializedObject.GetIterator();
    }

    public override void OnInspectorGUI()
    {
        if (!Target) return;

        // Script fields
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUILayout.ObjectField("Editor Script", MonoScript.FromScriptableObject(this), this.GetType(), false);
        EditorGUILayout.Space();

        // Is player 1 shown as enum instead of bool
        Player = Target.isPlayerOne ? PlayerNum.Player1 : PlayerNum.Player2;
        Target.isPlayerOne = (PlayerNum)EditorGUILayout.EnumPopup("Player", Player) == PlayerNum.Player1;

        // Iterate through properties and draw them like it normally would
        property.Reset();
        property.Next(true);
        do
        {
            if (property.name.StartsWith("m_")) continue; // Ignore Unity properties
            EditorGUILayout.PropertyField(property); // Draw property
        }
        while (property.NextVisible(property.propertyType != SerializedPropertyType.Vector3)); // Move to the next property if possible

        // Apply changes to the property
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfDirtyOrScript();
    }}