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
    public Transform target;
    public float distance = 5;
    public float smoothTime = 0.3f;
    public bool overrideOffset = false;
    public Vector3 overridenOffsetDirection = new Vector3(0, 0.5f, -1);

    private Vector3 newOffset;
    private Vector3 initialOffsetDirection;
    private Vector3 velocity = Vector3.zero;

    [System.NonSerialized]
    public float initialDistance;

    void Awake()
    {
        initialOffsetDirection = (target.transform.position - transform.position).normalized;
        initialDistance = distance;
    }

    void Update()
    {
        newOffset = overrideOffset ?
            target.transform.position + (overridenOffsetDirection.normalized * distance) :
            target.transform.position + (-initialOffsetDirection * distance);

        transform.position = Vector3.SmoothDamp(transform.position, newOffset, ref velocity, smoothTime);
    }
}
