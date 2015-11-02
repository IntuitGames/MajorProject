using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEditor;
using System.Reflection;[CustomEditor(typeof(TetherManager))]public class TetherManagerEditor : Editor
{
    private TetherManager Target;

    public void OnEnable()
    {
        Target = (TetherManager)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Editor actions
        if (!Application.isPlaying)
        {
            if (GUILayout.Button("Rebuild"))
            {
                Target.Rebuild();
            }

            if (GUILayout.Button("Reset"))
            {
                GameObject.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith(TetherManager.JOINT_NAME) || x.name.StartsWith(TetherManager.TETHER_NAME)).ToList().ForEach(x => DestroyImmediate(x));
                Target.Reset();
            }

            EditorGUILayout.HelpBox("A rebuild is recommended when a change is made to the joint count, joint prefab or when the start / end point moves.", MessageType.Info, true);
        }
        else // In-game actions
        {
            if (!Target.disconnected && GUILayout.Button(string.Format("Force Disconnect ({0})", Target.disconnectInput)))
            {
                Target.Disconnect(Target.joints[Target.jointCount / 2]);
            }
            else if (Target.disconnected && GUILayout.Button(string.Format("Force Reconnect ({0})", Target.reconnectInput)))
            {
                Target.Reconnect();
            }

            GUI.enabled = !Target.disconnected;
            if (GUILayout.Button("Stabilize"))
            {
                for (int i = 0; i < Target.joints.Count; i++)
                {
                    Target.joints[i].transform.position = Target.GetJointMovePosition(Target.joints[i], i, true);
                    Target.joints[i].rigidbodyComp.velocity = Vector3.zero;
                    Target.joints[i].rigidbodyComp.angularVelocity = Vector3.zero;
                }
            }
            GUI.enabled = true;
        }
    }}