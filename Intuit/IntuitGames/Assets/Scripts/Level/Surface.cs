using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Specifies the surface type of attached game object.
/// </summary>public class Surface : MonoBehaviour{
    public enum SurfaceTypes { Default, None, Grass, Stone, Metal, Dirt, Gravel, Water, Wood }

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