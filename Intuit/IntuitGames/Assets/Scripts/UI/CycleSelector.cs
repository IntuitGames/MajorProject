using UnityEngine;
using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;
using CustomExtensions;public class CycleSelector : MonoBehaviour{
    [Header("Components")]
    public Text text;
    public Button nextButton;
    public Button previousButton;
    public Image nextImage;
    public Image previousImage;

    [Header("Settings")]
    public string[] options;
    public bool wrap = false;

    [ReadOnly]
    public int index;
    [ReadOnly]
    public bool nextActive;
    [ReadOnly]
    public bool previousActive;    void OnEnable()
    {
        //index = options.Contains(text.text) ? System.Array.IndexOf<string>(options, text.text) : 0;

        UpdateActivity();
    }    public void Next()
    {
        if (!nextActive) return;

        if (!wrap || index != options.Length - 1)
            index = Mathf.Min(index + 1, options.Length - 1);
        else
            index = 0;

        UpdateActivity();
    }    public void Previous()
    {
        if (!previousActive) return;

        if (!wrap || index != 0)
            index = Mathf.Max(index - 1, 0);
        else
            index = options.Length - 1;

        UpdateActivity();    }    // Selects other button is this button is inactive    public void OnNextSelect()
    {
        if (!nextActive && previousActive)
            StartCoroutine(Unity.NextFrame(() => previousButton.Select()));
    }

    // Selects other button is this button is inactive    public void OnPreviousSelect()
    {
        if (!previousActive && nextActive)
            StartCoroutine(Unity.NextFrame(() => nextButton.Select()));
    }    private void UpdateActivity()
    {
        text.text = options.SafeGet(index, true);

        // Determine button activity
        if (!wrap)
        {
            nextActive = index < options.Length - 1;
            previousActive = index > 0;
        }
        else
        {
            nextActive = options.Length > 1;
            previousActive = options.Length > 1;
        }

        // Set image color
        nextImage.color = nextActive ? Color.white : nextButton.colors.disabledColor;
        previousImage.color = previousActive ? Color.white : previousButton.colors.disabledColor;
    }}