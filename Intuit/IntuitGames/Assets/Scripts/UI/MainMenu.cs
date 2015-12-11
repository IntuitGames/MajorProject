using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CustomExtensions;public class MainMenu : BaseUI{
    [Header("Components")]
    public RectTransform backPanel;
    public Button playButton;
    public Button exitButton;

    [Header("Settings")]
    [ReadOnly(EditableInEditor = true)]
    public bool remainClickable = false;
    public SoundClip onSelectedSFX = new SoundClip();

    private Button.ButtonClickedEvent playButtonEvent, exitButtonEvent;	private bool enableSound;

    protected override void Awake()
    {
        base.Awake();

        // Cache event listeners
        playButtonEvent = playButton.onClick;
        exitButtonEvent = exitButton.onClick;

        // Remove event listeners if buttons can no longer be activated via clicking
        if (!remainClickable)
        {
            playButton.onClick = new Button.ButtonClickedEvent();
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
        backPanel.gameObject.SetActive(true);

        // Select the default button
        StartCoroutine(Unity.NextFrame(playButton.Select));		// After auto-selecting the first option re-anble sound		StartCoroutine(Unity.NextFrame(() => enableSound = true));
    }

    protected override void Hide()
    {
        // Disable the parent back panel
        backPanel.gameObject.SetActive(false);

        // Deselect
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    public override void OnSelect(BaseEventData eventData)
    {		if (enableSound)
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
        Debug.LogWarning("Coop-Selector not in use!");

        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (selectedObject && selectedObject == playButton.gameObject)
            playButtonEvent.Invoke();
        else if (selectedObject && selectedObject == exitButton.gameObject)
            exitButtonEvent.Invoke();
    }

    public void Play()
    {
        GameManager.LoadLevel(1);
    }

    #endregion
}