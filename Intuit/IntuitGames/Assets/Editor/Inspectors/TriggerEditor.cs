using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;
using CustomExtensions;[CustomEditor(typeof(Trigger), true)]public class TriggerEditor : Editor{
    Trigger Target { get; set;}
    Dictionary<string, SerializedProperty> baseProperties = new Dictionary<string, SerializedProperty>();
    Trigger.TriggerType triggerType;
    Trigger.VolumeType volumeType;
    Collider triggerCollider
    {
        get
        {
            return baseProperties["colliderComp"].objectReferenceValue as Collider;
        }
        set
        {
            baseProperties["colliderComp"].objectReferenceValue = value;
        }
    }
    SerializedProperty property;
    bool showChildren;    void OnEnable()
    {
        Target = (Trigger)target;

        baseProperties.Add("triggerType", serializedObject.FindProperty("triggerType"));
        baseProperties.Add("volumeType", serializedObject.FindProperty("volumeType"));
        baseProperties.Add("proximityDistance", serializedObject.FindProperty("proximityDistance"));
        baseProperties.Add("proximityCheckFrequency", serializedObject.FindProperty("proximityCheckFrequency"));
        baseProperties.Add("triggerLayer", serializedObject.FindProperty("triggerLayer"));
        baseProperties.Add("isMultiTrigger", serializedObject.FindProperty("isMultiTrigger"));
        baseProperties.Add("hasBeenTriggered", serializedObject.FindProperty("hasBeenTriggered"));
        baseProperties.Add("onTriggeredEvent", serializedObject.FindProperty("onTriggeredEvent"));
        baseProperties.Add("colliderComp", serializedObject.FindProperty("colliderComp"));

        property = serializedObject.GetIterator();

        // Initializer
        DestroyComponent(null);
        if (!Target.colliderComp && Target.triggerType != Trigger.TriggerType.ProximityBased)
            ApplyTriggerChanges(Target.triggerType, Target.volumeType);
    }

    public override void OnInspectorGUI()
    {
        if (!Target) return;

        // Script fields
        EditorGUI.indentLevel = 0;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUILayout.ObjectField("Editor Script", MonoScript.FromScriptableObject(this), this.GetType(), false);

        // Reset trigger fields (Change flags)
        triggerType = (Trigger.TriggerType)baseProperties["triggerType"].enumValueIndex;
        volumeType = (Trigger.VolumeType)baseProperties["volumeType"].enumValueIndex;

        // Draw Header
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);

        // Draw trigger type enum fields
        EditorGUILayout.PropertyField(baseProperties["triggerType"]);
        if (triggerType != Trigger.TriggerType.ProximityBased)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(baseProperties["volumeType"]);
            EditorGUI.indentLevel--;
        }
        if (triggerType == Trigger.TriggerType.ProximityBased)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(baseProperties["proximityDistance"]);
            EditorGUILayout.PropertyField(baseProperties["proximityCheckFrequency"]);
            if (Application.isPlaying && Target.proximityTimer != null)
            {
                GUI.enabled = false;
                EditorGUILayout.Slider("Timer Value", Target.proximityTimer.Percentage, 0, 1);
                GUI.enabled = true;
            }
            EditorGUI.indentLevel--;
        }

        // Draw other base properties
        EditorGUILayout.PropertyField(baseProperties["triggerLayer"]);
        EditorGUILayout.PropertyField(baseProperties["isMultiTrigger"]);
        EditorGUILayout.PropertyField(baseProperties["hasBeenTriggered"]);
        EditorGUILayout.PropertyField(baseProperties["onTriggeredEvent"]);

        // Apply type changes
        if ((int)triggerType != baseProperties["triggerType"].enumValueIndex || (int)volumeType != baseProperties["volumeType"].enumValueIndex)
        {
            ApplyTriggerChanges((Trigger.TriggerType)baseProperties["triggerType"].enumValueIndex, (Trigger.VolumeType)baseProperties["volumeType"].enumValueIndex);
        }

        // Draw child fields
        // Iterate through properties and draw them like it normally would
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField(Target.GetType().Name + " Settings", EditorStyles.boldLabel);

        property.Reset();
        property.Next(true);
        do
        {
            EditorGUI.indentLevel = property.depth;
            if (property.name.StartsWith("m_") || property.name.EqualToAny(baseProperties.Keys.ToArray())) continue; // Ignore Unity and base properties
            showChildren = EditorGUILayout.PropertyField(property); // Draw property

        }
        while (property.NextVisible(showChildren));
        EditorGUI.indentLevel = 0;

        // Apply changes to the property
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfDirtyOrScript();
    }

    public void ApplyTriggerChanges(Trigger.TriggerType newTriggerType, Trigger.VolumeType newVolumeType)
    {
        if (triggerCollider)
        {
            DestroyComponent(triggerCollider);
            triggerCollider = null;
        }

        if (newTriggerType != Trigger.TriggerType.ProximityBased)
        {
            triggerCollider = AddNewCollider(newVolumeType);
            triggerCollider.isTrigger = newTriggerType == Trigger.TriggerType.TriggerVolume;
        }
    }

    private Collider AddNewCollider(Trigger.VolumeType type)
    {
        if (type == Trigger.VolumeType.Box)
            return Target.gameObject.AddComponent<BoxCollider>();
        else if (type == Trigger.VolumeType.Sphere)
            return Target.gameObject.AddComponent<SphereCollider>();
        else if (type == Trigger.VolumeType.Capsule)
            return Target.gameObject.AddComponent<CapsuleCollider>();
        else
            return Target.gameObject.AddComponent<MeshCollider>();
    }    // Destroying the component immediately causes an error    private void DestroyComponent(Collider colliderToDestroy)
    {
        // Destroy all flagged components
        foreach (var comp in Target.GetComponents<Collider>().Where(x => (x.hideFlags | HideFlags.HideInInspector) == x.hideFlags))
            DestroyImmediate(comp, true);

        // Flag to destroy
        if (colliderToDestroy)
        {
            colliderToDestroy.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
            colliderToDestroy.enabled = false;
        }
    }

    public override bool RequiresConstantRepaint()
    {
        return Application.isPlaying ? true : base.RequiresConstantRepaint();
    }}