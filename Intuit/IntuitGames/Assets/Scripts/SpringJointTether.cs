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
    public Transform objectA;
    public Transform objectB;

    private void Awake()
    {
        // Check for reference validity
        if(!objectA || !objectB)
        {
            Debug.LogWarning("Both objects must be specified.");
            return;
        }
    }

    private void Update()
    {
        if (!objectA || !objectB)
            return;

        // Determine the scale of the tether
        float DistanceBetweenCharacters = Vector3.Distance(objectA.position, objectB.position);
        Vector3 ScaleVec = new Vector3(transform.localScale.x, transform.localScale.y, DistanceBetweenCharacters);
        transform.localScale = ScaleVec;

        // Determine the position of the tether (half-way between the characters)
        Vector3 PositionVec = objectB.position + (objectA.position - objectB.position) * 0.5f;
        transform.localPosition = PositionVec;

        // Determine the rotation
        transform.localRotation = Quaternion.LookRotation(objectA.position - objectB.position);
    }
}
