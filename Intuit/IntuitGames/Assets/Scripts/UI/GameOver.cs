﻿using UnityEngine;using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.EventSystems;public class GameOver : BaseUI{
    [Header("Components")]
    public CoopSelector coopSelector;
    public Image backgroundPanel;
    public Button restartButton;
    public Button mainMenuButton;
    public Button exitButton;

    [Header("Settings")]
    [ReadOnly(EditableInEditor = true)]
    public bool remainClickable = false;
    public SoundClip onSelectedSFX = new SoundClip();

    private Button.ButtonClickedEvent restartButtonEvent, mainMenuEvent, exitButtonEvent;

    protected override void Awake()
    {
        base.Awake();

        // Cache event listeners
        restartButtonEvent = restartButton.onClick;
        mainMenuEvent = mainMenuButton.onClick;
        exitButtonEvent = exitButton.onClick;

        // Remove event listeners if buttons can no longer be activated via clicking
        if (!remainClickable)
        {
            restartButton.onClick = new Button.ButtonClickedEvent();
            mainMenuButton.onClick = new Button.ButtonClickedEvent();
            exitButton.onClick = new Button.ButtonClickedEvent();
        }
    }

    void Start()
    {
        onSelectedSFX.Initialize();
    }

    protected override void Show()
    {
        // Enable the parent back panel
        backgroundPanel.gameObject.SetActive(true);

        // Select the default button
        restartButton.Select();

        // Enable the coop selector
        coopSelector.SetActive(true);
    }

    protected override void Hide()
    {
        // Disable the parent back panel
        backgroundPanel.gameObject.SetActive(false);

        // Disable the coop selector
        coopSelector.SetActive(false);

        // Deselect
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    // Performs the onClick action for the selected UI
    public void SelectedButtonBehaviour()
    {
        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (selectedObject && selectedObject == restartButton.gameObject)
            restartButtonEvent.Invoke();
        else if (selectedObject && selectedObject == mainMenuButton.gameObject)
            mainMenuEvent.Invoke();
        else if (selectedObject && selectedObject == exitButton.gameObject)
            exitButtonEvent.Invoke();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        onSelectedSFX.PlayAttached(GetComponent<AudioSource>(), AudioManager.GetFMODAttribute(eventData.selectedObject.transform, Vector3.zero), 1);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        onSelectedSFX.Dispose();
    }
}