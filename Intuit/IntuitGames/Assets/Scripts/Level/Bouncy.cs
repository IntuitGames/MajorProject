using UnityEngine;
using CustomExtensions;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;/// <summary>
/// Put on any object that the player can bounce off from.
/// </summary>public class Bouncy : MonoBehaviour{
    public float bounceMultiplier = 1;
    public float velocityThreshold = 0;
    [Range(0, 100)]
    public float minBounceMagnitude = 11;
    [Range(0, 100)]
    public float maxBounceMagnitude = 25;

    public event Action<Collision, IBounce> OnBounce = delegate { };

    void Start() { }    void OnCollisionEnter(Collision col)
    {
        IBounce bounceHit = col.collider.gameObject.GetInterface<IBounce>();

        if (enabled && bounceHit != null)
        {
            bounceHit.Bounce(col.relativeVelocity, this.gameObject);
            OnBounce(col, bounceHit);
        }
    }}