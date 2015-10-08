﻿using UnityEngine;
    private Rigidbody rigidbodyComp;
    public Transform connectedBody;
    public Vector3 offset;
    {
        rigidbodyComp = GetComponent<Rigidbody>();
    }
    {
        if (connectedBody)
        {
            rigidbodyComp.isKinematic = true;
            rigidbodyComp.MovePosition(connectedBody.transform.position + offset);
        }
    }