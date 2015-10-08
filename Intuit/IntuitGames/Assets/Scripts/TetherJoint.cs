﻿using UnityEngine;
/// Custom joint accompanied with the TetherManager script.
/// </summary>
    [ReadOnly]
    public bool isColliding;
    public List<Collision> collisionInfoList = new List<Collision>();
    public Collider colliderComp;
    public Rigidbody rigidbodyComp;

#if UNITY_EDITOR
    public bool showDebugColour = true;
    public Color debugColour = Color.red;
    private Color normalColour;

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
    }
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
    }