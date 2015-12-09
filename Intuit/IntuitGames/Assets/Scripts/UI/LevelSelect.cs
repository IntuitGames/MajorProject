using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CustomExtensions;public class LevelSelect : BaseUI{
    [Header("Components")]
    public RectTransform backPanel;
    public LevelButton levelOneButton;
    public LevelButton levelTwoButton;
    public LevelButton testLevelButton;
    public Button returnButton;

    protected override void Show()
    {
        // Enable the parent back panel
        backPanel.gameObject.SetActive(true);

        // Select the default button
        StartCoroutine(Unity.NextFrame(returnButton.Select));
    }

    protected override void Hide()
    {
        // Disable the parent back panel
        backPanel.gameObject.SetActive(false);

        // Deselect
        if (UnityEngine.EventSystems.EventSystem.current)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }
}