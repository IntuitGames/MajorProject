﻿using UnityEngine;
/// Base class for all managers.
/// </summary>
    /// <summary>
    /// If true the game manager will attempt to find this manager type in the scene and prioritize that if found.
    /// </summary>
    public virtual bool sceneOverride { get { return false; } }