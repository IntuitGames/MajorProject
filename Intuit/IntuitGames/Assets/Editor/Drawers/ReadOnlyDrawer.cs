using UnityEngine;
using UnityEditor;using System.Collections;using System.Collections.Generic;using System.Linq;
using System.Reflection;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (Application.isPlaying)
        {
            if (!(attribute as ReadOnlyAttribute).EditableWhilePlaying) GUI.enabled = false;
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

    /// <summary>
    /// From a field info determine if it is read only.
    /// </summary>
    /// <param name="fieldMember"></param>
    /// <returns></returns>
    public static bool IsReadOnly(FieldInfo fieldMember)
    {
        ReadOnlyAttribute ROAtt = (ReadOnlyAttribute)fieldMember.GetCustomAttributes(typeof(ReadOnlyAttribute), false).FirstOrDefault();
        if (ROAtt == null)
            return false;
        else
            if (Application.isPlaying)
                return !ROAtt.EditableWhilePlaying;
            else
                return !ROAtt.EditableInEditor;
    }
}