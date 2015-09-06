using UnityEngine;
using CustomExtensions;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// A Our custom joint object for use in the tether.
/// </summary>[RequireComponent(typeof(Rigidbody))]public class CustomJoint : MonoBehaviour{
    [HideInInspector]
    public Rigidbody rigidBody;

    [SerializeField]
    private CustomJoint next;
    [SerializeField]
    private CustomJoint previous;
    [SerializeField, ReadOnly]
    private int index;    void Awake()    {        rigidBody = GetComponent<Rigidbody>();
    }
}