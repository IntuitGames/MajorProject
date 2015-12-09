using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;public class StretchButton : MonoBehaviour, ISelectHandler, IDeselectHandler{
    [Header("Player 1")]
    public Image imageCompP1;
    public Sprite[] spritesP1;
    public Sprite splitSpriteP1;
    public string inputP1 = "Submit_P1";

    [Header("Player 2")]
    public Image imageCompP2;
    public Sprite[] spritesP2;
    public Sprite splitSpriteP2;
    public string inputP2 = "Submit_P2";

    [Space(10)]
    public float selectedScale = 1.2f;
    public float holdDuration = 0.5f;
    public float duration;
    public float eventDelay = 0.25f;
    public UnityEvent OnStretchSelect = new UnityEvent();    private int indexP1;
    private int indexP2;
    private TimerPlus animTimer;
    private float singleDuration;
    private bool activated;
    private bool isSelected;
    private float holdTimer;

    private bool pullP1;
    private bool pullP2;    void Awake()
    {
        singleDuration = duration / (float)spritesP1.Length;
        imageCompP1.sprite = spritesP1[indexP1]; imageCompP2.sprite = spritesP2[indexP2];
        animTimer = TimerPlus.Create(singleDuration, TimerPlus.Presets.BackgroundRepeater, UpdateIndex);
    }    void OnEnable()
    {
        activated = false; holdTimer = 0;
        indexP1 = 0; indexP2 = 0;
        imageCompP1.sprite = spritesP1[indexP1]; imageCompP2.sprite = spritesP2[indexP2];
        animTimer.Start();
    }    void OnDisable()
    {
        animTimer.Stop();
    }    void Update()
    {
        if (activated) return;

        // Check inputs
        pullP1 = Input.GetButton(inputP1) && isSelected;
        pullP2 = Input.GetButton(inputP2) && isSelected;

        // Update sprites
        imageCompP1.sprite = spritesP1[indexP1];
        imageCompP2.sprite = spritesP2[indexP2];

        // Update scale
        if (isSelected)
        {
            imageCompP1.transform.localScale = new Vector3(selectedScale, selectedScale, selectedScale);
            imageCompP2.transform.localScale = new Vector3(selectedScale, selectedScale, selectedScale);
        }
        else
        {
            imageCompP1.transform.localScale = Vector3.one;
            imageCompP2.transform.localScale = Vector3.one;
        }

        // Check for hold activation
        if (indexP1 == spritesP1.Length - 1 && indexP2 == spritesP2.Length - 1)
            holdTimer += Time.unscaledDeltaTime;
        else
            holdTimer = 0;

        if (holdTimer > holdDuration)
        {
            activated = true;
            imageCompP1.sprite = splitSpriteP1;
            imageCompP2.sprite = splitSpriteP2;
            TimerPlus.Create(eventDelay, TimerPlus.Presets.BackgroundOneTimeUse, OnStretchSelect.Invoke);
        }
    }    private void UpdateIndex()
    {
        if (pullP1)
            indexP1 = Mathf.Min(indexP1 + 1, spritesP1.Length - 1);
        else
            indexP1 = Mathf.Max(indexP1 - 1, 0);

        if (pullP2)
            indexP2 = Mathf.Min(indexP2 + 1, spritesP2.Length - 1);
        else
            indexP2 = Mathf.Max(indexP2 - 1, 0);
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
    }
}