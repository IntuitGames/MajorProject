using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CustomExtensions;public class MainMenu : BaseUI{
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

    [Space(10)]
    public LevelButton levelOneButton;
    public LevelButton levelTwoButton;
    public LevelButton testLevelButton;
    public Button levelSelectToMenuButton;

    [Space(10)]
    public Slider masterVolumeSlider;
    public Slider BGMVolumeSlider;
    public Slider SFXVolumeSlider;
    public Toggle fullscreenToggle;
    public CycleSelector resolutionPicker;
    public CycleSelector qualityPicker;
    public Button optionsToMenuButton;

    [Header("Settings")]
    // 0 = Main Menu, 1 = Level Select, 2 = Options
    [SerializeField, ReadOnly]
    private int _mainMenuIndex = 0;
    public int mainMenuIndex
    {
        get { return _mainMenuIndex; }
        set
        {
            if (value == _mainMenuIndex || value < 0 || value > 2) return;
            _mainMenuIndex = value;
            OnMainMenuIndexChanged();
        }
    }
    public float transitionDelay = 0.5f;
    [ReadOnly(EditableInEditor = true)]
    public bool remainClickable = false;
    public SoundClip onSelectedSFX = new SoundClip();

    private Button.ButtonClickedEvent playButtonEvent, levelSelectButtonEvent, optionsButtonEvent, exitButtonEvent;

    protected override void Awake()
    {
        base.Awake();

        mainMenuIndex = 0;

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

        // Retrieve resolutions
        resolutionPicker.options = GameManager.allResolutionsDisplay;
        resolutionPicker.text.text = GameManager.currentResolutionDisplay;
        resolutionPicker.index = GameManager.currentResolutionIndex;

        // Retrieve quality settings
        qualityPicker.options = GameManager.qualityLevels;
        qualityPicker.text.text = GameManager.currentQualityLevelDisplay;
        qualityPicker.index = GameManager.currentQualityIndex;

        // Retrieve full screen setting
        fullscreenToggle.isOn = Screen.fullScreen;
    }

    void Start()
    {
        onSelectedSFX.Initialize();
    }

    protected override void Show()
    {
        // Enable the parent back panel
        menu.gameObject.SetActive(true);
        mainMenuIndex = 0;

        // Set volume slider values
        masterVolumeSlider.value = GameManager.AudioManager.masterVolume;
        BGMVolumeSlider.value = GameManager.AudioManager.backgroundMusicVolume;
        SFXVolumeSlider.value = GameManager.AudioManager.soundEffectVolume;

        // Select the default button
        playButton.Select();

        // Enable the coop selector
        coopSelector.SetActive(true);
    }

    protected override void Hide()
    {
        // Disable the parent back panel
        menu.gameObject.SetActive(false);
        levelSelect.gameObject.SetActive(false);
        options.gameObject.SetActive(false);

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

    // 0 = main, 1 = Level Select, 2 = Options
    private void OnMainMenuIndexChanged()
    {
        if (mainMenuIndex == 0)
        {
            playButton.Select();
            coopSelector.SetActive(true);
            levelSelect.gameObject.SetActive(false);
            options.gameObject.SetActive(false);
            TimerPlus.Create(transitionDelay, () =>
                {
                    menu.gameObject.SetActive(true);
                    playButton.Select();
                });
        }
        else if (mainMenuIndex == 1)
        {
            coopSelector.SetActive(false);
            menu.gameObject.SetActive(false);
            options.gameObject.SetActive(false);
            TimerPlus.Create(transitionDelay, () =>
            {
                levelSelect.gameObject.SetActive(true);
                levelOneButton.button.Select();
            });
        }
        else if (mainMenuIndex == 2)
        {
            coopSelector.SetActive(false);
            menu.gameObject.SetActive(false);
            options.gameObject.SetActive(false);
            TimerPlus.Create(transitionDelay, () =>
            {
                options.gameObject.SetActive(true);
                masterVolumeSlider.Select();
            });
        }
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

    // 0 = main, 1 = Level Select, 2 = Options
    public void ChangeMenuIndex(int index)
    {
        mainMenuIndex = index;
    }

    // 0 = master, 1 = BGM, 2 = SFX
    public void OnVolumeSliderChange(int slider)
    {
        if (slider == 0)
            GameManager.AudioManager.masterVolume = masterVolumeSlider.value;
        else if (slider == 1)
            GameManager.AudioManager.backgroundMusicVolume = BGMVolumeSlider.value;
        else
            GameManager.AudioManager.soundEffectVolume = SFXVolumeSlider.value;
    }

    public void ApplySettings()
    {
        GameManager.SetResolution(resolutionPicker.index, fullscreenToggle.isOn);
        GameManager.SetQualityLevel(qualityPicker.index);
    }

    #endregion
}