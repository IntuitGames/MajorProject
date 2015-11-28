using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Specifies the surface type of attached game object.
/// </summary>public class Surface : MonoBehaviour{
    public enum SurfaceTypes : int { Default = 1, None = 0, Dirt = 1, Water = 2, Wood = 3, Metal = 4, Gravel = 5, LongGrass = 6, Grass = 7 }

    [SerializeField]
    private SurfaceTypes _type = SurfaceTypes.Default;
    public SurfaceTypes type
    {
        get
        {
            return enabled ? _type : SurfaceTypes.None;
        }
        set
        {
            _type = value;
        }
    }

    void Start() { }}