﻿using UnityEngine;
/// Manages all other managers. Contains static references to other managers. Is singleton.
/// </summary>
    // References the instance to the singleton game manager instance
    private static GameManager gameManagerInstance = null;

    // References to other managers                         ---(STEP 1/2: ADD NEW MANAGERS HERE)---
    public static InputManager inputManager;
    public static ModeManager modeManager;
    {
        // Singleton check
        if (!gameManagerInstance)
            gameManagerInstance = this;
        else
            Destroy(this.gameObject);

        // Give this game object persistence across scenes
        DontDestroyOnLoad(this.gameObject);

        // Ensure all manager references are set
        SetManagerReferences();
    }

    private void SetManagerReferences()
    {
        // Find manager references                          ---(STEP 2/2: ADD NEW MANAGERS HERE)---
        SetManager(ref inputManager);
        SetManager(ref modeManager);
    }
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
    }