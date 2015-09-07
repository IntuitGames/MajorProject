using UnityEngine;
using CustomExtensions;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// A tether system that creates and pools new custom joints.
/// </summary>[ExecuteInEditMode]public class DynamicTetherManager : MonoBehaviour
{
    #region VARIABLES

    // CONSTANTS
    private const int EXPECTED_MAX_JOINTS = 100;
    private const float MIN_JOINT_DISTANCE = 0.1f;
    private const float MAX_JOINT_DISTANCE = 0.5f;
    private const string JOINT_NAME = "Joint {0}";
    private const string NEW_JOINT_NAME = "Joint";

    // NESTED
    public enum TetherSystemActivityStates { Off, RuntimeOnly, EditorAndRuntime };

    // INSPECTOR
    [Header("References")]
    public CustomJoint jointPrefab;
    public CustomJoint startJoint;
    public CustomJoint endJoint;
    [ReadOnly, System.NonSerialized]
    public List<CustomJoint> activeJoints = new List<CustomJoint>(EXPECTED_MAX_JOINTS + 10);
    [ReadOnly, System.NonSerialized]
    public Queue<CustomJoint> inactiveJoints = new Queue<CustomJoint>(EXPECTED_MAX_JOINTS + 10);

    [Header("Properties")]
    public TetherSystemActivityStates activity = TetherSystemActivityStates.EditorAndRuntime;
    [ReadOnly(EditableInEditor = true)]
    public int jointPoolSize = EXPECTED_MAX_JOINTS;
    public bool dynamicPoolSize = true;
    [Range(MIN_JOINT_DISTANCE, MAX_JOINT_DISTANCE)]
    public float minJointDistance = MIN_JOINT_DISTANCE;
    [Range(MIN_JOINT_DISTANCE, MAX_JOINT_DISTANCE)]
    public float restJointDistance = (MAX_JOINT_DISTANCE + MIN_JOINT_DISTANCE) / 2;
    [Range(MIN_JOINT_DISTANCE, MAX_JOINT_DISTANCE)]
    public float maxJointDistance = MAX_JOINT_DISTANCE;

    [Header("Info")]
    [ReadOnly]
    public int jointCount;
    [ReadOnly]
    public float tetherLength;
    [ReadOnly]
    public float directLength;

    // PROPERTIES
    public bool isActive
    {
        get
        {
            // Currently not functional in editor for simplicity
            return Application.isPlaying && isValidReferences;
            //if (activity == TetherSystemActivityStates.Off)
            //    return false;
            //else if (activity == TetherSystemActivityStates.RuntimeOnly)
            //    return Application.isPlaying && isValidReferences;
            //else
            //    return isValidReferences;
        }
    }
    private bool isValidReferences
    {
        get { return jointPrefab && startJoint && endJoint; }
    }

    // PRIVATES
    private bool isInitialized;

    #endregion

    void Start()
    {
        if (!isActive) return;

        SpawnPool(jointPoolSize);

        UpdateIndex();

        UpdateReferences();

        isInitialized = true;
    }

    void Update()
    {
        if (!isActive || !isInitialized) return;

        HandleJoints();

        UpdateInfo();
    }

    void FixedUpdate()
    {
        if (!isActive || !isInitialized) return;
    }    private void SpawnPool(int size)
    {
        for(int i = 0; i < size; i++)
        {
            CustomJoint newJoint = jointPrefab.Instantiate<CustomJoint>(transform);
            newJoint.name = NEW_JOINT_NAME;
            DeactivateJoint(newJoint);
        }
    }    private void UpdateInfo()
    {
        jointPoolSize = activeJoints.Count + inactiveJoints.Count;
        jointCount = activeJoints.Count;
        tetherLength = startJoint.distanceToNext + activeJoints.Sum<CustomJoint>(x => x.distanceToNext);
        directLength = Vector3.Distance(startJoint.transform.position, endJoint.transform.position);
    }    private void UpdateIndex()
    {
        startJoint.index = 0;

        for(int i = 0; i < activeJoints.Count; i++)
        {
            activeJoints[i].index = i + 1;
            activeJoints[i].name = string.Format(JOINT_NAME, activeJoints[i].index);
        }

        endJoint.index = activeJoints.Count + 1;
    }    private void UpdateReferences()
    {
        startJoint.next = GetJoint(1);
        startJoint.previous = null;
        startJoint.next.previous = startJoint;

        for (int i = 0; i < activeJoints.Count; i++)
        {
            activeJoints[i].next = GetJoint(i + 2);
            activeJoints[i].next.previous = activeJoints[i];
        }

        endJoint.next = null;
        endJoint.previous = GetJoint(activeJoints.Count);
    }    private void HandleJoints()
    {
        if(startJoint.distanceToNext > maxJointDistance)
        {
            ActivateJoint(startJoint);
        }
        else if(startJoint.distanceToNext < minJointDistance)
        {
            DeactivateJoint(startJoint.next);
        }

        for(int i = 0; i < activeJoints.Count; i++)
        {
            if(activeJoints[i].distanceToNext > maxJointDistance)
            {
                ActivateJoint(activeJoints[i]);
            }
            else if(activeJoints[i].distanceToPrevious < minJointDistance)
            {
                DeactivateJoint(activeJoints[i]);
            }
        }
    }

    private void ActivateJoint(CustomJoint joint)
    {
        CustomJoint newJoint = GetInactiveJoint(dynamicPoolSize);
        if (!newJoint) return;
        newJoint.transform.position = joint.nextMidPoint;
        newJoint.gameObject.SetActive(true);
        activeJoints.Insert(joint.index, newJoint);
        UpdateIndex();
        UpdateReferences();
    }    private void DeactivateJoint(CustomJoint joint)
    {
        bool contained = activeJoints.Contains(joint);
        if(contained) activeJoints.Remove(joint);
        joint.gameObject.SetActive(false);
        joint.index = -1;
        joint.next = null;
        joint.previous = null;
        inactiveJoints.Enqueue(joint);
        if (contained) UpdateIndex();
        if (contained) UpdateReferences();
    }    private CustomJoint GetJoint(int index)
    {
        if (index == startJoint.index) return startJoint;
        else if (index == endJoint.index) return endJoint;
        else if (index < 0) return null;
        else return activeJoints.FirstOrDefault(x => x.index == index, null);
    }    private CustomJoint GetInactiveJoint(bool createNew)
    {
        if(inactiveJoints.Count > 0)
            return inactiveJoints.Dequeue();
        else if(createNew)
        {
            CustomJoint newJoint = jointPrefab.Instantiate<CustomJoint>(transform);
            newJoint.name = newJoint.name.Replace("(Clone)", " (NEW)");
            DeactivateJoint(newJoint);
            return inactiveJoints.Dequeue();
        }
        else
            return null;
    }}