﻿using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Specifies the surface type of attached game object.
/// </summary>public class Surface : MonoBehaviour{
    public enum SurfaceType { None = 0, Dirt = 1, Gravel = 2, CobbleStone = 3, Grass = 4, Wood = 5, Metal = 6 }

    [SerializeField]
    private SurfaceType _type = SurfaceType.Dirt;
    public SurfaceType type
    {
        get
        {
            return enabled ? _type : SurfaceType.None;
        }
        set
        {
            _type = value;
        }
    }

    public bool isWet = false;

    void Start() { }}