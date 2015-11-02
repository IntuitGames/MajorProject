using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simply modifies the transform for this object so that it stretches between to given objects.
/// </summary>
[ExecuteInEditMode]
public class DebugTetherVisual : MonoBehaviour
{
    // Component references
    public Transform objectA;
    public Transform objectB;
    public bool useLocal = false;

    public bool debugStrainColours = true;
    public Color normalColour = Color.white;
    public Color strainedColour = Color.red;
    [Range(0, 25)]
    public float closeDistance = 3;
    [Range(0, 25)]
    public float farDistance = 15;

    [ReadOnly]
    public float distanceBetweenObjects;

    [HideInInspector]
    public Renderer rendererComp;
    private Vector3 lastA;
    private Vector3 lastB;

    void Start()
    {
        if (!rendererComp)
            rendererComp = GetComponent<Renderer>();
    }

    void Update()
    {
        if (!objectA || !objectB)
            return;

        distanceBetweenObjects = Vector3.Distance(objectA.position, objectB.position);

        // No need to update if no movement took place
        if (lastA == objectA.position && lastB == objectB.position)
            return;

        // Determine the scale of the tether
        Vector3 ScaleVec = new Vector3(transform.localScale.x, transform.localScale.y, distanceBetweenObjects);
        transform.localScale = ScaleVec;

        // Determine the position of the tether (half-way between the characters)
        Vector3 PositionVec;
        if(!useLocal) PositionVec = objectB.position + (objectA.position - objectB.position) * 0.5f;
        else PositionVec = objectB.localPosition + (objectA.localPosition - objectB.localPosition) * 0.5f;
        transform.localPosition = PositionVec;

        // Determine the rotation
        if (objectA.position - objectB.position != Vector3.zero)
            transform.localRotation = Quaternion.LookRotation(objectA.position - objectB.position);

        // Apply debug color if game is playing
        if (debugStrainColours && Application.isPlaying)
        {
            Material mat = rendererComp.material;
            if (mat)
            {
                if (distanceBetweenObjects < closeDistance)
                    mat.color = normalColour;
                else if (distanceBetweenObjects < farDistance)
                    mat.color = Color.Lerp(normalColour, strainedColour, (distanceBetweenObjects - closeDistance) / (farDistance - closeDistance));
                else
                    mat.color = strainedColour;
            }
        }

        lastA = objectA.position;
        lastB = objectB.position;
    }
}
