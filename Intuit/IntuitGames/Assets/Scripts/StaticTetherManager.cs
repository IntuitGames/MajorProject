using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// A tether manager that doesn't change based on distance.
/// </summary>[ExecuteInEditMode]public class StaticTetherManager : MonoBehaviour{
    // CONSTANTS
    private const int MAX_JOINT_COUNT = 50;
    private const int TETHER_LAYER = 10;
    private const int PLAYER_LAYER = 9;
    private const int GROUND_LAYER = 8;

    [Range(1, MAX_JOINT_COUNT)]
    public int jointCount = 1;

    public SpringJoint startObject;
    public Rigidbody endObject;
    [ReadOnly(EditableInEditor = true)]
    public DebugTetherVisual tetherClone;
    [ReadOnly(EditableInEditor = true)]
    public SpringJoint jointClone;

    [ReadOnly]
    public List<DebugTetherVisual> tethers = new List<DebugTetherVisual>(MAX_JOINT_COUNT + 1);
    [ReadOnly]
    public List<SpringJoint> joints = new List<SpringJoint>(MAX_JOINT_COUNT + 1);

    [ReadOnly]
    public float distanceBetweenObjects;
    [ReadOnly]
    public float distanceBetweenJoints;

    public bool linkStrainDistance = true;
    public bool updateAnchor = true;
    public bool autoStabilize = true;
    [Range(0, 1000)]
    public float stabilizationStrength = 100;
    public bool disableExternalForces = false;
    public bool ignoreSelfCollision = true;
    public bool ignorePlayerCollision = true;
    public bool ignoreGroundCollision = false;	    void Update()
    {
        // Update read-only data
        if (startObject && endObject)
            distanceBetweenObjects = Vector3.Distance(startObject.transform.position, endObject.position);
        else
            distanceBetweenObjects = default(float);

        if(distanceBetweenObjects > 0)
        {
            distanceBetweenJoints = distanceBetweenObjects / (jointCount + 1);
        }

        // Update strain colors
        if (linkStrainDistance)
        {
            tethers.ForEach(x => x.closeDistance = distanceBetweenJoints * 2);
            tethers.ForEach(x => x.farDistance = distanceBetweenJoints * 4);
        }

        // Update anchors
        if(updateAnchor)
        {
            joints.ForEach(x => x.autoConfigureConnectedAnchor = false);
            startObject.autoConfigureConnectedAnchor = false;
            joints.ForEach(x => x.connectedAnchor = new Vector3(0.5f, 0, 0));
            startObject.connectedAnchor = new Vector3(0.5f, 0, 0);
        }
        else if(Application.isPlaying)
        {
            joints.ForEach(x => x.autoConfigureConnectedAnchor = true);
            startObject.autoConfigureConnectedAnchor = true;
        }

        // Ignore collisions
        Physics.IgnoreLayerCollision(TETHER_LAYER, TETHER_LAYER, ignoreSelfCollision);
        Physics.IgnoreLayerCollision(PLAYER_LAYER, TETHER_LAYER, ignorePlayerCollision);
        Physics.IgnoreLayerCollision(GROUND_LAYER, TETHER_LAYER, ignoreGroundCollision);
    }    void FixedUpdate()
    {
        // Force joints towards their rest positions
        if (Application.isPlaying && autoStabilize)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                if (disableExternalForces)
                    joints[i].GetComponent<Rigidbody>().velocity = Vector3.zero;

                joints[i].GetComponent<Rigidbody>().AddForce((JointRestPosition(i) - joints[i].transform.position) * Time.fixedDeltaTime *
                    (disableExternalForces ? stabilizationStrength * 50 : stabilizationStrength));
            }
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
        }

        // Copy component values to the object spring joint
        startObject.GetCopyof<SpringJoint>(jointClone);        // Joint placement & connection
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].name = (joints[i].name + (i + 1)).Replace("(Clone)", " ");
            Vector3 PositionVec = JointRestPosition(i);
            joints[i].transform.localPosition = PositionVec;

            if (i == 0)
                startObject.connectedBody = joints[i].GetComponent<Rigidbody>();

            if (i < joints.Count - 1)
                joints[i].connectedBody = joints[i + 1].GetComponent<Rigidbody>();
            else
                joints[i].connectedBody = endObject;
        }        // Tether hookup        for(int i = 0; i < tethers.Count; i++)
        {
            tethers[i].name = (tethers[i].name + (i + 1)).Replace("(Clone)", " ");

            if(i == 0)
            {
                tethers[i].useLocal = false;
                tethers[i].objectA = startObject.transform;
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
                tethers[i].objectB = endObject.transform;
            }
        }	}    private Vector3 JointRestPosition(int index)
    {
        return endObject.position + (startObject.transform.position - endObject.position) * JointPositionMultiplier(index);
    }    private float JointPositionMultiplier(int index)
    {
        return (distanceBetweenJoints / distanceBetweenObjects) * (jointCount - index);
    }}