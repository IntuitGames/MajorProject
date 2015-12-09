using UnityEngine;
using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;public class HUD : BaseUI{
    [Header("Components")]
    public Image jellyImage;
    public Text jellyTextValue;
    public Text collectibleScoreLabel;
    public Text collectibleScoreTextValue;

    [Header("Settings")]
    public float collectibleSmoothTime = 0.2f;
    public bool showCollectibleScore = false;

    private float collectibleScoreActual;
    private float collectibleScoreSpeed;
    protected override void Awake()
    {
        base.Awake();
    }    void Update()
    {
        jellyTextValue.text = GameManager.PlayerManager.currentJelly.ToString();

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
        jellyImage.gameObject.SetActive(true);
        collectibleScoreLabel.gameObject.SetActive(showCollectibleScore);
        collectibleScoreTextValue.gameObject.SetActive(showCollectibleScore);
    }

    protected override void Hide()
    {
        jellyImage.gameObject.SetActive(false);
        collectibleScoreLabel.gameObject.SetActive(false);
        collectibleScoreTextValue.gameObject.SetActive(false);
    }
}