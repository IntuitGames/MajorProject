using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private GameManager Target;

    private bool showManagers;

    public void OnEnable()
    {
        Target = (GameManager)target;
    }

    public override void OnInspectorGUI()
    {
        if (!Target) return;

        // Script fields
        EditorGUI.indentLevel = 0;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUILayout.ObjectField("Editor Script", MonoScript.FromScriptableObject(this), this.GetType(), false);

        showManagers = EditorGUILayout.Foldout(showManagers, "Sub Managers");
        if (showManagers)
        {
            EditorGUI.indentLevel++;
            DrawManagerField<InputManager>(GameManager.InputManager, "Input Manager");
            DrawManagerField<ModeManager>(GameManager.ModeManager, "Mode Manager");
            DrawManagerField<AudioManager>(GameManager.AudioManager, "Audio Manager");
            DrawManagerField<TetherManager>(GameManager.TetherManager, "Tether Manager");
            DrawManagerField<PlayerManager>(GameManager.PlayerManager, "Player Manager");
            DrawManagerField<CameraManager>(GameManager.CameraManager, "Camera Manager");
            EditorGUI.indentLevel--;
        }
    }    public void DrawManagerField<T>(T manager, string label) where T: Manager
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        GUI.enabled = false;
        EditorGUILayout.ObjectField(manager, typeof(T), true);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }}