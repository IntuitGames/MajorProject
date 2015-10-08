﻿using UnityEngine;
using UnityEditor;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(!Application.isPlaying && GUILayout.Button("Rebuild"))
        {
            (target as TetherManager).Rebuild();
        }

        if (!Application.isPlaying && GUILayout.Button("Reset"))
        {
            (target as TetherManager).Reset();
        }

        if (!Application.isPlaying && GUILayout.Button("Clear"))
        {
            GameObject.FindObjectsOfType<GameObject>().Where(x => x.hideFlags != HideFlags.None).ToList().ForEach(x => DestroyImmediate(x));
        }

        if(!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("A rebuild is recommended when a change is made to the joint count, joint prefab or when the start / end point moves.", MessageType.Info, true);
        }
    }