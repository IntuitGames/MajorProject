using UnityEngine;
using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;public class HUD : MonoBehaviour{
    public Slider weakenedSlider;
    public Text weakenedTextValue;
    public Text collectibleScoreLabel;
    public Text collectibleScoreTextValue;

    void Awake()
    {
        if (GameManager.ModeManager.currentGameMode.EqualToAny(ModeManager.GameMode.InGame, ModeManager.GameMode.PauseMenu))
            ShowHUD();
        else
            HideHUD();

        GameManager.ModeManager.OnGameModeChanged += CheckForGameModeChange;
    }    void Start()
    {
        weakenedSlider.maxValue = GameManager.PlayerManager.maxJelly;
    }    void Update()
    {
        weakenedSlider.value = GameManager.PlayerManager.jellyPercentage * weakenedSlider.maxValue;

        if (GameManager.PlayerManager.currentJelly < 0.5f && GameManager.PlayerManager.currentJelly > 0)
            weakenedTextValue.text = "1";
        else if (GameManager.PlayerManager.currentJelly < GameManager.PlayerManager.maxJelly && GameManager.PlayerManager.currentJelly > GameManager.PlayerManager.maxJelly - 0.5f)
            weakenedTextValue.text = (GameManager.PlayerManager.maxJelly - 1).ToString();
        else
            weakenedTextValue.text = Mathf.RoundToInt(GameManager.PlayerManager.currentJelly).ToString();

        collectibleScoreTextValue.text = GameManager.PlayerManager.collectibleScore.ToString();
    }

    void OnDestroy()
    {
        GameManager.ModeManager.OnGameModeChanged -= CheckForGameModeChange;
    }

    public void ShowHUD()
    {
        weakenedSlider.gameObject.SetActive(true);
        weakenedTextValue.gameObject.SetActive(true);
        collectibleScoreLabel.gameObject.SetActive(true);
        collectibleScoreTextValue.gameObject.SetActive(true);
    }

    public void HideHUD()
    {
        weakenedSlider.gameObject.SetActive(false);
        weakenedTextValue.gameObject.SetActive(false);
        collectibleScoreLabel.gameObject.SetActive(false);
        collectibleScoreTextValue.gameObject.SetActive(false);
    }

    private void CheckForGameModeChange(ModeManager.GameMode newMode, ModeManager.GameMode oldMode)
    {
        if (newMode == ModeManager.GameMode.InGame || newMode == ModeManager.GameMode.PauseMenu)
            ShowHUD();
        else if (oldMode == ModeManager.GameMode.InGame || oldMode == ModeManager.GameMode.PauseMenu &&
            newMode != ModeManager.GameMode.InGame || newMode != ModeManager.GameMode.PauseMenu)
            HideHUD();
    }}