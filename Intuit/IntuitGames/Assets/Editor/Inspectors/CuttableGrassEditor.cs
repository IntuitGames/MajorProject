using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using CustomExtensions;

[CustomEditor(typeof(CuttableGrass), true), CanEditMultipleObjects]
public class CuttableGrassEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (!serializedObject.isEditingMultipleObjects)
            base.OnInspectorGUI();

        if (GUILayout.Button("Randomize"))
        {
            foreach (var cuttableGrass in targets)
                (cuttableGrass as CuttableGrass).Randomize();
        }

        if (GUILayout.Button("Reset"))
        {
            foreach (var cuttableGrass in targets)
                (cuttableGrass as CuttableGrass).Reset();
        }
    }}