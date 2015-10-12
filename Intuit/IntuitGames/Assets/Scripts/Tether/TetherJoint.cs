using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Custom joint accompanied with the TetherManager script.
/// </summary>[RequireComponent(typeof(Rigidbody))]public class TetherJoint : MonoBehaviour{
    [ReadOnly]
    public bool isColliding;
    public List<Collision> collisionInfoList = new List<Collision>();
    public Collider colliderComp;
    public Rigidbody rigidbodyComp;

    public bool showDebugColour = true;
    public Color debugColour = Color.red;
    private Color normalColour;
    [ReadOnly]	public bool passingThroughWeakSpot = false;
    [ReadOnly]
    public bool disconnectedEnd;
    private static TetherManager tetherManager;
    public TetherJoint previousJoint
    {
        get { if (tetherManager.joints.IndexOf(this) > 0) return tetherManager.joints[tetherManager.joints.IndexOf(this) - 1]; else return null; }
    }
    public TetherJoint nextJoint
    {
        get { if (tetherManager.joints.IndexOf(this) < tetherManager.joints.Count - 1) return tetherManager.joints[tetherManager.joints.IndexOf(this) + 1]; else return null; }
    }

    void Awake()
    {
        if (!tetherManager) tetherManager = FindObjectOfType<TetherManager>();
    }

#if UNITY_EDITOR
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer) normalColour = renderer.material.color;
    }
#endif

    void OnCollisionEnter(Collision col)
    {
        if(!collisionInfoList.Any(x => x.gameObject == col.gameObject))
        {
            collisionInfoList.Add(col);
            SetIsColliding(true);
        }
    }

    void OnCollisionExit(Collision col)
    {
        collisionInfoList.RemoveAll(x => x.gameObject == col.gameObject);

        if (collisionInfoList.Count <= 0)
            SetIsColliding(false);
    }    private void SetIsColliding(bool value)
    {
        if (tetherManager.experimentalWrapping && isColliding && !value)
        {
            rigidbodyComp.velocity = Vector3.zero;
            rigidbodyComp.angularVelocity = Vector3.zero;
        }

        isColliding = value;

#if UNITY_EDITOR
        Renderer renderer = GetComponent<Renderer>();
        if (renderer) renderer.material.color = value ? debugColour : normalColour;
#endif
    }}