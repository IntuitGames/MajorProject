using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;[CustomEditor(typeof(Character)), CanEditMultipleObjects()]public class CharacterEditor : Editor{
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
        if (!serializedObject.isEditingMultipleObjects)
            DrawSingleCharacter();
        else
            DrawMultipleCharacters();
    }

    private void DrawSingleCharacter()
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
            EditorGUI.indentLevel = property.depth;
            if (property.name.StartsWith("m_")) continue; // Ignore Unity properties
            showChildren = EditorGUILayout.PropertyField(property); // Draw property
        }
        while (property.NextVisible(showChildren));
        EditorGUI.indentLevel = 0;

        if (Application.isPlaying)
        {
            GUI.enabled = true;
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.Vector3Field("Real Velocity", Target.rigidbodyComp.velocity);
            EditorGUILayout.Slider("Move Speed", Mathf.Round(Target.rigidbodyComp.velocity.magnitude * 100) / 100, 0, Target.maxSpeed);
            EditorGUILayout.Slider("Slope", Target.slopeAngle, 0, 90);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Toggle("Weakened", Target.isWeakened);
            EditorGUILayout.Toggle("Walking", Target.isWalking);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Toggle("Sprinting", Target.isSprinting);
            EditorGUILayout.Toggle("Grounded", Target.isGrounded);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Toggle("Falling", Target.isFalling);
            EditorGUILayout.Toggle("Dashing", Target.isDashing);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Toggle("Heavy", Target.isHeavy);
            EditorGUILayout.Toggle("Bouncing", Target.isBouncing);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Toggle("Dash Jumping", Target.isDashJumping);
            EditorGUILayout.Toggle("Heavy High Jump", Target.isHeavyHighJump);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Toggle("Frozen", Target.isFreeze);
            EditorGUILayout.Toggle("Knocked Back", Target.isKnockedBack);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Toggle("Suspended", Target.isSuspended);
            EditorGUILayout.Toggle("Sliding", Target.isSliding);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        // Apply changes to the property
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfDirtyOrScript();
    }

    private void DrawMultipleCharacters()
    {
        // Script fields
        EditorGUI.indentLevel = 0;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUILayout.ObjectField("Editor Script", MonoScript.FromScriptableObject(this), this.GetType(), false);
        
        // Iterate through properties and draw them like it normally would
        property.Reset();
        property.Next(true);
        do
        {
            EditorGUI.indentLevel = property.depth;
            if (property.name.StartsWith("m_")) continue; // Ignore Unity properties
            if (property.name == "_isPlayerOne")
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Basic", EditorStyles.boldLabel);
                continue;
            }
            showChildren = EditorGUILayout.PropertyField(property); // Draw property

        }
        while (property.NextVisible(showChildren));
        EditorGUI.indentLevel = 0;

        // Apply changes to the property
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfDirtyOrScript();
    }}