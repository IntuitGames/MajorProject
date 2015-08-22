using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// A tether manager that doesn't change based on distance.
/// </summary>[ExecuteInEditMode]public class StaticTetherManager : MonoBehaviour{
    [Range(1, 25)]
    public int jointCount = 1;

    [ReadOnly(EditableInEditor = true)]
    public SpringJoint startObject;
    [ReadOnly(EditableInEditor = true)]
    public Rigidbody endObject;
    [ReadOnly(EditableInEditor = true)]
    public DebugTetherVisual tetherClone;
    [ReadOnly(EditableInEditor = true)]
    public SpringJoint jointClone;

    [ReadOnly]
    public List<DebugTetherVisual> tethers = new List<DebugTetherVisual>(12);
    [ReadOnly]
    public List<SpringJoint> joints = new List<SpringJoint>(12);

    [ReadOnly]
    public float distanceBetweenObjects;
    [ReadOnly]
    public float distanceBetweenJoints;	    void Update()
    {
        if (startObject && endObject)
            distanceBetweenObjects = Vector3.Distance(startObject.transform.position, endObject.position);
        else
            distanceBetweenObjects = default(float);

        if(distanceBetweenObjects > 0)
        {
            distanceBetweenJoints = distanceBetweenObjects / (jointCount + 1);
        }
    }	public void Rebuild()	{
        if (Application.isPlaying || !startObject || !endObject || !tetherClone || !jointClone) return;        // Clear joints
        joints.ForEach(x => DestroyImmediate(x.gameObject));
        joints.Clear();

        // Clear tethers
        tethers.ForEach(x => DestroyImmediate(x.gameObject));
        tethers.Clear();

        // Joint creation
        while (joints.Count < jointCount)
        {
            SpringJoint jointToBeAdded = Unity.Instantiate<SpringJoint>(jointClone, transform);
            joints.Add(jointToBeAdded);
        }        // Tether creation        while (tethers.Count < jointCount + 1)
        {
            DebugTetherVisual tetherToBeAdded = Unity.Instantiate<DebugTetherVisual>(tetherClone, transform);
            tethers.Add(tetherToBeAdded);
        }        // Joint placement & connection
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].name = joints[i].name + " " + (i + 1);
            Vector3 PositionVec = endObject.position + (startObject.transform.position - endObject.position) * JointPositionMultiplier(i);
            joints[i].transform.localPosition = PositionVec;

            if (i == 0)
                startObject.connectedBody = joints[i].GetComponent<Rigidbody>();

            if (i < joints.Count - 1)
                joints[i].connectedBody = joints[i + 1].GetComponent<Rigidbody>();
            else
                joints[i].connectedBody = endObject;
        }        // Tether hookup        for(int i = 0; i < tethers.Count; i++)
        {
            tethers[i].name = tethers[i].name + " " + (i + 1);

            if(i == 0)
            {
                tethers[i].useLocal = false;
                tethers[i].objectA = endObject.transform;
                tethers[i].objectB = joints[i].transform;
            }
            else if(i < jointCount)
            {
                tethers[i].useLocal = true;
                tethers[i].objectA = joints[i - 1].transform;
                tethers[i].objectB = joints[i].transform;
            }
            else
            {
                tethers[i].useLocal = false;
                tethers[i].objectA = joints[i - 1].transform;
                tethers[i].objectB = startObject.transform;
            }
        }	}    private float JointPositionMultiplier(int index)
    {
        return (distanceBetweenJoints / distanceBetweenObjects) * (index + 1);
    }}