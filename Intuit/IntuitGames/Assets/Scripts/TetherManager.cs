﻿using UnityEngine;
using CustomExtensions;
/// A tether system that determines joint positioning based on collisions.
/// </summary>
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
    public bool experimentalWrapping = false;   // Adds force instead of moving if a joint is colliding (Sticky behavior)

    [Header("Info")]
    [ReadOnly]
    public bool disconnected = false;
    [ReadOnly]
    public float directLength;
    [ReadOnly]
    public float tetherLength;
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

        if (!Application.isPlaying) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            Disconnect(jointCount / 2);
        }
    }
    {
        if (!Application.isPlaying) return;

        if (!disconnected)
        {
            for (int i = 0; i < joints.Count; i++)
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
        }
        else // Disconnected movement
        {

        }
    }
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
    }
    {
        joints.ForEach(x => DestroyImmediate(x.gameObject));
        tetherVisuals.ForEach(x => DestroyImmediate(x.gameObject));

        joints.Clear();
        tetherVisuals.Clear();
    }
    {
        for (int i = 0; i < amount; i++)
            yield return Instantiate<T>(prefab);
    }
    {
        if (direct)
        {
            return Vector3.Lerp(startPoint.position, endPoint.position, ((float)index + 1) / (jointCount + 1));
        }
        else
        {
            // Declare locals
            Vector3 startPointPos, endPointPos;
            int relativeIndex, relativeCount;
            TetherJoint startTempJoint, endTempJoint;

            if (performanceBoost) // Left it as an option because there may be cases where this fails
            {
                List<TetherJoint> tempTetherList = joints.GetRange(0, index);
                tempTetherList.Reverse();
                startTempJoint = tempTetherList.Find(x => x.isColliding && x != joint);
                endTempJoint = joints.GetRange(index, jointCount - index).Find(x => x.isColliding && x != joint);
            }
            else // Whereas this is basically fail proof
            {
                startTempJoint = joints.LastOrDefault(x => x.isColliding && joints.IndexOf(x) < index);
                endTempJoint = joints.FirstOrDefault(x => x.isColliding && joints.IndexOf(x) > index);
            }

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
    }
    {
        if(isStartPoint)
        {
            return (joints.FirstOrDefault().transform.position - startPoint.position).normalized;
        }
        else
        {
            return (joints.LastOrDefault().transform.position - endPoint.position).normalized;
        }
    }
    {
        if (!disconnectionJointPrefab || disconnected) return;

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
//                 tempJoint.connectedBody = startPoint.GetComponent<Rigidbody>();
            else if (i == joints.Count - 1)
            {
//                 tempJoint.connectedBody = endPoint.GetComponent<Rigidbody>();
            else if (i == breakJoint)
            {
                tempJoint.connectedBody = joints[i + 1].rigidbodyComp;
                tetherVisuals[breakJoint].GetComponent<Renderer>().enabled = false;
            }
            else if (i < breakJoint)
            {
                tempJoint.connectedBody = joints[i - 1].rigidbodyComp;
            }
            else if (i > breakJoint)
            {
                tempJoint.connectedBody = joints[i + 1].rigidbodyComp;
            }

            // Set other
            Character.character1.Weaken();
            Character.character2.Weaken();
            joints[i].rigidbodyComp.useGravity = true;
            disconnected = true;
        }
    }