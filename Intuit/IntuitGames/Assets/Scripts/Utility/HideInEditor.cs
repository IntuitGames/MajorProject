﻿using UnityEngine;
    public bool show = false;
    public GameObject[] gameObjects;
    private bool isShown;
    {
        if (!Application.isPlaying)
        {
            if (isShown != show)
            {
                foreach (var GO in gameObjects)
                {
                    GO.SetActive(show);
                }
                isShown = show;
            }
        }
        else
        {
            if (!isShown)
            {
                foreach(var GO in gameObjects)
                {
                    GO.SetActive(true);
                }
                isShown = true;
            }
        }
    }