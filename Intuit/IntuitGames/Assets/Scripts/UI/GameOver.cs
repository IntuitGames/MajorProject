using UnityEngine;using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;public class GameOver : MonoBehaviour{
    public CoopSelector coopSelector;
    public Image backgroundPanel;
    public Button restartButton;
    public Button exitButton;

    [ReadOnly(EditableInEditor = true)]
    public bool remainClickable = false;

    private Button.ButtonClickedEvent restartButtonEvent, exitButtonEvent;

    void Awake()
    {
        HideGameOverMenu();
        GameManager.ModeManager.OnGameModeChanged += CheckForGameModeChange;

        // Cache event listeners
        restartButtonEvent = restartButton.onClick;
        exitButtonEvent = exitButton.onClick;

        // Remove event listeners if buttons can no longer be activated via clicking
        if (!remainClickable)
        {
            restartButton.onClick = new Button.ButtonClickedEvent();
            exitButton.onClick = new Button.ButtonClickedEvent();
        }
    }

    void OnDestroy()
    {
        GameManager.ModeManager.OnGameModeChanged -= CheckForGameModeChange;
    }

    public void ShowGameOverMenu()
    {
        // Enable the parent back panel
        backgroundPanel.gameObject.SetActive(true);

        // Select the default button
        restartButton.Select();

        // Enable the coop selector
        coopSelector.SetActive(true);
    }

    public void HideGameOverMenu()
    {
        // Disable the parent back panel
        backgroundPanel.gameObject.SetActive(false);

        // Disable the coop selector
        coopSelector.SetActive(false);

        // Deselect
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    private void CheckForGameModeChange(ModeManager.GameMode newMode, ModeManager.GameMode oldMode)
    {
        if (newMode == ModeManager.GameMode.GameOver)
            ShowGameOverMenu();
        else if (oldMode == ModeManager.GameMode.GameOver)
            HideGameOverMenu();
    }

    #region BUTTON BEHAVIOURS

    // Performs the onClick action for the selected UI
    public void SelectedButtonBehaviour()
    {
        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (selectedObject && selectedObject == restartButton.gameObject)
            restartButtonEvent.Invoke();
        else if (selectedObject && selectedObject == exitButton.gameObject)
            exitButtonEvent.Invoke();
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