﻿using UnityEngine;
using System;
/// Manages the current mode of the game.
/// </summary>
    public enum GameMode { MainMenu, PauseMenu, InGame };

    private GameMode _currentGameMode = GameMode.InGame;
    public GameMode currentGameMode
    {
        get { return _currentGameMode; }
        set
        {
            if(value != currentGameMode)
            {
                OnGameModeChanged(value, currentGameMode);
                _currentGameMode = value;
            }
        }
    }

    public event Action<GameMode, GameMode> OnGameModeChanged = delegate { };