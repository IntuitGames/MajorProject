﻿using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
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
            GUI.enabled = true;
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.Vector3Field("Real Velocity", Target.characterController.velocity);
            EditorGUILayout.Slider("Move Speed", Mathf.Round(Target.characterController.velocity.magnitude * 100) / 100, 0, Target.maxSpeed);
            EditorGUILayout.Toggle("Walking", Target.isWalking);
            EditorGUILayout.Toggle("Airborne", Target.isAirborne);
            EditorGUILayout.Toggle("Falling", Target.isFalling);
            EditorGUILayout.Toggle("Dashing", Target.isDashing);
            EditorGUILayout.Toggle("Heavy", Target.isHeavy);
            EditorGUILayout.Toggle("Bouncing", Target.isBouncing);
            GUI.enabled = true;
        }

        // Apply changes to the property
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfDirtyOrScript();
    }}