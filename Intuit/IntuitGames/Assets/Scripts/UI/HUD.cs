using UnityEngine;
using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;public class HUD : BaseUI{
    [Header("Components")]
    public Slider weakenedSlider;
    public Text weakenedTextValue;
    public Text collectibleScoreLabel;
    public Text collectibleScoreTextValue;

    [Header("Settings")]
    public float collectibleSmoothTime = 0.2f;

    private float collectibleScoreActual;
    private float collectibleScoreSpeed;
    protected override void Awake()
    {
        base.Awake();

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

        // Smooth collectible score value
        collectibleScoreActual = Mathf.SmoothDamp(
            collectibleScoreActual,
            GameManager.PlayerManager.collectibleScore,
            ref collectibleScoreSpeed,
            collectibleSmoothTime);

        collectibleScoreTextValue.text = Mathf.Round(collectibleScoreActual).ToString();
    }

    protected override void Show()
    {
        weakenedSlider.gameObject.SetActive(true);
        weakenedTextValue.gameObject.SetActive(true);
        collectibleScoreLabel.gameObject.SetActive(true);
        collectibleScoreTextValue.gameObject.SetActive(true);
    }

    protected override void Hide()
    {
        weakenedSlider.gameObject.SetActive(false);
        weakenedTextValue.gameObject.SetActive(false);
        collectibleScoreLabel.gameObject.SetActive(false);
        collectibleScoreTextValue.gameObject.SetActive(false);
    }
}