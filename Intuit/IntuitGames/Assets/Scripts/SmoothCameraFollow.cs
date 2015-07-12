using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simply gets the camera to follow a target transform.
/// </summary>
[RequireComponent(typeof(Camera))]
public class SmoothCameraFollow : MonoBehaviour
{
    public Transform Target;
    public float Distance = 5;
    public float SmoothTime = 0.3f;
    public bool OverrideOffset = false;
    public Vector3 OverridenOffsetDirection = new Vector3(0, 0.5f, -1);

    private Vector3 NewOffset;
    private Vector3 InitialOffsetDirection;
    private Vector3 Velocity = Vector3.zero;

    void Awake()
    {
        InitialOffsetDirection = (Target.transform.position - transform.position).normalized;
    }

    void Update()
    {
        NewOffset = OverrideOffset ?
            Target.transform.position + (OverridenOffsetDirection.normalized * Distance) :
            Target.transform.position + (-InitialOffsetDirection * Distance);

        transform.position = Vector3.SmoothDamp(transform.position, NewOffset, ref Velocity, SmoothTime);
    }
}
