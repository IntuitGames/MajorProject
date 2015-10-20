using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Custom joint accompanied with the TetherManager script.
/// </summary>[RequireComponent(typeof(Rigidbody))]public class TetherJoint : MonoBehaviour{
    [ReadOnly]
    public bool isColliding;
    public List<Collider> collisionList = new List<Collider>();
    public Collider colliderComp;
    public Rigidbody rigidbodyComp;

    public bool showDebugColour = true;
    public Color debugColour = Color.red;
    private Color normalColour;
    [ReadOnly]	public bool passingThroughWeakSpot = false;
    [ReadOnly]
    public bool disconnectedEnd;
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

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer) normalColour = renderer.material.color;
    }

    void OnCollisionEnter(Collision col)
    {
        if(!collisionList.Any(x => x.gameObject == col.gameObject))
        {
            if (collisionList.Count <= 0)
                SetIsColliding(true);

            collisionList.Add(col.collider);
        }
    }

    void OnCollisionExit(Collision col)
    {
        collisionList.Remove(col.collider);

        if (collisionList.Count <= 0)
            SetIsColliding(false);
    }    private void SetIsColliding(bool value)
    {
        if (GameManager.TetherManager.experimentalWrapping && isColliding && !value)
        {
            rigidbodyComp.velocity = Vector3.zero;
            rigidbodyComp.angularVelocity = Vector3.zero;
        }

        isColliding = value;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer) renderer.material.color = value ? debugColour : normalColour;
    }	public void DisconnectAtThisJoint()		{				if (GameManager.TetherManager.disconnected) return;						int thisIndex = GameManager.TetherManager.joints.IndexOf(this);				if(thisIndex > 0) GameManager.TetherManager.Disconnect(this);		}	public bool IsSevered()		{				if(GameManager.TetherManager.disconnected) return true; else return false;		}}