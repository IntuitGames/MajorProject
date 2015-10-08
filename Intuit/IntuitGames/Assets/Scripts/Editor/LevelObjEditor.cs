using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LevelObj))]
public class LevelObjEditor : Editor 
{
	LevelObj Target;
	SerializedProperty property;
	bool showChild;
	LEVELOBTYPE obType;

	public void OnEnable()
	{
		Target = (LevelObj)target;
		property = serializedObject.GetIterator();
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.indentLevel = 0;
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
		EditorGUILayout.ObjectField("Editor Script", MonoScript.FromScriptableObject(this), this.GetType(), false);
		property.Reset();
		property.Next(true);
		do
		{
			EditorGUI.indentLevel = property.depth;
			if (property.name.StartsWith("m_")) continue; // Ignore Unity properties
			showChild = EditorGUILayout.PropertyField(property); // Draw property
			
		}
		while (property.NextVisible(showChild));

		EditorGUILayout.BeginHorizontal();
		obType = (LEVELOBTYPE)EditorGUILayout.EnumPopup("Object Type: ",obType);
		if(GUILayout.Button("Apply"))
		{
			Target.ObType = obType;
		}
		EditorGUILayout.EndHorizontal();

		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfDirtyOrScript();
	}
}
