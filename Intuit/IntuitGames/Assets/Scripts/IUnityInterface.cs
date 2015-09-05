using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Base class for Unity interfaces. Allows for easy conversion between interface types and game object types.
/// </summary>public interface IUnityInterface{
    GameObject gameObject { get; }}