using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;public class MainMenu : BaseUI{
    [Header("Components")]
    public CoopSelector coopSelector;
    public RectTransform menu;
    public RectTransform levelSelect;
    public RectTransform options;

    [Space(10)]
    public Button playButton;
    public Button levelSelectButton;
    public Button optionsButton;
    public Button exitButton;

    [Header("Settings")]
    [ReadOnly(EditableInEditor = true)]
    public bool remainClickable = false;
    public SoundClip onSelectedSFX = new SoundClip();

    private Button.ButtonClickedEvent playButtonEvent, levelSelectButtonEvent, optionsButtonEvent, exitButtonEvent;

    protected override void Awake()
    {
        base.Awake();

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

    void Start()
    {
        onSelectedSFX.Initialize();
    }

    protected override void Show()
    {
        // Enable the parent back panel
        menu.gameObject.SetActive(true);

        // Select the default button
        playButton.Select();

        // Enable the coop selector
        coopSelector.SetActive(true);
    }

    protected override void Hide()
    {
        // Disable the parent back panel
        menu.gameObject.SetActive(false);

        // Disable the coop selector
        coopSelector.SetActive(false);

        // Deselect
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
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

    #endregion
}