﻿using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;using FMOD.Studio;
using CustomExtensions;
using System;/// <summary>
/// Master control for all audio outputs.
/// </summary>public class AudioManager : Manager{
    // NESTED TYPES
    public enum Player { None, Unity, FMOD };
    public enum Type { SoundEffect, BackgroundMusic };
    public enum Group { Game, UI, Player, Enemy };

    // INSPECTOR
    public bool masterEnabled = true;
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float unityVolume = 1;
    [Range(0, 1)]
    public float FMODVolume = 1;
    [Range(0, 1)]
    public float soundEffectVolume = 1;
    [Range(0, 1)]
    public float backgroundMusicVolume = 1;
    [Range(0, 1)]
    public float gameGroupVolume = 1;
    [Range(0, 1)]
    public float UIGroupVolume = 1;
    [Range(0, 1)]
    public float playerGroupVolume = 1;
    [Range(0, 1)]
    public float enemyGroupVolume = 1;    // Returns the final play volume of a sound clip    public float GetFinalVolume(SoundClip soundClip, float additionalMulti = 1)
    {
        if(!masterEnabled) return 0;

        float value = masterVolume;
        value *= soundClip.volume;
        value *= additionalMulti;

        if (soundClip.player == Player.None)
            return 0;
        else if (soundClip.player == Player.Unity)
            value *= unityVolume;
        else if (soundClip.player == Player.FMOD)
            value *= FMODVolume;

        if (soundClip.type == Type.SoundEffect)
            value *= soundEffectVolume;
        else if (soundClip.type == Type.BackgroundMusic)
            value *= backgroundMusicVolume;

        if (soundClip.group == Group.Game)
            value *= gameGroupVolume;
        else if (soundClip.group == Group.UI)
            value *= UIGroupVolume;
        else if (soundClip.group == Group.Player)
            value *= playerGroupVolume;
        else if (soundClip.group == Group.Enemy)
            value *= enemyGroupVolume;

        return value;
    }

    // Returns the 3D attributes of given transform and rigid body
    public static ATTRIBUTES_3D GetFMODAttribute(Transform transform, Vector3 velocity)
    {
        var FM_attribute = new ATTRIBUTES_3D();
        FM_attribute.position = transform.position;
        FM_attribute.velocity = velocity;
        FM_attribute.forward = transform.forward;
        FM_attribute.up = transform.up;
        return FM_attribute;
    }}