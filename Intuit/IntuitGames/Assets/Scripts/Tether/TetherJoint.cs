using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Custom joint accompanied with the TetherManager script.
/// </summary>[RequireComponent(typeof(Rigidbody), typeof(CollisionMonitor))]public class TetherJoint : MonoBehaviour{
    [HideInInspector]
    public Collider colliderComp;
    [HideInInspector]
    public Rigidbody rigidbodyComp;
    [HideInInspector]
    public CollisionMonitor collisionMonitorComp;
    [HideInInspector]
    public Renderer rendererComp;

    public bool showDebugColour = true;
    public Color debugColour = Color.red;
    private Color normalColour;
    private bool wasColliding;
    [ReadOnly]	public bool passingThroughWeakSpot = false;
    [ReadOnly]
    public bool disconnectedEnd;
    [ReadOnly]
    public float angle;
    public int index
    {
        get
        {
            if (indexCache.HasValue) return indexCache.Value;
            else
            {
                indexCache = GameManager.TetherManager.joints.IndexOf(this);
                return indexCache.Value;
            }
        }
    }
    public TetherJoint previousJoint
    {
        get
        {
            if (prevJointCache) return prevJointCache;
            if (!indexCache.HasValue) indexCache = GameManager.TetherManager.joints.IndexOf(this);
            if (indexCache.HasValue)
            {
                indexCache = GameManager.TetherManager.joints.IndexOf(this);
                if (indexCache > 0)
                {
                    prevJointCache = GameManager.TetherManager.joints[indexCache.Value - 1];
                    return prevJointCache;
                }
            }
            return null;
        }
    }
    public TetherJoint nextJoint
    {
        get
        {
            if (nextJointCache) return nextJointCache;
            if (!indexCache.HasValue) indexCache = GameManager.TetherManager.joints.IndexOf(this);
            if (indexCache.HasValue)
            {
                indexCache = GameManager.TetherManager.joints.IndexOf(this);
                if (indexCache < GameManager.TetherManager.joints.Count - 1)
                {
                    nextJointCache = GameManager.TetherManager.joints[indexCache.Value + 1];
                    return nextJointCache;
                }
            }
            return null;
        }
    }

    private int? indexCache = null;
    private TetherJoint prevJointCache;
    private TetherJoint nextJointCache;

    public bool isColliding { get { return collisionMonitorComp.isColliding; } }

    // Find component references
    void Awake()
    {
        if (!colliderComp)
            colliderComp = GetComponent<Collider>();
        if (!rigidbodyComp)
            rigidbodyComp = GetComponent<Rigidbody>();
        if (!collisionMonitorComp)
            collisionMonitorComp = GetComponent<CollisionMonitor>();
        if (!rendererComp)
            rendererComp = GetComponent<Renderer>();
    }

    void Start()
    {
        collisionMonitorComp.OnCollisionCountChange += UpdateCollisionInfo;

        if (rendererComp) normalColour = rendererComp.material.color;
    }

    private void UpdateCollisionInfo(int newCount)
    {
        SetIsColliding(newCount > 0);
    }    private void SetIsColliding(bool value)
    {
        if (GameManager.TetherManager.experimentalWrapping && wasColliding && !value)
        {
            rigidbodyComp.velocity = Vector3.zero;
            rigidbodyComp.angularVelocity = Vector3.zero;
        }

        if (showDebugColour)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer) renderer.material.color = value ? debugColour : normalColour;
        }

        if (wasColliding && !value)
            GameManager.TetherManager.collidingJointCount--;
        else if (!wasColliding && value)
            GameManager.TetherManager.collidingJointCount++;

        wasColliding = value;
    }	public void DisconnectAtThisJoint()		{					        GameManager.TetherManager.Disconnect(this);		}	public bool IsSevered()		{
        return GameManager.TetherManager.disconnected;		}}