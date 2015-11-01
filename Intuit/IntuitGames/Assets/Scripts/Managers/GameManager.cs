using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// Manages all other managers. Contains static references to other managers. Is singleton.
/// </summary>public class GameManager : MonoBehaviour{
    // References the instance to the singleton game manager instance
    private static GameManager gameManagerInstance = null;

    // References to other managers                         ---(STEP 1/4: ADD NEW MANAGERS HERE)---
    public static InputManager InputManager;
    public static ModeManager ModeManager;
    public static AudioManager AudioManager;
    public static TetherManager TetherManager;
    public static PlayerManager PlayerManager;
    public static CameraManager CameraManager;

    #region MESSAGES

    void Awake()
    {
        // Singleton check
        if (!gameManagerInstance)
        {
            gameManagerInstance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        // Ensure all manager references are set
        SetManagerReferences();

        // Sub-manager awake calls must be after references are set
        InvokeManagerAwake();
    }

    // Update all the timers
    void Update()
    {
        TimerPlus.UpdateAll();
    }

    // Disable timers that should only exist on one level
    void OnLevelWasLoaded(int level)
    {
        TimerPlus.DisposeAllOnLoad();

        InvokeManagerOnLevelLoad();
    }

    // Disable timers that should only exist on one level
    void OnDestroy()
    {
        if (this == gameManagerInstance)
            TimerPlus.DisposeAllOnLoad();
    }

    #endregion

    #region MANAGER MANAGEMENT

    private void SetManagerReferences()
    {
        // Find manager references                          ---(STEP 2/4: ADD NEW MANAGERS HERE)---
        SetManager<InputManager>(ref InputManager);
        SetManager<ModeManager>(ref ModeManager);
        SetManager<AudioManager>(ref AudioManager);
        SetManager<TetherManager>(ref TetherManager);
        SetManager<PlayerManager>(ref PlayerManager);
        SetManager<CameraManager>(ref CameraManager);

        // STEP 3/4: Add in GameManageEditor script
        // STEP 4/4: Add in Manager base class script
    }    private void SetManager<T>(ref T managerReference) where T: Manager
    {
        // Check if the manager already exists
        if (managerReference)
            return;

        // Find on self
        managerReference = GetComponent<T>();

        if (managerReference)
            return;

        // Create one on self
        managerReference = gameObject.AddComponent<T>();

        if (!managerReference)
            Debug.LogWarningFormat("Unable to find or create a {0} manager reference.", typeof(T).Name);
    }    // Calls manager awake methods    private void InvokeManagerAwake()
    {
        InputManager.ManagerAwake();
        ModeManager.ManagerAwake();
        AudioManager.ManagerAwake();
        TetherManager.ManagerAwake();
        PlayerManager.ManagerAwake();
        CameraManager.ManagerAwake();
    }    // Calls manager on level load methods    private void InvokeManagerOnLevelLoad()
    {
        InputManager.ManagerOnLevelLoad();
        ModeManager.ManagerOnLevelLoad();
        AudioManager.ManagerOnLevelLoad();
        TetherManager.ManagerOnLevelLoad();
        PlayerManager.ManagerOnLevelLoad();
        CameraManager.ManagerOnLevelLoad();
    }

    #endregion

    #region PUBLIC STATICS

    public static void ExitGame()
    {
#if !UNITY_EDITOR
        Application.Quit();
#else
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public static void ReloadLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    #endregion
}