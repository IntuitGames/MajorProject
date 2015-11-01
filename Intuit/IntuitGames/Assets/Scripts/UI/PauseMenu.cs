using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomExtensions;public class PauseMenu : MonoBehaviour{
    [Header("Components")]
    public Image backgroundPanel;
    public Button resumeButton;
    public Button restartButton;
    public Button exitButton;    void Awake()
    {
        HidePauseMenu();
        GameManager.ModeManager.OnGameModeChanged += CheckForGameModeChange;
    }

    void OnDestroy()
    {
        GameManager.ModeManager.OnGameModeChanged -= CheckForGameModeChange;
    }

    public void ShowPauseMenu()
    {
        backgroundPanel.gameObject.SetActive(true);
        resumeButton.Select();
    }

    public void HidePauseMenu()
    {
        backgroundPanel.gameObject.SetActive(false);
    }    private void CheckForGameModeChange(ModeManager.GameMode newMode, ModeManager.GameMode oldMode)
    {
        if (newMode == ModeManager.GameMode.PauseMenu)
            ShowPauseMenu();
        else if (oldMode == ModeManager.GameMode.PauseMenu)
            HidePauseMenu();
    }

    #region BUTTON ACTIONS

    public void Resume()
    {
        GameManager.ModeManager.RequestGameModeChange(ModeManager.GameMode.InGame, false, 0.1f);
    }

    public void Restart()
    {
        GameManager.ReloadLevel();
    }

    public void Exit()
    {
        GameManager.ExitGame();
    }

    #endregion
}