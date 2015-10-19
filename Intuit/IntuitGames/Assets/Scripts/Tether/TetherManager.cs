using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// A tether system that determines joint positioning based on collisions.
/// </summary>
[ExecuteInEditMode]
public class TetherManager : Manager
{
    // INSPECTOR
    [Header("References")]
    public TetherJoint jointPrefab;
    public DebugTetherVisual tetherVisualPrefab;
    public Joint disconnectionJointPrefab;
    public Transform startPoint;
    public Transform endPoint;
    [HideInInspector]
    public List<TetherJoint> joints;
    [HideInInspector]
    public List<DebugTetherVisual> tetherVisuals;

    [Header("Properties")]
    [Range(3, 200)]
    public int jointCount = 100;
    [Range(0, 200)]
    public float jointSpeed = 100;
    public bool instantJointMovement = true;
    public bool showJoints = true;
    public bool showTetherVisual = true;
    public bool showInHierarchy = false;
    public bool performanceBoost = true;
    [Range(0, 100)]
    public float breakForce = 50;
    public KeyCode disconnectInput = KeyCode.B;
    public KeyCode reconnectInput = KeyCode.N;

    [Header("Experimental")]
    public bool experimentalNoStick = false;
    public bool experimentalWrapping = false;   // Adds force instead of moving if a joint is colliding (Sticky behavior)
    public bool experimentalCollision = false;
    [Range(0, 200)]
    public int collisionBuffer = 30;

    [Header("Info")]
    [ReadOnly]
    public bool disconnected = false;
    [ReadOnly]
    public float directLength;
    [ReadOnly]
    public float tetherLength;

    // EVENTS
    public event System.Action<TetherJoint> OnDisconnected = delegate { };
    public event System.Action<TetherJoint> OnReconnected = delegate { };

    // CONST
    public const string JOINT_NAME = "TM_Joint";
    public const string TETHER_NAME = "TM_Tether";

    // PRIVATES
    private Vector3 startPointPos, endPointPos;
    private int relativeIndex, relativeCount;
    private TetherJoint startTempJoint, endTempJoint, tempJoint;
    private List<TetherJoint> tempTetherList;

    void Awake()
    {
        if (!Application.isPlaying) return;

        // Setup collision buffer at the start of the game
        if (experimentalCollision)
        {
            joints.GetRange(0, collisionBuffer).ForEach(x => x.gameObject.IgnoreCollision(startPoint.gameObject));
            joints.GetRange(joints.Count - collisionBuffer, collisionBuffer).ForEach(x => x.gameObject.IgnoreCollision(endPoint.gameObject));
        }
    }

    void Update()
    {
        // Update info
        if (startPoint && endPoint) directLength = Vector3.Distance(startPoint.position, endPoint.position);
        else directLength = default(float);
        if (tetherVisuals != null && tetherVisuals.Any()) tetherLength = tetherVisuals.Sum(x => x.distanceBetweenObjects);
        else tetherLength = default(float);

        if (!jointPrefab || !tetherVisualPrefab || !startPoint || !endPoint) return;

        if (joints.Any() && joints.FirstOrDefault().GetComponent<Renderer>().enabled != showJoints)
            joints.ForEach(x => x.GetComponent<Renderer>().enabled = showJoints);

        if (tetherVisuals.Any() && tetherVisuals.FirstOrDefault().GetComponent<Renderer>().enabled != showTetherVisual)
            tetherVisuals.ForEach(x => x.GetComponent<Renderer>().enabled = showTetherVisual);

        if (joints.Any() && joints.FirstOrDefault().gameObject.hideFlags != (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy))
            joints.ForEach(x => x.gameObject.hideFlags = (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy));

        if (tetherVisuals.Any() && tetherVisuals.FirstOrDefault().gameObject.hideFlags != (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy))
            tetherVisuals.ForEach(x => x.gameObject.hideFlags = (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy));

        if (Physics.GetIgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Tether")) != !experimentalCollision)
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Tether"), !experimentalCollision);

        if (!Application.isPlaying) return;

        if (Input.GetKeyDown(disconnectInput))
        {
            Disconnect(joints[jointCount / 2]);
        }

        if (Input.GetKeyDown(reconnectInput))
        {
            Reconnect();
        }
    }

    void FixedUpdate()
    {
        if (!Application.isPlaying) return;

        for (int i = 0; i < joints.Count; i++)
        {
            if (!disconnected)
            {
                if (!experimentalWrapping)
                {
                    if (instantJointMovement)
                        joints[i].rigidbodyComp.MovePosition(GetMovePosition(joints[i], i, false));
                    else
                        joints[i].rigidbodyComp.MovePosition(Vector3.Lerp(joints[i].transform.position, GetMovePosition(joints[i], i, false), jointSpeed * Time.fixedDeltaTime));
                }
                else
                {
                    if (!joints[i].isColliding)
                        if (instantJointMovement)
                            joints[i].rigidbodyComp.MovePosition(GetMovePosition(joints[i], i, false));
                        else
                            joints[i].rigidbodyComp.MovePosition(Vector3.Lerp(joints[i].transform.position, GetMovePosition(joints[i], i, false), jointSpeed * Time.fixedDeltaTime));
                    else
                        joints[i].rigidbodyComp.AddForce((GetMovePosition(joints[i], i, false) - joints[i].transform.position).normalized * 100 * Time.fixedDeltaTime, ForceMode.Force);
                }
            }
            else // Disconnected movement
            {
                joints[i].rigidbodyComp.velocity = Vector3.ClampMagnitude(joints[i].rigidbodyComp.velocity, 10);
            }
        }
    }

    // Re-creates the joints and puts them in their starting positions
    public void Rebuild()
    {
        if (!jointPrefab || !tetherVisualPrefab || !startPoint || !endPoint) return;

        Reset();

        joints = InstantiateMany(jointPrefab, jointCount, JOINT_NAME).ToList();
        tetherVisuals = InstantiateMany(tetherVisualPrefab, jointCount + 1, TETHER_NAME).ToList();

        // Provides initial positioning for the joints
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].transform.position = GetMovePosition(joints[i], i, true);
        }

        // Sets up tether visual references
        for (int i = 0; i < tetherVisuals.Count; i++)
        {
            if (i == 0) // First
            {
                tetherVisuals[i].objectA = startPoint;
                tetherVisuals[i].objectB = joints[i].transform;
            }
            else if (i < joints.Count) // In-between
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
    }

    // Destroys all current references and clears the joint and tether visual list
    public void Reset()
    {
        if (joints != null)
        {
            joints.ForEach(x => DestroyImmediate(x.gameObject));
            joints.Clear();
        }

        if (tetherVisuals != null)
        {
            tetherVisuals.ForEach(x => DestroyImmediate(x.gameObject));
            tetherVisuals.Clear();
        }
    }

    // Used to instantiate many joints and tether visuals (Probably should move to extension methods)
    private IEnumerable<T> InstantiateMany<T>(T prefab, int amount, string name) where T : Component
    {
        for (int i = 0; i < amount; i++)
            yield return Unity.Instantiate<T>(prefab, transform, string.Format("{0} {1}", name, i));
    }

    // Used to determine the position of each joint
    private Vector3 GetMovePosition(TetherJoint joint, int index, bool direct)
    {
        if (direct)
        {
            return Vector3.Lerp(startPoint.position, endPoint.position, ((float)index + 1) / (jointCount + 1));
        }
        else
        {
            if (performanceBoost) // Left it as an option because there may be cases where this fails
            {
                // Better CPU performance but horrendous GC alloc
                tempTetherList = joints.GetRange(0, index);
                tempTetherList.Reverse();
                startTempJoint = tempTetherList.Find(x => x.isColliding && x != joint);
                endTempJoint = joints.GetRange(index, jointCount - index).Find(x => x.isColliding && x != joint);
            }
            else // Whereas this is basically fail proof + better but still bad GC Alloc
            {
                startTempJoint = joints.LastOrDefault(x => x.isColliding && joints.IndexOf(x) < index);
                endTempJoint = joints.FirstOrDefault(x => x.isColliding && joints.IndexOf(x) > index);
            }

            if (experimentalNoStick && joint.isColliding)
            {
                while (startTempJoint && startTempJoint.isColliding)
                {
                    tempJoint = startTempJoint.previousJoint;
                    if (tempJoint) startTempJoint = tempJoint;
                    else break;
                }

                while (endTempJoint && endTempJoint.isColliding)
                {
                    tempJoint = endTempJoint.nextJoint;
                    if (tempJoint) endTempJoint = tempJoint;
                    else break;
                }
            }

            if (startTempJoint && endTempJoint)
            {
                startPointPos = startTempJoint.transform.position;
                endPointPos = endTempJoint.transform.position;
                relativeIndex = index - (joints.IndexOf(startTempJoint) + 1) + 1;
                relativeCount = joints.IndexOf(endTempJoint) - (joints.IndexOf(startTempJoint) + 1) + 1;
            }
            else if (startTempJoint)
            {
                startPointPos = startTempJoint.transform.position;
                endPointPos = endPoint.position;
                relativeIndex = index - (joints.IndexOf(startTempJoint) + 1) + 1;
                relativeCount = jointCount - (joints.IndexOf(startTempJoint) + 1) + 1;
            }
            else if (endTempJoint)
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
    }

    // Determines the direction the start and end points should move in
    // Instead of the first and last maybe change this to the closest colliding joint
    // Although the result will is the same anyway when normalized
    public Vector3 GetStartAndEndMoveDirection(bool isStartPoint)
    {
        if (isStartPoint)
        {
            return (joints.FirstOrDefault().transform.position - startPoint.position).normalized;
        }
        else
        {
            return (joints.LastOrDefault().transform.position - endPoint.position).normalized;
        }
    }

    [System.Obsolete("Use the one that takes a joint instead.")]
    public void Disconnect(int breakJoint)
    {
        Disconnect(joints[Mathf.Clamp(breakJoint, 0, jointCount - 1)]);
    }

    public void Disconnect(TetherJoint breakJoint)
    {
        if (!disconnectionJointPrefab || disconnected || !joints.Contains(breakJoint)) return;

        int breakJointIndex = joints.IndexOf(breakJoint);
        Joint tempJoint;

        for (int i = 0; i < joints.Count; i++)
        {
            // Get the joint prefab and add it
            if (disconnectionJointPrefab.GetComponent<HingeJoint>())
                tempJoint = joints[i].gameObject.AddComponent<HingeJoint>(disconnectionJointPrefab.GetComponent<HingeJoint>());
            else if (disconnectionJointPrefab.GetComponent<SpringJoint>())
                tempJoint = joints[i].gameObject.AddComponent<SpringJoint>(disconnectionJointPrefab.GetComponent<SpringJoint>());
            else if (disconnectionJointPrefab.GetComponent<FixedJoint>())
                tempJoint = joints[i].gameObject.AddComponent<FixedJoint>(disconnectionJointPrefab.GetComponent<FixedJoint>());
            else
                break;

            // Set its connections
            if (i == 0)
            {
                //tempJoint.connectedBody = startPoint.GetComponent<Rigidbody>();
                Destroy(tempJoint);
                joints[i].gameObject.AddComponent<AttachRigidbody>().connectedBody = startPoint;
            }
            else if (i == joints.Count - 1)
            {
                //tempJoint.connectedBody = endPoint.GetComponent<Rigidbody>();
                Destroy(tempJoint);
                joints[i].gameObject.AddComponent<AttachRigidbody>().connectedBody = endPoint;
            }
            else if (i == breakJointIndex)
            {
                tempJoint.connectedBody = joints[i + 1].rigidbodyComp;
                tetherVisuals[breakJointIndex].GetComponent<Renderer>().enabled = false;
            }
            else if (i < breakJointIndex)
            {
                tempJoint.connectedBody = joints[i - 1].rigidbodyComp;
            }
            else if (i > breakJointIndex)
            {
                tempJoint.connectedBody = joints[i + 1].rigidbodyComp;
            }

            // Use gravity
            joints[i].rigidbodyComp.useGravity = true;
        }

        // Set other
        disconnected = true;
        joints[breakJointIndex].disconnectedEnd = true;
        joints[breakJointIndex - 1].disconnectedEnd = true;

        // Add forces
        joints[breakJointIndex].rigidbodyComp.AddForce(Vector3.up * breakForce, ForceMode.VelocityChange);
        joints[breakJointIndex - 1].rigidbodyComp.AddForce(Vector3.up * breakForce, ForceMode.VelocityChange);

        // Raise event
        OnDisconnected(breakJoint);
    }

    public void Reconnect()
    {
        if (!disconnected) return;

        // For the event
        TetherJoint reconnectJoint = null;

        for (int i = 0; i < joints.Count; i++)
        {
            // Remove the joint component
            Destroy(joints[i].GetComponent<Joint>());
            
            // Remove the attaching component
            if (i == 0)
            {
                Destroy(joints[i].GetComponent<AttachRigidbody>());
            }
            else if (i == joints.Count - 1)
            {
                Destroy(joints[i].GetComponent<AttachRigidbody>());
            }

            // Reset other values
            joints[i].rigidbodyComp.velocity = Vector3.zero;
            joints[i].rigidbodyComp.useGravity = false;
            if (!reconnectJoint && joints[i].disconnectedEnd)
                reconnectJoint = joints[i];
            joints[i].disconnectedEnd = false;
        }

        // Set the missing tether visual
        tetherVisuals.First(x => !x.GetComponent<Renderer>().enabled).GetComponent<Renderer>().enabled = true;
        disconnected = false;

        // Raise event
        OnReconnected(reconnectJoint);
    }
}