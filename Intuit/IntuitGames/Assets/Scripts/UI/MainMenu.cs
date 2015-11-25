using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.UI;public class MainMenu : MonoBehaviour{
    public CoopSelector coopSelector;
    public RectTransform menu;
    public RectTransform levelSelect;
    public RectTransform options;

    [ReadOnly(EditableInEditor = true)]
    public bool remainClickable = false;

    [Header("Components")]
    public Button playButton;
    public Button levelSelectButton;
    public Button optionsButton;
    public Button exitButton;

    private Button.ButtonClickedEvent playButtonEvent, levelSelectButtonEvent, optionsButtonEvent, exitButtonEvent;

    void Awake()
    {
        if (GameManager.ModeManager.currentGameMode != ModeManager.GameMode.MainMenu)
            HideMainMenu();
        else
            ShowMainMenu();

        GameManager.ModeManager.OnGameModeChanged += CheckForGameModeChange;

        // Cache event listeners
        playButtonEvent = playButton.onClick;
        levelSelectButtonEvent = levelSelectButton.onClick;
        optionsButtonEvent = optionsButton.onClick;
        exitButtonEvent = exitButton.onClick;

        // Remove event listeners if buttons can no longer be activated via clicking
        if (!remainClickable)
        {
            playButton.onClick = new Button.ButtonClickedEvent();
            levelSelectButton.onClick = new Button.ButtonClickedEvent();
            optionsButton.onClick = new Button.ButtonClickedEvent();
            exitButton.onClick = new Button.ButtonClickedEvent();
        }
    }

    void OnDestroy()
    {
        GameManager.ModeManager.OnGameModeChanged -= CheckForGameModeChange;
    }

    private void CheckForGameModeChange(ModeManager.GameMode newMode, ModeManager.GameMode oldMode)
    {
        if (newMode == ModeManager.GameMode.MainMenu)
            ShowMainMenu();
        else if (oldMode == ModeManager.GameMode.MainMenu)
            HideMainMenu();
    }

    public void ShowMainMenu()
    {
        // Enable the parent back panel
        menu.gameObject.SetActive(true);

        // Select the default button
        playButton.Select();

        // Enable the coop selector
        coopSelector.SetActive(true);
    }

    public void HideMainMenu()
    {
        // Disable the parent back panel
        menu.gameObject.SetActive(false);

        // Disable the coop selector
        coopSelector.SetActive(false);

        // Deselect
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    #region BUTTON BEHAVIOURS

    // Performs the onClick action for the selected UI
    public void SelectedButtonBehaviour()
    {
        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (selectedObject && selectedObject == playButton.gameObject)
            playButtonEvent.Invoke();
        else if (selectedObject && selectedObject == levelSelectButton.gameObject)
            levelSelectButtonEvent.Invoke();
        else if (selectedObject && selectedObject == optionsButton.gameObject)
            optionsButtonEvent.Invoke();
        else if (selectedObject && selectedObject == exitButton.gameObject)
            exitButtonEvent.Invoke();
    }

    public void Play()
    {
        GameManager.LoadLevel(1);
    }

    public void ShowLevelSelect()
    {
        Debug.Log("Not implemented!");
    }

    public void ShowOptions()
    {
        Debug.Log("Not implemented!");
    }

    public void Exit()
    {
        GameManager.ExitGame();
    }

    #endregion
}