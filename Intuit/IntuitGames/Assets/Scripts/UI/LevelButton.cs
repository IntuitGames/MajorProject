using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using CustomExtensions;public class LevelButton : MonoBehaviour{    public int levelIndex = 0;
    public string levelDisplayName;
    public bool developmentOnly = false;

    [Header("Components")]
    public Button button;
    public Image thumbnail;
    public Text numLabel;
    public Text titleLabel;
    public Text descriptionLabel;
    public Text timeLabel;
    public Text scoreLabel;    void Awake()
    {
        if (developmentOnly && Debug.isDebugBuild)
            gameObject.SetActive(false);
    }    void OnEnable()
    {
        numLabel.text = "Lvl. " + levelIndex;
        titleLabel.text = levelDisplayName;

        if (PlayerPrefs.HasKey("Level" + levelIndex + "Time"))
            timeLabel.text = string.Format("Time: {0}", PlayerPrefs.GetFloat("Level" + levelIndex + "Time"));
        else
            timeLabel.text = "N/A";

        if (PlayerPrefs.HasKey("Level" + levelIndex + "Score"))
            timeLabel.text = string.Format("Score: {0}", PlayerPrefs.GetFloat("Level" + levelIndex + "Score"));
        else
            timeLabel.text = "N/A";
    }    public void LoadLevel()
    {
        GameManager.LoadLevel(levelIndex);
    }}