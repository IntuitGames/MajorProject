using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;[System.Serializable]public struct FloatRange{
    public float normal;
    public float min;
    public float max;    public float GetRandom()
    {
        return Random.Range(min, max);
    }}