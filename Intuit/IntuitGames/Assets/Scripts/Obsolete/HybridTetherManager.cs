using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// A tether system that dynamically creates collisional joints but has a static amount of physics joints.
/// NOT FUNCTIONAL
/// </summary>[System.Obsolete]public class HybridTetherManager : MonoBehaviour
{
    #region VARIABLES

    // NESTED
    [System.Serializable]
    public struct HybridJoint
    {
        public Joint physicsJoint { get; set; }
        public List<Rigidbody> collisionalJoints { get; set; }
    }

    // INSPECTOR
    [Header("References")]
    public Joint physicsJointPrefab;
    public Rigidbody collisionalJointPrefab;
    public Rigidbody startObject;
    public Rigidbody endObject;
    [System.NonSerialized]
    public List<HybridJoint> joints;

    [Header("Properties"), Popup(new string[] { "1", "3", "5", "7", "9", "11"})]
    public int physicsJointCount = 5;
    public int collisionJointPoolSize = 250;
    [Range(0, 1)]
    public float minJointDistance = 0.1f;
    [Range(0, 1)]
    public float maxJointDistance = 0.4f;

    #endregion

    void Awake()
    {
        joints = new List<HybridJoint>(physicsJointCount);
    }}