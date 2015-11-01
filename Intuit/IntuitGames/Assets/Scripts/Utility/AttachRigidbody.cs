using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;[RequireComponent(typeof(Rigidbody))]public class AttachRigidbody : MonoBehaviour{
    private Rigidbody rigidbodyComp;
    public Transform connectedBody;
    public Vector3 offset;    void Start()
    {
        rigidbodyComp = GetComponent<Rigidbody>();
    }    void FixedUpdate()
    {
        if (connectedBody)
        {
            rigidbodyComp.isKinematic = true;
            rigidbodyComp.MovePosition(connectedBody.transform.position + offset);
        }
    }}