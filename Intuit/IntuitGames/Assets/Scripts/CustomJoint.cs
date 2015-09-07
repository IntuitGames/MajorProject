using UnityEngine;
using CustomExtensions;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// A Our custom joint object for use in the tether.
/// </summary>[RequireComponent(typeof(Rigidbody))]public class CustomJoint : MonoBehaviour{
    [HideInInspector]
    public Rigidbody rigidBody;

    public CustomJoint next;
    public CustomJoint previous;

    [ReadOnly]
    public int index;
#pragma warning disable 414
    [SerializeField, ReadOnly]
    private float _distanceToNext;
    [SerializeField, ReadOnly]
    private float _distanceToPrevious;
#pragma warning restore 414

    public float distanceToNext
    {
        get { return next ? Vector3.Distance(this.transform.position, next.transform.position) : 0; }
    }
    public float distanceToPrevious
    {
        get { return previous ? Vector3.Distance(this.transform.position, previous.transform.position) : 0; }
    }    public Vector3 nextMidPoint
    {
        get { return next ? next.transform.position + (transform.position - next.transform.position) * 0.5f : Vector3.zero; }
    }    public Vector3 previousMidPoint
    {
        get { return previous ? previous.transform.position + (transform.position - previous.transform.position) * 0.5f : Vector3.zero; }
    }    void Awake()    {        rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _distanceToNext = distanceToNext;
        _distanceToPrevious = distanceToPrevious;
    }
}