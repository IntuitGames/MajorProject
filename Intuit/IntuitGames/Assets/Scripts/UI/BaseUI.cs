﻿using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;
using UnityEngine.EventSystems;public abstract class BaseUI : MonoBehaviour, ISelectHandler{
    [SerializeField, ReadOnly]
    protected bool isShown = false;
    [EnumFlags]
    public ModeManager.GameMode shownModes;

    protected virtual void Awake()
    {
        if (shownModes.IsFlagSet<ModeManager.GameMode>(GameManager.ModeManager.currentGameMode))
        {
            Show();
            isShown = true;
        }
        else
        {
            Hide();
            isShown = false;
        }

        GameManager.ModeManager.OnGameModeChanged += this.OnGameModeChanged;
    }

    protected virtual void OnDestroy()
    {
        GameManager.ModeManager.OnGameModeChanged -= this.OnGameModeChanged;
    }

    private void OnGameModeChanged(ModeManager.GameMode newMode, ModeManager.GameMode oldMode)
    {
        ChangeVisibility(shownModes.IsFlagSet<ModeManager.GameMode>(newMode));
    }

    public void ChangeVisibility(bool state)
    {
        if (isShown && !state)
        {
            Hide();
            isShown = false;
        }
        else if (!isShown && state)
        {
            Show();
            isShown = true;
        }
    }

    #region ABSTRACT METHODS

    protected abstract void Show();

    protected abstract void Hide();

    // Called when a new UI element is selected
    public virtual void OnSelect(BaseEventData eventData) { }

    #endregion

    #region COMMON BUTTON BEHAVIOURS

    public virtual void Restart()
    {
        Deselect();

        GameManager.ReloadLevel();
    }

    public virtual void MainMenu()
    {
        Deselect();

        GameManager.LoadMainMenu();
    }

    public virtual void Exit()
    {
        Deselect();

        GameManager.ExitGame();
    }

    public virtual void OptionsMenu()
    {
        Deselect();

        GameManager.ModeManager.RequestGameModeChange(ModeManager.GameMode.Options, false, 0.25f);
    }

    public virtual void LevelSelectMenu()
    {
        Deselect();

        GameManager.ModeManager.RequestGameModeChange(ModeManager.GameMode.LevelSelect, false, 0.25f);
    }

    public virtual void ReturnToLastMenu()
    {
        Deselect();

        GameManager.ModeManager.RequestGameModeChange(GameManager.ModeManager.previousGameMode, false, 0.25f);
    }

    private void Deselect()
    {
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

    }

    #endregion
}