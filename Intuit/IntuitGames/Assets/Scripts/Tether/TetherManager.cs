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
    [Popup(new string[] { "Fast CPU | Low GC Alloc", "Very Fast CPU | Very High GC Alloc", "Very Fast and Slow CPU | High GC Alloc" })]
    public string jointSearchMode = "Fast CPU | Low GC Alloc";
    [Range(0, 100)]
    public float breakForce = 50;
    public KeyCode disconnectInput = KeyCode.B;
    public KeyCode reconnectInput = KeyCode.N;
    public bool instantReconnection = true;
    public DynamicThickness dynamicThickness = new DynamicThickness();
    [SerializeField]
    private bool blendColour = true;
    [SerializeField]
    private bool includeOriginal = true;

    [System.Serializable]
    public class DynamicThickness
    {
        public bool enabled;
        [Range(0, 30)]
        public float minLengthThreshold;
        [Range(0, 30)]
        public float maxLengthThreshold;
        [Range(0, 2)]
        public float minScale;
        [Range(0, 2)]
        public float maxScale;

        public DynamicThickness()
        {
            enabled = true;
            minLengthThreshold = 5;
            maxLengthThreshold = 17;
            minScale = 0.1f;
            maxScale = 1;
        }
    }

    [Header("Experimental")]
    public bool experimentalNoStick = false;
    public bool experimentalStickResist = false;
    public bool experimentalWrapping = false;   // Adds force instead of moving if a joint is colliding (Sticky behavior)
    public bool experimentalCollision = false;
    [Range(0, 200)]
    public int collisionBuffer = 30;

    [Header("Audio")]
    public SoundClip disconnectSound = new SoundClip();
    public SoundClip reconnectSound = new SoundClip();

    [Header("Info")]
    [ReadOnly]
    public bool disconnected = false;
    [ReadOnly]
    public float directLength;
    [ReadOnly]
    public float tetherLength;
    [ReadOnly]
    public float collidingJointCount;

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
    private Vector3 originalJointScale;
    private Color originalColour, startColour, endColour;

    public override void ManagerAwake()
    {
        if (!Application.isPlaying) return;

        originalJointScale = joints.SafeGet(0).transform.localScale;

        // Setup collision buffer at the start of the game
        if (experimentalCollision)
        {
            joints.GetRange(0, collisionBuffer).ForEach(x => x.gameObject.IgnoreCollision(startPoint.gameObject));
            joints.GetRange(joints.Count - collisionBuffer, collisionBuffer).ForEach(x => x.gameObject.IgnoreCollision(endPoint.gameObject));
        }

        // Subscribe audio events
        OnDisconnected += (joint) => disconnectSound.PlayDetached(GameManager.CameraManager.audioSourceComp, AudioManager.GetFMODAttribute(joint.transform, joint.rigidbodyComp.velocity), 1, joint.transform);
        OnReconnected += (joint) => reconnectSound.PlayDetached(GameManager.CameraManager.audioSourceComp, AudioManager.GetFMODAttribute(joint.transform, joint.rigidbodyComp.velocity), 1, joint.transform);

        // Fetch colours
        originalColour = joints.SafeGet(0).rendererComp.material.color;
        startColour = startPoint.GetComponent<Character>().bodyRendererComp.material.color.SetAlpha(originalColour.a);
        endColour = endPoint.GetComponent<Character>().bodyRendererComp.material.color.SetAlpha(originalColour.a);

        // Blend colours
        if (blendColour)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                if (!includeOriginal)
                    joints[i].rendererComp.material.color = Color.Lerp(originalColour, endColour, ((float)i).Normalize(0, jointCount, 0, 1));
                else if (i < jointCount / 2)
                    joints[i].rendererComp.material.color = Color.Lerp(originalColour, startColour, Mathf.Abs(((float)i - (jointCount / 2f))).Normalize(0, jointCount / 2f, 0, 1.5f));
                else
                    joints[i].rendererComp.material.color = Color.Lerp(originalColour, endColour, Mathf.Abs(((float)i - (jointCount / 2f))).Normalize(0, jointCount / 2f, 0, 1.5f));
            }
        }
    }

    public override void ManagerOnLevelLoad()
    {
        startPoint = GameManager.PlayerManager.character1.transform;
        endPoint = GameManager.PlayerManager.character2.transform;
        tetherVisuals.SafeGet(0).objectA = startPoint;
        tetherVisuals.SafeGet(jointCount).objectB = endPoint;

        if (disconnected)
            Reconnect();
    }

    void Start()
    {
        // Initialize audio
        disconnectSound.Initialize();
        reconnectSound.Initialize();
    }

    void Update()
    {
        // Basic info
        if (startPoint && endPoint) directLength = Vector3.Distance(startPoint.position, endPoint.position);
        else directLength = default(float);

        // Reference gate
        if (!jointPrefab || !tetherVisualPrefab || !startPoint || !endPoint || !joints.Any() || !tetherVisuals.Any()) return;

        bool changeJointRenderer = joints.SafeGet(0).GetComponent<Renderer>().enabled != showJoints;
        bool changeJointHideFlags = joints.SafeGet(0).gameObject.hideFlags != (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy);
        bool changeTetherHideFlags = tetherVisuals.SafeGet(0).gameObject.hideFlags != (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy);
        float distance = 0;

        for (int i = 0; i < tetherVisuals.Count; i++)
        {
            distance += tetherVisuals[i].distanceBetweenObjects;

            // Tether visual changes
            tetherVisuals[i].rendererComp.enabled = showTetherVisual;

            if (changeTetherHideFlags)
                tetherVisuals[i].gameObject.hideFlags = (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy);

            if (i == joints.Count)
                break;

            // Joint changes
            if (changeJointRenderer)
                joints[i].GetComponent<Renderer>().enabled = showJoints;

            if (changeJointHideFlags)
                joints[i].gameObject.hideFlags = (showInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy);

            if (!Application.isPlaying) continue;

            // Determine angles
            if (i > 0 && i < joints.Count - 1)
                joints[i].angle = joints[i].transform.position.AngleBetweenThreePoints(joints[i].nextJoint.transform.position, joints[i].previousJoint.transform.position) * Mathf.Rad2Deg;
            else if (i == 0)
                joints[i].angle = joints[i].transform.position.AngleBetweenThreePoints(joints[i].nextJoint.transform.position, startPoint.position) * Mathf.Rad2Deg;
            else
                joints[i].angle = joints[i].transform.position.AngleBetweenThreePoints(endPoint.position, joints[i].previousJoint.transform.position) * Mathf.Rad2Deg;

            // Determine scale
            if (dynamicThickness.enabled && tetherLength > dynamicThickness.minLengthThreshold)
            {
                float newScale = !disconnected ? Mathf.Abs(i - (jointCount / 2f)).Normalize(0, (jointCount / 2f),
                    tetherLength.Normalize(dynamicThickness.minLengthThreshold, dynamicThickness.maxLengthThreshold, dynamicThickness.maxScale, dynamicThickness.minScale), dynamicThickness.maxScale)
                    : Mathf.Abs(i - (jointCount / 2f)).Normalize(0, (jointCount / 2f), dynamicThickness.minScale, dynamicThickness.maxScale);
                joints[i].transform.localScale = new Vector3(newScale * originalJointScale.x, newScale * originalJointScale.y, newScale * originalJointScale.z);
            }
            else
                joints[i].transform.localScale = originalJointScale;
        }

        tetherLength = distance;

        // Collision layers
        if (Physics.GetIgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Tether")) != !experimentalCollision)
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Tether"), !experimentalCollision);

        if (!Application.isPlaying) return;

        // Handle debug input
        if (Input.GetKeyDown(disconnectInput))
            Disconnect(joints[jointCount / 2]);
        if (Input.GetKeyDown(reconnectInput))
            Reconnect();
    }

    void FixedUpdate()
    {
        if (!Application.isPlaying) return;
        for (int i = 0; i < joints.Count; i++)
        {
            if (!disconnected) // Connected movement
            {
                joints[i].rigidbodyComp.velocity = Vector3.zero;
                if (experimentalStickResist && joints[i].isColliding)
                    ExperimentalStickResist(joints[i], i);
                else if (experimentalWrapping && joints[i].isColliding) // Experiment wrapping
                    joints[i].rigidbodyComp.velocity = (GetJointMovePosition(joints[i], i, false) - joints[i].transform.position).normalized * jointSpeed * Time.fixedDeltaTime;
                else if (instantJointMovement) // Instantaneous movement
                    joints[i].rigidbodyComp.MovePosition(GetJointMovePosition(joints[i], i, false));
                else // Interpolated movement
                    joints[i].rigidbodyComp.MovePosition(Vector3.Lerp(joints[i].transform.position, GetJointMovePosition(joints[i], i, false), jointSpeed * Time.fixedDeltaTime));
            }
            else // Disconnected movement
            {

            }
        }
    }

    void OnDestroy()
    {
        disconnectSound.Dispose();
        reconnectSound.Dispose();
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
            // Set component references
            joints[i].SetComponentReferences();
            joints[i].transform.position = GetJointMovePosition(joints[i], i, true);
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
    public Vector3 GetJointMovePosition(TetherJoint joint, int index, bool direct)
    {
        if (direct || collidingJointCount <= 0)
        {
            return Vector3.Lerp(startPoint.position, endPoint.position, ((float)index + 1) / (jointCount + 1));
        }
        else
        {
            if (jointSearchMode == "Very Fast CPU | Very High GC Alloc")
            {
                // (ms: 0.6 - 1.3 GC: 118kb)
                tempTetherList = joints.GetRange(0, index);
                tempTetherList.Reverse();
                startTempJoint = tempTetherList.Find(x => x.isColliding && x != joint);
                endTempJoint = joints.GetRange(index, jointCount - index).Find(x => x.isColliding && x != joint);
            }
            else if (jointSearchMode == "Fast CPU | Low GC Alloc")
            {
                // (ms: 1.0 - 1.5 GC: 2.9kb)
                startTempJoint = joint.previousJoint;
                while (startTempJoint && !startTempJoint.isColliding)
                    startTempJoint = startTempJoint.previousJoint;

                endTempJoint = joint.nextJoint;
                while (endTempJoint && !endTempJoint.isColliding)
                    endTempJoint = endTempJoint.nextJoint;
            }
            else
            {
                // (ms: 4.0 - 500.0+ GC: 25kb)
                //startTempJoint = joints.LastOrDefault(x => x.isColliding && joints.IndexOf(x) < index);
                //endTempJoint = joints.FirstOrDefault(x => x.isColliding && joints.IndexOf(x) > index);

                // (ms: 0.4 - 2.8 GC: 17kb)
                startTempJoint = joints.LastOrDefaultInRange(x => x.isColliding && x != joint, 0, index);
                endTempJoint = joints.FirstOrDefaultInRange(x => x.isColliding && x != joint, index + 1, jointCount);
            }

            if (experimentalNoStick)
            {
                while (startTempJoint && startTempJoint.previousJoint && startTempJoint.previousJoint.isColliding)
                    startTempJoint = startTempJoint.previousJoint;

                while (endTempJoint && endTempJoint.nextJoint && endTempJoint.nextJoint.isColliding)
                    endTempJoint = endTempJoint.nextJoint;
            }

            if (startTempJoint && endTempJoint)
            {
                startPointPos = startTempJoint.transform.position;
                endPointPos = endTempJoint.transform.position;
                relativeIndex = index - (startTempJoint.index + 1) + 1;
                relativeCount = endTempJoint.index - (startTempJoint.index + 1) + 1;
            }
            else if (startTempJoint)
            {
                startPointPos = startTempJoint.transform.position;
                endPointPos = endPoint.position;
                relativeIndex = index - (startTempJoint.index + 1) + 1;
                relativeCount = jointCount - (startTempJoint.index + 1) + 1;
            }
            else if (endTempJoint)
            {
                startPointPos = startPoint.position;
                endPointPos = endTempJoint.transform.position;
                relativeIndex = index + 1;
                relativeCount = endTempJoint.index + 1;
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
            {
                reconnectJoint = joints[i];

                // Set the missing tether visual
                tetherVisuals[i].rendererComp.enabled = showTetherVisual;
            }
            joints[i].disconnectedEnd = false;

            if (instantReconnection)
                joints[i].transform.position = GetJointMovePosition(joints[i], i, true);
        }

        disconnected = false;

        // Raise event
        OnReconnected(reconnectJoint);
    }

    private void ExperimentalStickResist(TetherJoint joint, int index)
    {
        Vector3 targetDirection = joint.transform.position - Vector3.Lerp(startPoint.position, endPoint.position, 0.5f).normalized * jointSpeed * Time.fixedDeltaTime;
        RaycastHit hit;
        if (!Physics.SphereCast(joint.transform.position, joint.transform.localScale.x, targetDirection, out hit, targetDirection.magnitude, 1 << 0 | 1 << 8))
            joint.rigidbodyComp.MovePosition(targetDirection);
    }
}