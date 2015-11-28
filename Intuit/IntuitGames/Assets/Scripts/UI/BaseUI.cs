using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;public abstract class BaseUI : MonoBehaviour{
    [SerializeField, ReadOnly]
    private bool isShown = false;
    [EnumFlags]
    public ModeManager.GameMode shownModes;

    void Start()
    {
        if (GameManager.ModeManager.currentGameMode.HasFlags(shownModes))
        {
            Show();
            isShown = true;
        }
        else
        {
            Hide();
            isShown = false;
        }

        GameManager.ModeManager.OnGameModeChanged += this.OnGameModeChanged;
    }

    void OnDestroy()
    {
        GameManager.ModeManager.OnGameModeChanged -= this.OnGameModeChanged;
    }

    private void OnGameModeChanged(ModeManager.GameMode newMode, ModeManager.GameMode oldMode)
    {
        if (newMode.HasFlags(shownModes))
            ChangeVisibility(true);
        else
            ChangeVisibility(false);
    }

    public void ChangeVisibility(bool state)
    {
        if (isShown && !state)
        {
            Hide();
            isShown = false;
        }
        else if (!isShown && state)
        {
            Show();
            isShown = true;
        }
    }

    protected abstract void Show();

    protected abstract void Hide();}