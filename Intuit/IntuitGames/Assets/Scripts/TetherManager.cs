using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// A tether system that determines joint positioning based on collisions.
/// </summary>[ExecuteInEditMode]public class TetherManager : MonoBehaviour{
    // INSPECTOR
    [Header("References")]
    public TetherJoint jointPrefab;
    public DebugTetherVisual tetherVisualPrefab;
    public Transform startPoint;
    public Transform endPoint;
    [HideInInspector]
    public List<TetherJoint> joints;
    [HideInInspector]
    public List<DebugTetherVisual> tetherVisuals;

    [Header("Properties")]
    [Range(1, 200)]
    public int jointCount = 10;
    public bool showJoints = true;
    public bool showTetherVisual = true;
    public bool showInHierarchy = false;    [Header("Info")]
    [ReadOnly]
    public float directLength;
    [ReadOnly]
    public float tetherLength;    void Update()
    {
        // Update info
        if(startPoint && endPoint) directLength = Vector3.Distance(startPoint.position, endPoint.position);
        else directLength = default(float);
        if(tetherVisuals.Any()) tetherLength = tetherVisuals.Sum(x => x.distanceBetweenObjects);
        else tetherLength = default(float);

        if (!jointPrefab || !tetherVisualPrefab || !startPoint || !endPoint) return;

        if (joints.Any() && joints.FirstOrDefault().GetComponent<Renderer>().enabled != showJoints)
            joints.ForEach(x => x.GetComponent<Renderer>().enabled = showJoints);

        if (tetherVisuals.Any() && tetherVisuals.FirstOrDefault().GetComponent<Renderer>().enabled != showTetherVisual)
            tetherVisuals.ForEach(x => x.GetComponent<Renderer>().enabled = showTetherVisual);

        if(joints.Any() && joints.FirstOrDefault().gameObject.hideFlags != (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy))
            joints.ForEach(x => x.gameObject.hideFlags = (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy));

        if (tetherVisuals.Any() && tetherVisuals.FirstOrDefault().gameObject.hideFlags != (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy))
            tetherVisuals.ForEach(x => x.gameObject.hideFlags = (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy));
    }    void FixedUpdate()
    {
        if (!Application.isPlaying) return;

        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].rigidbodyComp.MovePosition(GetMovePosition(joints[i], i, false));
        }
    }    // Re-creates the joints and puts them in their starting positions    public void Rebuild()
    {
        if (!jointPrefab || !tetherVisualPrefab || !startPoint || !endPoint) return;

        Reset();

        joints = InstantiateMany(jointPrefab, jointCount).ToList();
        tetherVisuals = InstantiateMany(tetherVisualPrefab, jointCount + 1).ToList();

        // Provides initial positioning for the joints
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].transform.position = GetMovePosition(joints[i], i, true);
        }

        // Sets up tether visual references
        for (int i = 0; i < tetherVisuals.Count; i++)
        {
            if(i == 0) // First
            {
                tetherVisuals[i].objectA = startPoint;
                tetherVisuals[i].objectB = joints[i].transform;
            }
            else if(i < joints.Count) // In-between
            {
                tetherVisuals[i].objectA = joints[i - 1].transform;
                tetherVisuals[i].objectB = joints[i].transform;
            }
            else // Last
            {
                tetherVisuals[i].objectA = joints[i - 1].transform;
                tetherVisuals[i].objectB = endPoint;
            }
        }
    }    // Destroys all current references and clears the joint and tether visual list    public void Reset()
    {
        joints.ForEach(x => DestroyImmediate(x.gameObject));
        tetherVisuals.ForEach(x => DestroyImmediate(x.gameObject));

        joints.Clear();
        tetherVisuals.Clear();
    }    // Used to instantiate many joints and tether visuals (Probably should move to extension methods)    private IEnumerable<T> InstantiateMany<T>(T prefab, int amount) where T: Component
    {
        for (int i = 0; i < amount; i++)
            yield return Instantiate<T>(prefab);
    }    // Used to determine the position of each joint    private Vector3 GetMovePosition(TetherJoint joint, int index, bool direct)
    {
        if (direct)
        {
            return Vector3.Lerp(startPoint.position, endPoint.position, ((float)index + 1) / (jointCount + 1));
        }
        else
        {
            Vector3 startPointPos, endPointPos;
            int relativeIndex, relativeCount;

            TetherJoint startTempJoint = joints.LastOrDefault(x => x.isColliding && joints.IndexOf(x) < index);
            TetherJoint endTempJoint = joints.FirstOrDefault(x => x.isColliding && joints.IndexOf(x) > index);

            if(startTempJoint && endTempJoint)
            {
                startPointPos = startTempJoint.transform.position;
                endPointPos = endTempJoint.transform.position;
                relativeIndex = index - (joints.IndexOf(startTempJoint) + 1) + 1;
                relativeCount = joints.IndexOf(endTempJoint) - (joints.IndexOf(startTempJoint) + 1) + 1;
            }
            else if(startTempJoint)
            {
                startPointPos = startTempJoint.transform.position;
                endPointPos = endPoint.position;
                relativeIndex = index - (joints.IndexOf(startTempJoint) + 1) + 1;
                relativeCount = jointCount - (joints.IndexOf(startTempJoint) + 1) + 1;
            }
            else if(endTempJoint)
            {
                startPointPos = startPoint.position;
                endPointPos = endTempJoint.transform.position;
                relativeIndex = index + 1;
                relativeCount = joints.IndexOf(endTempJoint) + 1;
            }
            else
            {
                startPointPos = startPoint.position;
                endPointPos = endPoint.position;
                relativeIndex = index + 1;
                relativeCount = jointCount + 1;
            }

            return Vector3.Lerp(startPointPos, endPointPos, (float)relativeIndex / relativeCount);
        }
    }    // Determines the direction the start and en points should move in    // Instead of the first and last maybe change this to the closest colliding joint    // Although the result will is the same anyway when normalized    public Vector3 GetStartAndEndMoveDirection(bool isStartPoint)
    {
        if(isStartPoint)
        {
            return (joints.FirstOrDefault().transform.position - startPoint.position).normalized;
        }
        else
        {
            return (joints.LastOrDefault().transform.position - endPoint.position).normalized;
        }
    }}