using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;
using CustomExtensions;/// <summary>
/// Manages the current mode of the game.
/// </summary>public class ModeManager : Manager{
    public enum GameMode { None, MainMenu, PauseMenu, InGame };

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
    public float modeChangeCooldown = 0.1f;

    public event Action<GameMode, GameMode> OnGameModeChanged = delegate { };

    void Start() { }

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
        else
            Time.timeScale = normalTimeScale;
    }}