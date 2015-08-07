using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;
using CustomExtensions;
using System;

/// <summary>
/// Creates a popup for a property. (Currently only supports booleans LOL)
/// </summary>
public class PopupAttribute : PropertyAttribute
{
    public string OverrideName { get; set; }
    public string[] DisplayedOptions { get; set; }

    public PopupAttribute(string[] displayedOptions)
    {
        if (displayedOptions.IsNullOrEmpty())
            throw new NullReferenceException();

        DisplayedOptions = displayedOptions;
    }
}

[CustomPropertyDrawer(typeof(PopupAttribute))]
public class PopupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Determine the display name
        string displayName = (attribute as PopupAttribute).OverrideName ?? label.text;
        GUIContent displayPrefix = label;
        displayPrefix.text = displayName;

        // For some reason the prefix position is a little off
        Rect prefixPosition = position;
        prefixPosition.x += 2;
        prefixPosition.y += 1;

        if (ReadOnlyDrawer.IsReadOnly(fieldInfo)) GUI.enabled = false;

        switch(property.propertyType)
        {
            case SerializedPropertyType.Boolean:
                EditorGUI.PrefixLabel(prefixPosition, displayPrefix, property.prefabOverride ? EditorStyles.boldLabel : GUIStyle.none);
                property.boolValue = EditorGUI.Popup(position, " ", property.boolValue ? 0 : 1, (attribute as PopupAttribute).DisplayedOptions) == 0;
                break;
            default:
                break;
        }

        GUI.enabled = true;
    }
}