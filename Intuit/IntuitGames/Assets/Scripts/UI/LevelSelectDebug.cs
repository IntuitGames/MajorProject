using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomExtensions;
using UnityEngine.EventSystems;public class LevelSelectDebug : MonoBehaviour{
    public bool devBuildOnly = false;
    [EnumFlags]
    public ModeManager.GameMode accessModes;
    public KeyCode[] accessInputs;    private bool buildAccess
    {
        get { return Debug.isDebugBuild || !devBuildOnly; }
    }	private void Update()	{		if (buildAccess && accessModes.IsFlagSet<ModeManager.GameMode>(GameManager.ModeManager.currentGameMode))
        {
            foreach (var key in accessInputs)
                if (Input.GetKeyDown(key))
                    GameManager.ModeManager.RequestGameModeChange(ModeManager.GameMode.LevelSelect, false, 0.25f);
        }	}}