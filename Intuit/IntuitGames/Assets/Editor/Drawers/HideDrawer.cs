using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(HideAttribute))]
public class HideDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (Application.isPlaying && (attribute as HideAttribute).ShowInPlayMode ||
            !Application.isPlaying && (attribute as HideAttribute).ShowInEditor)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUILayout.PropertyField(property, label, property.hasVisibleChildren);
            EditorGUI.EndProperty();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
}