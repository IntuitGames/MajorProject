using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// A tether system that creates and pools new custom joints.
/// </summary>[ExecuteInEditMode]public class DynamicTetherManager : MonoBehaviour
{
    #region VARIABLES

    // CONSTANTS
    private const int EXPECTED_MAX_JOINTS = 100;
    private const float MIN_JOINT_DISTANCE = 0.1f;
    private const float MAX_JOINT_DISTANCE = 0.5f;

    // NESTED
    public enum TetherSystemActivityStates { Off, RuntimeOnly, EditorAndRuntime };

    // INSPECTOR
    [Header("References")]
    public CustomJoint jointPrefab;
    public CustomJoint startJoint;
    public CustomJoint endJoint;
    [ReadOnly]
    public List<CustomJoint> activeJoints = new List<CustomJoint>(EXPECTED_MAX_JOINTS + 10);
    [ReadOnly]
    public List<CustomJoint> inactiveJoints = new List<CustomJoint>(EXPECTED_MAX_JOINTS + 10);

    [Header("Properties")]
    public TetherSystemActivityStates activity = TetherSystemActivityStates.EditorAndRuntime;
    [ReadOnly(EditableInEditor = true)]
    public int jointPoolSize = EXPECTED_MAX_JOINTS;
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
        isInitialized = true;
    }

    void Update()
    {
        if (!isActive || !isInitialized) return;
    }

    void FixedUpdate()
    {
        if (!isActive || !isInitialized) return;
    }}