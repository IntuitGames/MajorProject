using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMODAsset))]
public class FMODEventInspector : Editor
{
	FMODAsset currentAsset; //Make an easy shortcut to the Dialogue your editing
	bool isPlaying = false;
	struct Param
	{
		public FMOD.Studio.PARAMETER_DESCRIPTION desc;
		public float val;
	}
	
	Param[] parameters = new Param[0];
	
	bool is3D;
	float minDistance, maxDistance;
	
	void Awake()
	{
		currentAsset=(FMODAsset)target;
		FMODEditorExtension.StopEvent();
		isPlaying = false;
		
		// set up parameters
		FMOD.Studio.EventDescription desc = FMODEditorExtension.GetEventDescription(currentAsset.id);
		int count;
		
		if (desc == null)
		{
			return;
		}
		
		desc.is3D(out is3D);
		desc.getMinimumDistance(out minDistance);
		desc.getMaximumDistance(out maxDistance);
		
		desc.getParameterCount(out count);
		parameters = new Param[count];
		
		for (int i = 0; i < count; ++i)
		{
			desc.getParameterByIndex(i, out parameters[i].desc);			
			parameters[i].val = parameters[i].desc.minimum;			
		}
	}
	
	void OnDestroy()
	{
		FMODEditorExtension.StopEvent();		
	}
	
	public override void OnInspectorGUI()
	{
        EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Path");
        GUI.enabled = false;
        EditorGUILayout.TextField(currentAsset.path);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("GUID");
        GUI.enabled = false;
        EditorGUILayout.TextField(currentAsset.id);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("Distance ({0})", is3D ? "3D" : "2D"));
		if (is3D)
		{
            GUI.enabled = false;
            EditorGUILayout.FloatField(minDistance);
            EditorGUILayout.FloatField(maxDistance);
            GUI.enabled = true;
		}
        EditorGUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		if (!isPlaying && GUILayout.Button("Play", new GUILayoutOption[0]))
		{
			FMODEditorExtension.AuditionEvent(currentAsset);
			isPlaying = true;
		}
		if (isPlaying && GUILayout.Button("Stop", new GUILayoutOption[0]))
		{
			FMODEditorExtension.StopEvent();
			isPlaying = false;
		}
		GUILayout.EndHorizontal();		
		
		for (int i = 0; i < parameters.Length; ++i)
		{
            if (i == 0)
                EditorGUILayout.LabelField("Paramters", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();	
            parameters[i].val = EditorGUILayout.Slider(parameters[i].desc.name, parameters[i].val, parameters[i].desc.minimum, parameters[i].desc.maximum, new GUILayoutOption[0]);
			FMODEditorExtension.SetEventParameterValue(i, parameters[i].val);
			GUILayout.EndHorizontal();
		}
	}
}
