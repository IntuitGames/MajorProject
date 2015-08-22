using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;[CustomEditor(typeof(StaticTetherManager))]public class StaticTetherManagerEditor : Editor{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(!Application.isPlaying && GUILayout.Button("Rebuild"))
        {
            (target as StaticTetherManager).Rebuild();
        }

        if(!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("A rebuild is recommended when a change is made to the joint count, tether clone prefab or joint clone prefab", MessageType.Info, true);
        }
    }}