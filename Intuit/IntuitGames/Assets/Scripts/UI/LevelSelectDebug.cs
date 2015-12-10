﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomExtensions;
using UnityEngine.EventSystems;
    public bool devBuildOnly = false;
    [EnumFlags]
    public ModeManager.GameMode accessModes;
    public KeyCode[] accessInputs;
    {
        get { return Debug.isDebugBuild || !devBuildOnly; }
    }
        {
            foreach (var key in accessInputs)
                if (Input.GetKeyDown(key))
                    GameManager.ModeManager.RequestGameModeChange(ModeManager.GameMode.LevelSelect, false, 0.25f);
        }