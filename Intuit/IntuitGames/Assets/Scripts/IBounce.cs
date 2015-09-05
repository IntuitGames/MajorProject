using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Any object that can bounce.
/// </summary>public interface IBounce : IUnityInterface{
    void Bounce(Vector3 relativeVelocity, GameObject bounceObject);}