using UnityEngine;
using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;public class HUD : BaseUI{
    [Header("Components")]
    public Image jellyJarImage;
    public Image jellyTextImage;
    public Text collectibleScoreLabel;
    public Text collectibleScoreTextValue;
    public Animator jellyTextAnimatorComp;

    [Header("Settings")]
    public float collectibleSmoothTime = 0.2f;
    public bool showCollectibleScore = false;
    public Sprite[] jellyJarSprites;
    public Sprite[] jellyTextSprites;

    private float collectibleScoreActual;
    private float collectibleScoreSpeed;
    private int expectedJelly;
    protected override void Awake()
    {
        base.Awake();

        expectedJelly = GameManager.PlayerManager.currentJelly;
    }    void Update()
    {
        jellyJarImage.sprite = jellyJarSprites[GameManager.PlayerManager.currentJelly];
        jellyTextImage.sprite = jellyTextSprites[GameManager.PlayerManager.currentJelly];

        // Smooth collectible score value
        collectibleScoreActual = Mathf.SmoothDamp(
            collectibleScoreActual,
            GameManager.PlayerManager.collectibleScore,
            ref collectibleScoreSpeed,
            collectibleSmoothTime);

        collectibleScoreTextValue.text = Mathf.Round(collectibleScoreActual).ToString();

        // Play growth animation
        if (expectedJelly != GameManager.PlayerManager.currentJelly)
            jellyTextAnimatorComp.SetTrigger("Grow");

        expectedJelly = GameManager.PlayerManager.currentJelly;
    }

    protected override void Show()
    {
        jellyJarImage.gameObject.SetActive(true);
        jellyTextImage.gameObject.SetActive(true);
        collectibleScoreLabel.gameObject.SetActive(showCollectibleScore);
        collectibleScoreTextValue.gameObject.SetActive(showCollectibleScore);
    }

    protected override void Hide()
    {
        jellyJarImage.gameObject.SetActive(false);
        jellyTextImage.gameObject.SetActive(false);
        collectibleScoreLabel.gameObject.SetActive(false);
        collectibleScoreTextValue.gameObject.SetActive(false);
    }
}