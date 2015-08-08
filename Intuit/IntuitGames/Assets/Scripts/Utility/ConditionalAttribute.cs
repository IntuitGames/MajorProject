using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;
using CustomExtensions;
using System;

/// <summary>
/// Makes the property read-only if the condition is not met.
/// </summary>
public class ConditionalAttribute : PropertyAttribute
{
    public string PropertyName { get; set; }
    public object ConditionalValue { get; set; }

    public ConditionalAttribute(string propertyName, object conditionalValue)
    {
        PropertyName = propertyName; ConditionalValue = conditionalValue;
    }
}

[CustomPropertyDrawer(typeof(ConditionalAttribute))]
public class ConditionalDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string propertyName = (attribute as ConditionalAttribute).PropertyName;
        object conditionalValue = (attribute as ConditionalAttribute).ConditionalValue;
        SerializedProperty propertyObj = property.serializedObject.FindProperty(propertyName);

        EditorGUI.BeginProperty(position, label, property);

        if(propertyObj != null)
        {
            switch(propertyObj.propertyType)
            {
                case SerializedPropertyType.String:
                    if (propertyObj.stringValue != (string)conditionalValue)
                        GUI.enabled = false;
                    break;
                case SerializedPropertyType.Boolean:
                    if (propertyObj.boolValue != (bool)conditionalValue)
                        GUI.enabled = false;
                    break;
                case SerializedPropertyType.Float:
                    if (propertyObj.floatValue != (float)conditionalValue)
                        GUI.enabled = false;
                    break;
                case SerializedPropertyType.Integer:
                    if (propertyObj.intValue != (int)conditionalValue)
                        GUI.enabled = false;
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Unable to find property to compare: " + propertyName);
        }

        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;

        EditorGUI.EndProperty();
    }
}