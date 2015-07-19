using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Base class for all managers.
/// </summary>public class Manager : MonoBehaviour{
    /// <summary>
    /// If true the game manager will attempt to find this manager type in the scene and prioritize that if found.
    /// </summary>
    public virtual bool sceneOverride { get { return false; } }}