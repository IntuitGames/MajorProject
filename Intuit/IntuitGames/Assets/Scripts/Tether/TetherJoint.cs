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
    public TetherJoint previousJoint
    {
        get
        {
            if (isIndexCached)
                if (index > 0)
                    return GameManager.TetherManager.joints[index - 1];
                else
                    return null;

            index = GameManager.TetherManager.joints.IndexOf(this);
            isIndexCached = true;

            return previousJoint;
        }
    }
    public TetherJoint nextJoint
    {
        get
        {
            if (isIndexCached)
                if (index < GameManager.TetherManager.joints.Count - 1)
                    return GameManager.TetherManager.joints[index + 1];
                else
                    return null;

            index = GameManager.TetherManager.joints.IndexOf(this);
            isIndexCached = true;

            return previousJoint;
        }
    }

    private int index;
    private bool isIndexCached;

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