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
    public Button mainMenuButton;
    public Button exitButton;
    public Slider volumeSlider;    void Awake()
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
        // Enable the parent back panel
        backgroundPanel.gameObject.SetActive(true);

        volumeSlider.value = GameManager.AudioManager.masterVolume;

        // Select the default button
        resumeButton.Select();
    }

    public void HidePauseMenu()
    {
        // Disable the parent back panel
        backgroundPanel.gameObject.SetActive(false);

        // Deselect
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
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

    public void MainMenu()
    {
        GameManager.LoadMainMenu();
    }

    public void Exit()
    {
        GameManager.ExitGame();
    }

    public void OnVolumeSliderChange()
    {
        GameManager.AudioManager.masterVolume = volumeSlider.value;
    }

    #endregion
}