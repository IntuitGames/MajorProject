using UnityEngine;
using CustomExtensions;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// A tether system that creates and pools new custom joints.
/// </summary>[ExecuteInEditMode, System.Obsolete]public class DynamicTetherManager : MonoBehaviour
{
    #region VARIABLES

    // STATICS
    public static DynamicTetherManager instance;

    // CONSTANTS
    private const int EXPECTED_MAX_JOINTS = 100;
    private const float MIN_JOINT_DISTANCE = 0.01f;
    private const float MAX_JOINT_DISTANCE = 2;
    private const string ACTIVE_JOINT_NAME = "Joint {0}";
    private const string INACTIVE_JOINT_NAME = "Joint";

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
    public int jointMaxCap = EXPECTED_MAX_JOINTS * 2;
    [ReadOnly(EditableInEditor = true)]
    public int jointPoolSize = EXPECTED_MAX_JOINTS;
    public bool dynamicPoolSize = true;
    [Range(MIN_JOINT_DISTANCE, MAX_JOINT_DISTANCE)]
    public float minJointDistance = MIN_JOINT_DISTANCE;
    [Range(MIN_JOINT_DISTANCE, MAX_JOINT_DISTANCE)]
    public float restJointDistance = (MAX_JOINT_DISTANCE + MIN_JOINT_DISTANCE) / 2;
    [Range(MIN_JOINT_DISTANCE, MAX_JOINT_DISTANCE)]
    public float maxJointDistance = MAX_JOINT_DISTANCE;
    [Range(1, 10)]
    public int jointUpdatesPerFrame = 5;
    public float localAppliedForce = 100;
    public AnimationCurve localForceCurve = AnimationCurve.EaseInOut(MIN_JOINT_DISTANCE, 0, MAX_JOINT_DISTANCE, 1);
    public float restPositionForce = 500;
    public AnimationCurve restForceCurve = AnimationCurve.EaseInOut(MIN_JOINT_DISTANCE, 0, MAX_JOINT_DISTANCE, 1);
    public float characterForce = 50;
    public AnimationCurve characterForceCurve = AnimationCurve.EaseInOut(MIN_JOINT_DISTANCE, 0, MAX_JOINT_DISTANCE, 1);
    public bool alignRotation = true;

    [Header("Info")]
    [ReadOnly]
    public int jointCount;
    [ReadOnly]
    public float tetherLength;
    [ReadOnly]
    public float directLength;
    [ReadOnly]
    public float targetJointDistance;
    [ReadOnly]
    public float averageJointDistance;
    [ReadOnly]
    public float averageJointVelocity;

    // PROPERTIES
    public bool isActive
    {
        get
        {
            // Currently not functional in editor for simplicity
            return Application.isPlaying && isValidReferences && activity != TetherSystemActivityStates.Off;
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
    private int jointUpdates;

    #endregion

    #region MESSAGES

    void Start()
    {
        instance = this;

        if (!isActive) return;

        if (!isInitialized) Initialize();
    }

    void Update()
    {
        UpdateInfo();

        if (!isActive)
        {
            if (!isInitialized)
                return;
            else
            {
                Reset(Application.isPlaying);
                return;
            }
        }

        if (!isInitialized) Initialize();

        HandleJoints();
    }    void FixedUpdate()
    {
        if (!isActive || !Application.isPlaying) return;

        float tetherLengthMulti = characterForceCurve.Evaluate(tetherLength);

        Vector3 startJointForce = (startJoint.nextMidPoint - startJoint.transform.position).normalized * characterForce * tetherLengthMulti;
        Vector3 endJointForce = (endJoint.previousMidPoint - endJoint.transform.position).normalized * characterForce * tetherLengthMulti;

        startJoint.rigidBody.AddForce(startJointForce * Time.fixedDeltaTime);
        endJoint.rigidBody.AddForce(endJointForce * Time.fixedDeltaTime);
    }

    #endregion

    // Destroys all active joints and pooled joints    private void Reset(bool isPlaying)
    {
        System.Action<CustomJoint> destroyMethod;

        if (isPlaying)
            destroyMethod = x => Destroy(x.gameObject);
        else
            destroyMethod = x => DestroyImmediate(x.gameObject);

        activeJoints.ForEach(x => destroyMethod(x));
        inactiveJoints.ToList().ForEach(x => destroyMethod(x));
        activeJoints.Clear();
        inactiveJoints.Clear();

        UpdateIndex(0);

        UpdateReferences(0);

        isInitialized = false;
    }    // Creates the initial joint pool and sets indexes and references up    private void Initialize()
    {
        SpawnPool(jointPoolSize);

        UpdateIndex(0);

        UpdateReferences(0);

        isInitialized = true;
    }    // Instantiates a specified amount of joints    private void SpawnPool(int size)
    {
        for(int i = 0; i < size; i++)
        {
            CustomJoint newJoint = jointPrefab.Instantiate<CustomJoint>(transform);
            newJoint.name = INACTIVE_JOINT_NAME;
            DeactivateJoint(newJoint);
        }
    }    // Updates the read-only data    private void UpdateInfo()
    {
        if (Application.isPlaying) jointPoolSize = activeJoints.Count + inactiveJoints.Count;
        jointCount = activeJoints.Count;
        if (startJoint && endJoint) tetherLength = startJoint.distanceToNext + activeJoints.Sum<CustomJoint>(x => x.distanceToNext);
        else tetherLength = 0;
        if (startJoint && endJoint) directLength = Vector3.Distance(startJoint.transform.position, endJoint.transform.position);
        else directLength = 0;
        if (startJoint && endJoint) targetJointDistance = directLength / jointCount;
        else targetJointDistance = 0;
        if (startJoint && endJoint) averageJointDistance = (tetherLength - startJoint.distanceToNext) / jointCount;
        else averageJointDistance = 0;
        if (startJoint && endJoint) averageJointVelocity = activeJoints.Sum(x => x.rigidBody.velocity.magnitude) / jointCount;
        else averageJointDistance = 0;
    }    // Updates the joint indexes    private void UpdateIndex(int initialIndex)
    {
        startJoint.index = 0;

        for (int i = Mathf.Max(initialIndex - 1, 0); i < activeJoints.Count; i++)
        {
            activeJoints[i].index = i + 1;
            activeJoints[i].name = string.Format(ACTIVE_JOINT_NAME, activeJoints[i].index);
        }

        endJoint.index = activeJoints.Count + 1;
    }    // Updates the next and previous references on each of the joints
    private void UpdateReferences(int initialIndex)
    {
        startJoint.next = GetJoint(1);
        startJoint.previous = null;
        startJoint.next.previous = startJoint;

        for (int i = Mathf.Max(initialIndex - 1, 0); i < activeJoints.Count; i++)
        {
            activeJoints[i].next = GetJoint(i + 2);
            activeJoints[i].next.previous = activeJoints[i];
        }

        endJoint.next = null;
        endJoint.previous = GetJoint(activeJoints.Count);
    }    // Determines when to enable and disable new joints based on the distance between    private void HandleJoints()
    {
        if(startJoint.distanceToNext > maxJointDistance)
        {
            ActivateJoint(startJoint);
        }
        else if(startJoint.distanceToNext < minJointDistance)
        {
            DeactivateJoint(startJoint.next);
        }

        activeJoints.OrderBy(x => x.distanceToNext);

        jointUpdates = 0;
        for (int i = 0; i < activeJoints.Count; i++)
        {
            if (jointUpdates >= jointUpdatesPerFrame) break;

            if(activeJoints[i].distanceToNext > maxJointDistance)
            {
                ActivateJoint(activeJoints[i]);
                jointUpdates++;
            }
        }

        jointUpdates = 0;
        for (int i = activeJoints.Count - 1; i >= 0; i--)
        {
            if (jointUpdates >= jointUpdatesPerFrame) break;

            if (activeJoints[i].distanceToPrevious < minJointDistance)
            {
                DeactivateJoint(activeJoints[i]);
                jointUpdates++;
            }
        }

        if(endJoint.distanceToPrevious > maxJointDistance)
        {
            ActivateJoint(endJoint.previous);
        }
        else if(endJoint.distanceToPrevious < minJointDistance)
        {
            DeactivateJoint(endJoint.previous);
        }
    }

    // Attempts to activate a new joint by taking one from the pool. Mat create a new one if specified to do so
    private void ActivateJoint(CustomJoint joint)
    {
        CustomJoint newJoint = GetInactiveJoint(dynamicPoolSize);
        if (!newJoint) return;
        newJoint.transform.position = joint.nextMidPoint;
        newJoint.gameObject.SetActive(true);
        newJoint.gameObject.hideFlags = HideFlags.None;
        activeJoints.Insert(joint.index, newJoint);
        UpdateIndex(joint.index);
        UpdateReferences(joint.index);
    }    // Takes an active joint and puts it in the joint pool    private void DeactivateJoint(CustomJoint joint)
    {
        int index = joint.index;
        bool contained = activeJoints.Contains(joint);
        if(contained) activeJoints.Remove(joint);
        joint.gameObject.SetActive(false);
        joint.name = INACTIVE_JOINT_NAME;
        joint.index = -1;
        joint.next = null;
        joint.previous = null;
        joint.gameObject.hideFlags = HideFlags.HideInHierarchy;
        inactiveJoints.Enqueue(joint);
        if (contained) UpdateIndex(index);
        if (contained) UpdateReferences(index);
    }    // Gets the joint with an index    private CustomJoint GetJoint(int index)
    {
        if (index == startJoint.index) return startJoint;
        else if (index == endJoint.index) return endJoint;
        else if (index < 0) return null;
        else return activeJoints.FirstOrDefault(x => x.index == index, null);
    }    // Gets the next inactive joint but may create a new one if required    private CustomJoint GetInactiveJoint(bool createNew)
    {
        if(inactiveJoints.Count > 0)
            return inactiveJoints.Dequeue();
        else if(createNew && jointPoolSize <= jointMaxCap)
        {
            CustomJoint newJoint = jointPrefab.Instantiate<CustomJoint>(transform);
            newJoint.name = newJoint.name.Replace("(Clone)", " (NEW)");
            DeactivateJoint(newJoint);
            return inactiveJoints.Dequeue();
        }
        else
            return null;
    }    public Vector3 GetRestPosition(int index)
    {
        return Vector3.Lerp(startJoint.transform.position, endJoint.transform.position, (float)index / (jointCount + 1f));
    }}