﻿using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PopupAttribute))]
public class PopupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Determine the display name
        string displayName = (attribute as PopupAttribute).OverrideName ?? label.text;
        GUIContent displayPrefix = label;
        displayPrefix.text = displayName;
        string[] displayedOptions = (attribute as PopupAttribute).DisplayedOptions;

        // For read only attribute compatibility
        if (ReadOnlyDrawer.IsReadOnly(fieldInfo)) GUI.enabled = false;

        EditorGUI.BeginProperty(position, label, property);
        switch (property.propertyType)
        {
            case SerializedPropertyType.Boolean:
                property.boolValue = EditorGUI.Popup(position, displayPrefix.text, property.boolValue ? 0 : 1, displayedOptions) == 0;
                break;
            case SerializedPropertyType.String:
                int selectedIndex = displayedOptions.Contains(property.stringValue) ? displayedOptions.ToList().IndexOf(property.stringValue) : 0;
                property.stringValue = displayedOptions[EditorGUI.Popup(position, displayPrefix.text, selectedIndex, displayedOptions)];
                break;
            default:
                break;
        }
        EditorGUI.EndProperty();

        GUI.enabled = true;
    }
}