using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Base class for all managers.
/// </summary>public abstract class Manager : MonoBehaviour{
    /// <summary>
    /// If true the game manager will attempt to find this manager type in the scene and prioritize that if found.
    /// </summary>
    public virtual bool sceneOverride { get { return false; } }

    public static InputManager InputManager { get { return GameManager.InputManager; } }
    public static ModeManager ModeManager { get { return GameManager.ModeManager; } }
    public static AudioManager AudioManager { get { return GameManager.AudioManager; } }
    public static TetherManager TetherManager { get { return GameManager.TetherManager; } }
    public static PlayerManager PlayerManager { get { return GameManager.PlayerManager; } }
    public static CameraManager CameraManager { get { return GameManager.CameraManager; } }}