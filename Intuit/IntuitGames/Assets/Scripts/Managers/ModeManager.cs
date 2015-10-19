using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;/// <summary>
/// Manages the current mode of the game.
/// </summary>public class ModeManager : Manager{
    public enum GameMode { None, MainMenu, PauseMenu, InGame };

    public GameMode initialGameMode = GameMode.InGame;
    [System.NonSerialized]
    private GameMode _currentGameMode = GameMode.None;
    public GameMode currentGameMode
    {
        get { return _currentGameMode; }
        set
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

    public event Action<GameMode, GameMode> OnGameModeChanged = delegate { };    void Start()
    {
        currentGameMode = initialGameMode;
    }}