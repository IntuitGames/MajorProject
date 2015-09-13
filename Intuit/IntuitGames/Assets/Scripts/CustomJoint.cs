using UnityEngine;
using CustomExtensions;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Our custom joint object for use in the tether.
/// </summary>[RequireComponent(typeof(Rigidbody))]public class CustomJoint : MonoBehaviour{
    [HideInInspector]
    public Rigidbody rigidBody;

    public CustomJoint next;
    public CustomJoint previous;

    [ReadOnly]
    public int index;

    public float distanceToNext
    {
        get { return next ? Vector3.Distance(this.transform.position, next.transform.position) : 0; }
    }
    public float distanceToPrevious
    {
        get { return previous ? Vector3.Distance(this.transform.position, previous.transform.position) : 0; }
    }    public Vector3 nextMidPoint
    {
        get { return next ? Vector3.Lerp(transform.position, next.transform.position, 0.5f) : Vector3.zero; }
    }    public Vector3 thisMidPoint
    {
        get { return next && previous ? Vector3.Lerp(previous.transform.position, next.transform.position, 0.5f) : Vector3.zero; }
    }    public Vector3 previousMidPoint
    {
        get { return previous ? Vector3.Lerp(transform.position, previous.transform.position, 0.5f) : Vector3.zero; }
    }    void Awake()    {        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!next || !previous) return;

        Vector3 localForce = (thisMidPoint - transform.position).normalized * DynamicTetherManager.instance.localAppliedForce;
        Vector3 restForce = (DynamicTetherManager.instance.GetRestPosition(index) - transform.position).normalized * DynamicTetherManager.instance.restPositionForce;

        localForce *= DynamicTetherManager.instance.localForceCurve.Evaluate((thisMidPoint - transform.position).magnitude);
        restForce *= DynamicTetherManager.instance.restForceCurve.Evaluate((DynamicTetherManager.instance.GetRestPosition(index) - transform.position).magnitude);

        rigidBody.AddForce(localForce * Time.fixedDeltaTime);
        rigidBody.AddForce(restForce * Time.fixedDeltaTime);

        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, 100);
    }

    void Update()
    {
        if (DynamicTetherManager.instance.alignRotation && next && previous && transform.position - next.transform.position != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(transform.position - next.transform.position);
    }

    public static Vector3 AverageNext(CustomJoint joint)
    {
        Vector3 direction = Vector3.zero;
        CustomJoint nextJoint = joint.next;
        int counter = 0;
        while(nextJoint != null)
        {
            direction += joint.transform.position - nextJoint.transform.position;
            nextJoint = nextJoint.next;
            counter++;
        }
        if (counter > 0) direction /= counter;
        return direction;
    }

    public static Vector3 AveragePrevious(CustomJoint joint)
    {
        Vector3 direction = Vector3.zero;
        CustomJoint previousJoint = joint.previous;
        int counter = 0;
        while (previousJoint != null)
        {
            direction += joint.transform.position - previousJoint.transform.position;
            previousJoint = previousJoint.previous;
            counter++;
        }
        if (counter > 0) direction /= counter;
        return direction;
    }
}