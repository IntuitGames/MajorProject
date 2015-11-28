using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;
using CustomExtensions;/// <summary>
/// Manages the current mode of the game.
/// </summary>public class ModeManager : Manager{
    public enum GameMode { None = 0, MainMenu = 1 << 1, PauseMenu = 1 << 2, InGame = 1 << 3, GameOver = 1 << 4 };

    public GameMode initialGameMode = GameMode.InGame;
    [System.NonSerialized]
    private GameMode _currentGameMode = GameMode.None;
    public GameMode currentGameMode
    {
        get { return _currentGameMode; }
        private set
        {
            if(value != currentGameMode)
            {
                previousGameMode = currentGameMode;
                _currentGameMode = value;
                OnGameModeChanged(currentGameMode, previousGameMode);
            }
        }
    }
    public GameMode previousGameMode { get; private set; }
    public bool canChangeMode { get; private set; }

    [Range(0, 1)]
    public float normalTimeScale = 1;
    [Range(0, 1)]
    public float pauseTimeScale = 0;
    [Range(0, 1)]
    public float gameOverTimeScale = 0.5f;
    public float modeChangeCooldown = 0.1f;

    public float ingameTimeTotal;
    public float ingameTimeLevel;

    // NewMode, OldMode
    public event Action<GameMode, GameMode> OnGameModeChanged = delegate { };

    void Start() { }

    void Update()
    {
        // Counts time spent in-game
        if (currentGameMode == GameMode.InGame)
        {
            ingameTimeLevel += Time.deltaTime;
            ingameTimeTotal += Time.deltaTime;
        }
    }

    void OnDestroy()
    {
        // Get the true mode manager and force a switch to this scene's starter
        GameManager.ModeManager.RequestGameModeChange(initialGameMode, true, 0);
    }

    public override void ManagerAwake()
    {
        OnGameModeChanged += HandleTimeScale;
        RequestGameModeChange(initialGameMode, true, 0);
        canChangeMode = true;
    }

    public override void ManagerOnLevelLoad()
    {
        // Reset in-game timer
        ingameTimeLevel = 0;
    }    public void RequestGameModeChange(GameMode newMode, bool force, float delay)
    {
        if (canChangeMode && newMode != currentGameMode && enabled || force && newMode != currentGameMode)
        {
            canChangeMode = false;

            if (delay > 0)
            {
                TimerPlus.Create(delay, TimerPlus.Presets.BackgroundOneTimeUse, () => currentGameMode = newMode);
                TimerPlus.Create(delay + modeChangeCooldown, TimerPlus.Presets.BackgroundOneTimeUse, () => canChangeMode = true);
            }
            else
            {
                currentGameMode = newMode;
                TimerPlus.Create(modeChangeCooldown, TimerPlus.Presets.BackgroundOneTimeUse, () => canChangeMode = true);
            }
        }
    }    private void HandleTimeScale(GameMode newMode, GameMode oldMode)    {
        if (newMode == GameMode.PauseMenu)
            Time.timeScale = pauseTimeScale;
        else if (newMode == GameMode.GameOver)
            Time.timeScale = gameOverTimeScale;
        else
            Time.timeScale = normalTimeScale;
    }}