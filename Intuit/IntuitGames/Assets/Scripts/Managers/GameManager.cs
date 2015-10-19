using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
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
    public static CameraManager CameraManager;    void Awake()
    {
        // Singleton check
        if (!gameManagerInstance)
            gameManagerInstance = this;
        else
            Destroy(this.gameObject);

        // Give this game object persistence across scenes
        //DontDestroyOnLoad(gameManagerInstance.gameObject);               // TEMPORARILY OFF (NOT SCENE PERSISTANT)

        // Ensure all manager references are set
        SetManagerReferences();
    }

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
        // First check to see if this manager should be overridden by another in the scene
        if (managerReference && managerReference.sceneOverride)
        {
            // Save a reference to the old manager
            T oldManager = managerReference;

            // Attempt to find a different manager of the same type in the scene
            managerReference = GameObject.FindObjectsOfType<T>().ToList().FirstOrDefault(x => x != oldManager);

            // If succesful; destroy unneeded manager and return
            if (managerReference && oldManager)
            {
                Destroy(oldManager);
                return;
            }
        }

        // Find on self
        managerReference = GetComponent<T>();

        if (managerReference)
            return;

        // Create one on self
        managerReference = gameObject.AddComponent<T>();

        if (!managerReference)
            Debug.LogWarningFormat("Unable to find or create a {0} manager reference.", typeof(T).Name);
    }    // Update all the timers    void Update()
    {
        TimerPlus.UpdateAll();
    }    // Disable timers that should only exist on one level    void OnLevelWasLoaded(int level)
    {
        TimerPlus.DisposeAllOnLoad();
    }}