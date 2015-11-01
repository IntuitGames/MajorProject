using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// Base class for all managers.
/// </summary>public abstract class Manager : MonoBehaviour{
    public static InputManager InputManager { get { return GameManager.InputManager; } }
    public static ModeManager ModeManager { get { return GameManager.ModeManager; } }
    public static AudioManager AudioManager { get { return GameManager.AudioManager; } }
    public static TetherManager TetherManager { get { return GameManager.TetherManager; } }
    public static PlayerManager PlayerManager { get { return GameManager.PlayerManager; } }
    public static CameraManager CameraManager { get { return GameManager.CameraManager; } }    protected void Awake()
    {
        if (Application.isPlaying && !this.EqualToAny(InputManager, ModeManager, AudioManager, TetherManager, PlayerManager, CameraManager))
            Destroy(this);
    }

    protected void OnLevelWasLoaded(int level)
    {
        if (!this.EqualToAny(InputManager, ModeManager, AudioManager, TetherManager, PlayerManager, CameraManager))
            Destroy(this);
    }

    public virtual void ManagerAwake() { }
    public virtual void ManagerOnLevelLoad() { }}