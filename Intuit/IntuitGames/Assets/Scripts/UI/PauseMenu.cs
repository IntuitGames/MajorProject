using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomExtensions;
using UnityEngine.EventSystems;public class PauseMenu : BaseUI{
    [Header("Components")]
    public Image backgroundPanel;
    public Button resumeButton;
    public Button optionsButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button exitButton;

    [Header("Settings")]
    public SoundClip onSelectedSFX = new SoundClip();

	private bool enableSound;

    void Start()
    {
        onSelectedSFX.Initialize();
    }

    protected override void Show()
    {
        // Enable the parent back panel
        backgroundPanel.gameObject.SetActive(true);

        // Select the default button
        StartCoroutine(Unity.NextFrame(resumeButton.Select));

        // After auto-selecting the first option re-enable sound
		StartCoroutine(Unity.NextFrame(() => enableSound = true));
    }

    protected override void Hide()
    {
        // Disable the parent back panel
        backgroundPanel.gameObject.SetActive(false);

        // Deselect
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    public override void OnSelect(BaseEventData eventData)
    {
		if (enableSound)
        	onSelectedSFX.PlayAttached(GetComponent<AudioSource>(), AudioManager.GetFMODAttribute(eventData.selectedObject.transform, Vector3.zero), 1);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        onSelectedSFX.Dispose();
    }

    #region BUTTON ACTIONS

    public void Resume()
    {
        GameManager.ModeManager.RequestGameModeChange(ModeManager.GameMode.InGame, false, 0.1f);
    }

    #endregion
}