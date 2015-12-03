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
    private Camera cameraComp;

    public Transform target;
    public float distance = 5;
    public float smoothTime = 0.3f;
    public bool overrideOffset = false;
    public Vector3 overridenOffsetDirection = new Vector3(0, 0.5f, -1);

    private Vector3 newOffset;
    private Vector3 initialOffsetDirection;
    private Vector3 zoomVelocity;
    private float FOVSpeed;

    [System.NonSerialized]
    public Transform initialTarget;
    [System.NonSerialized]
    public float initialDistance;
    [System.NonSerialized]
    public float initialFOV;
    [System.NonSerialized]
    public float targetFOV;

    void Awake()
    {
        cameraComp = GetComponent<Camera>();
        initialOffsetDirection = (target.transform.position - transform.position).normalized;
        initialTarget = target;
        initialDistance = distance;
        initialFOV = cameraComp.fieldOfView;
        targetFOV = initialFOV;
    }

    void Update()
    {
        newOffset = overrideOffset ?
            target.transform.position + (overridenOffsetDirection.normalized * distance) :
            target.transform.position + (-initialOffsetDirection * distance);

        transform.position = Vector3.SmoothDamp(transform.position, newOffset, ref zoomVelocity, smoothTime);

        cameraComp.fieldOfView = Mathf.SmoothDamp(cameraComp.fieldOfView, targetFOV, ref FOVSpeed, smoothTime);
    }
}
