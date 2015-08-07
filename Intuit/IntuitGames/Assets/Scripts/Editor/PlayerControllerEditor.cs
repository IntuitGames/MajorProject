﻿using UnityEngine;
using UnityEditor;
    PlayerController Target;

    public enum PlayerNum { Player1, Player2 };
    public PlayerNum Player;

    public void OnEnable()
    {
        Target = (PlayerController)target;
    }

    public override void OnInspectorGUI()
    {
        if (!Target) return;

        Player = Target.isPlayerOne ? PlayerNum.Player1 : PlayerNum.Player2;

        Player = (PlayerNum)EditorGUILayout.EnumPopup("Player", Player);

        Target.isPlayerOne = Player == PlayerNum.Player1;

        base.OnInspectorGUI();
    }