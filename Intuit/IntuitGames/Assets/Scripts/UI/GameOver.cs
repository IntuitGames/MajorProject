using UnityEngine;using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.EventSystems;
using CustomExtensions;public class GameOver : BaseUI{
    [Header("Components")]
    public Image backgroundPanel;
    public Button restartButton;
    public Button exitButton;
    public StretchButton restartStrechButton;
    public StretchButton exitStrechButton;

    [Header("Settings")]
    [ReadOnly(EditableInEditor = true)]
    public bool remainClickable = false;
    public SoundClip onSelectedSFX = new SoundClip();
    public SoundClip onShowSFX = new SoundClip();
    public float fadeSpeed = 1;
    [ReadOnly]
    public float currentAlpha;

    private Button.ButtonClickedEvent restartButtonEvent, exitButtonEvent;

    private bool enableSound;

    protected override void Awake()
    {
        base.Awake();

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

    void Start()
    {
        onSelectedSFX.Initialize();
        onShowSFX.Initialize();
    }

    void Update()
    {
        if (isShown && currentAlpha < 1)
            SetUIAlpha(currentAlpha + (Time.unscaledDeltaTime * fadeSpeed));
        else if (!isShown && currentAlpha > 0)
            SetUIAlpha(currentAlpha - (Time.unscaledDeltaTime * fadeSpeed));
    }

    protected override void Show()
    {
        // Play show sound
        onShowSFX.PlayAttached(GetComponent<AudioSource>(), AudioManager.GetFMODAttribute(GameManager.CameraManager.mainCamera.transform, Vector3.zero), 1);

        // Zero alpha
        SetUIAlpha(0);

        // Enable the parent back panel
        backgroundPanel.gameObject.SetActive(true);

        // Select the default button
        StartCoroutine(Unity.NextFrame(restartButton.Select));

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

        enableSound = false;
    }

    private void SetUIAlpha(float alpha)
    {
        currentAlpha = Mathf.Clamp(alpha, 0, 1);
        backgroundPanel.color = backgroundPanel.color.SetAlpha(currentAlpha);
        restartStrechButton.imageCompP1.color = restartStrechButton.imageCompP1.color.SetAlpha(currentAlpha);
        restartStrechButton.imageCompP2.color = restartStrechButton.imageCompP2.color.SetAlpha(currentAlpha);
        exitStrechButton.imageCompP1.color = exitStrechButton.imageCompP1.color.SetAlpha(currentAlpha);
        exitStrechButton.imageCompP2.color = exitStrechButton.imageCompP2.color.SetAlpha(currentAlpha);
    }

    // Performs the onClick action for the selected UI
    public void SelectedButtonBehaviour()
    {
        Debug.LogWarning("Coop-Selector not in use!");

        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (selectedObject && selectedObject == restartButton.gameObject)
            restartButtonEvent.Invoke();
        else if (selectedObject && selectedObject == exitButton.gameObject)
            exitButtonEvent.Invoke();
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
        onShowSFX.Dispose();
    }
}