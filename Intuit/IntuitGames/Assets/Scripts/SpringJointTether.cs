using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simply modifies the transform for this object so that it stretches between to given objects.
/// </summary>
[ExecuteInEditMode]
public class SpringJointTether : MonoBehaviour
{
    // Component references
    public Transform ObjectA;
    public Transform ObjectB;

    private void Awake()
    {
        // Check for reference validity
        if(!ObjectA || !ObjectB)
        {
            Debug.LogWarning("Both objects must be specified.");
            return;
        }
    }

    private void Update()
    {
        if (!ObjectA || !ObjectB)
            return;

        // Determine the scale of the tether
        float DistanceBetweenCharacters = Vector3.Distance(ObjectA.position, ObjectB.position);
        Vector3 ScaleVec = new Vector3(transform.localScale.x, transform.localScale.y, DistanceBetweenCharacters);
        transform.localScale = ScaleVec;

        // Determine the position of the tether (half-way between the characters)
        Vector3 PositionVec = ObjectB.position + (ObjectA.position - ObjectB.position) * 0.5f;
        transform.localPosition = PositionVec;

        // Determine the rotation
        transform.localRotation = Quaternion.LookRotation(ObjectA.position - ObjectB.position);
    }
}
