using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;

[CustomEditor(typeof(ModeManager))]
public class ModeManagerEditor : Editor
{
    private ModeManager Target;

    public void OnEnable()
    {
        Target = (ModeManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Current game mode
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Game Mode");
        GUI.enabled = false;
        EditorGUILayout.EnumPopup(Target.currentGameMode);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        // Previous game mode
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Previous Game Mode");
        GUI.enabled = false;
        EditorGUILayout.EnumPopup(Target.previousGameMode);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }
}