using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;/// <summary>
/// Put on any object that the player can bounce off from.
/// </summary>public class Bouncy : MonoBehaviour{
    public bool isBouncy
    {
        get { return this.enabled; }
        set
        {
            if (GetComponent<Collider>())
                enabled = value;
            else
                enabled = false;
        }
    }

    public float bounceMultiplier = 1;

    public Action OnBounce = delegate { };    void Start()
    {
        if (!GetComponent<Collider>())
        {
            Debug.LogWarning("A bouncy object must have a collider!");
            this.enabled = false;
        }
    }}