using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CustomExtensions;public class OptionsMenu : BaseUI{
    [Header("Components")]
    public RectTransform backPanel;
    public Slider masterVolumeSlider;
    public Slider BGMVolumeSlider;
    public Slider SFXVolumeSlider;
    public Toggle fullscreenToggle;
    public CycleSelector resolutionPicker;
    public CycleSelector qualityPicker;
    public Button applyButton;
    public Button cancelButton;

    [Header("Settings")]
    public float selectScale = 1.2f;
    public SoundClip onSelectedSFX = new SoundClip();

    private bool enableSound;

    protected override void Awake()
    {
        base.Awake();

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
        // Set volume slider values
        masterVolumeSlider.value = GameManager.AudioManager.masterVolume;
        BGMVolumeSlider.value = GameManager.AudioManager.backgroundMusicVolume;
        SFXVolumeSlider.value = GameManager.AudioManager.soundEffectVolume;

        // Show parent panel
        backPanel.gameObject.SetActive(true);

        // Select the default button
        StartCoroutine(Unity.NextFrame(masterVolumeSlider.Select));

        // After auto-selecting the first option re-enable sound
        StartCoroutine(Unity.NextFrame(() => enableSound = true));
    }

    protected override void Hide()
    {
        // Hide parent panel
        backPanel.gameObject.SetActive(false);

        // Deselect
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        eventData.selectedObject.transform.localScale = new Vector3(selectScale, selectScale, selectScale);
        if (enableSound)
            onSelectedSFX.PlayAttached(GetComponent<AudioSource>(), AudioManager.GetFMODAttribute(eventData.selectedObject.transform, Vector3.zero), 1);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        eventData.selectedObject.transform.localScale = Vector3.one;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        onSelectedSFX.Dispose();
    }

    #region BUTTON BEHAVIOURS

    // 0 = Master, 1 = BGM, 2 = SFX
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

        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("BGMVolume", BGMVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", SFXVolumeSlider.value);
    }

    #endregion
}