using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;

/// <summary>
/// Property is grayed out and is only shown NOT editable.
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute
{
    public bool EditableWhilePlaying { get; set; }
    public bool EditableInEditor { get; set; }
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (Application.isPlaying)
        {
            if (!(attribute as ReadOnlyAttribute).EditableWhilePlaying)GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
        else
        {
            if (!(attribute as ReadOnlyAttribute).EditableInEditor) GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}