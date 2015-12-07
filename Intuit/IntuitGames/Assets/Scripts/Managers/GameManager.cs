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

    // Events
    public static event System.Action OnApplicationExit = delegate { };

    // Static Data
    public static Resolution[] resolutions { private set; get;}
    public static int currentResolutionIndex { private set; get;}
    public static string[] qualityLevels { private set; get;}
    public static int currentQualityIndex { private set; get;}

    // Quick-access Properties
    public static string currentResolutionDisplay
    {
        get { return ResolutionDisplayString(resolutions[currentResolutionIndex]); }
    }
    public static string[] allResolutionsDisplay
    {
        get { return ResolutionDisplayString(resolutions).ToArray(); }
    }
    public static string currentQualityLevelDisplay
    {
        get { return qualityLevels[currentQualityIndex]; }
    }

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

        // Get resolution and quality settings data
        Screen.fullScreen = PlayerPrefs.GetInt("IsFullScreen", Screen.fullScreen ? 1 : 0) == 1;

        resolutions = Screen.resolutions.ToArray();
        qualityLevels = QualitySettings.names;

        if (!Application.isEditor)
        {
            // For some reason Screen.currentResolution is incorrect when not in full screen
            if (Screen.fullScreen)
                currentResolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt("ResolutionIndex", System.Array.IndexOf<Resolution>(resolutions, Screen.currentResolution)), 0, resolutions.Length - 1);
            else
                currentResolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt("ResolutionIndex", System.Array.IndexOf<Resolution>(resolutions, new Resolution() { width = Screen.width, height = Screen.height, refreshRate = Screen.currentResolution.refreshRate })), 0, resolutions.Length - 1);
        }
        else // Only a single resolution in editor
            currentResolutionIndex = 0;
        currentQualityIndex = Mathf.Clamp(PlayerPrefs.GetInt("QualityIndex", QualitySettings.GetQualityLevel()), 0, qualityLevels.Length - 1);

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

    #region HELPER METHODS

    // Returns a nicely formatted string of a resolution
    private static string ResolutionDisplayString(Resolution resolution)
    {
        return string.Format("{0} x {1}", resolution.height, resolution.width);
    }

    // Returns a nicely formatted string of an array of resolutions
    private static IEnumerable<string> ResolutionDisplayString(params Resolution[] resolutions)
    {
        foreach (var res in resolutions)
            yield return ResolutionDisplayString(res);
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
        OnApplicationExit();

#if !UNITY_EDITOR
        Application.Quit();
#else
        PlayerPrefs.Save();
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public static void ReloadLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public static void LoadLevel(int number)
    {
        Application.LoadLevel(number);
    }

    public static void LoadMainMenu()
    {
        Application.LoadLevel(0);
    }

    public static void SetResolution(int requestedIndex, bool isFull)
    {
        currentResolutionIndex = requestedIndex;
        Screen.SetResolution(resolutions[currentResolutionIndex].width, resolutions[currentResolutionIndex].height, isFull);
        //PlayerPrefs.SetInt("ResolutionIndex", requestedIndex);
    }

    public static void SetQualityLevel(int requestedIndex)
    {
        if (requestedIndex == currentQualityIndex) return;

        currentQualityIndex = requestedIndex;
        QualitySettings.SetQualityLevel(currentQualityIndex, true);
        PlayerPrefs.SetInt("QualityIndex", requestedIndex);
    }

    #endregion
}