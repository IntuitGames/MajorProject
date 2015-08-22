using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;[CustomEditor(typeof(StaticTetherManager))]public class StaticTetherManagerEditor : Editor{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(!Application.isPlaying && GUILayout.Button("Rebuild"))
        {
            (target as StaticTetherManager).Rebuild();
        }
    }}