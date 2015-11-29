﻿using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;

/// <summary>
/// Stores all character related audio related.
/// </summary>
[RequireComponent(typeof(Character), typeof(AudioSource))]
public class CharacterAudio : MonoBehaviour, System.IDisposable
{
    #region VARIABLES

    [Range(0, 1)]
    public float volume = 1;

    // COMPONENTS
    [HideInInspector]
    public Character character;
    [HideInInspector]
    public AudioSource audioSource;
    [HideInInspector]
    public Rigidbody rigidbodyComp;
    public Transform feetTransform;

    // SOUND EFFECTS
    [Header("Sound Effects")]
    public SoundClip footstep = new SoundClip();
    public SoundClip land = new SoundClip();
    public SoundClip jump = new SoundClip();
    public SoundClip dash = new SoundClip();
    public SoundClip collide = new SoundClip();

    // FMOD PARAMS
    private float playerMoveSpeed
    {
        get { return Mathf.Lerp(0, 1, character.currentMoveSpeed / character.sprintMoveSpeed); }
    }
    private float weakenedStateParam = 0;
    private bool increaseWeakenedValue = false;

    private float[] parameters;

    #endregion

    #region METHODS

    // Initializes components and sound clips
    void Awake()
    {
        if (!character)
            character = GetComponent<Character>();
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
        if (!rigidbodyComp)
            rigidbodyComp = GetComponent<Rigidbody>();

        footstep.Initialize();
        land.Initialize();
        jump.Initialize();
        dash.Initialize();
        collide.Initialize();

        GameManager.TetherManager.OnDisconnected += StartWeakened;
        GameManager.TetherManager.OnReconnected += StopWeakened;
    }

    void Update()
    {
        weakenedStateParam = Mathf.Clamp(weakenedStateParam + (increaseWeakenedValue ? Time.deltaTime : -Time.deltaTime), 0, 1);

        // Update weakened params
        footstep.UpdateParameter(2, weakenedStateParam);
        land.UpdateParameter(2, weakenedStateParam);
        jump.UpdateParameter(1, weakenedStateParam);
        dash.UpdateParameter(1, weakenedStateParam);
        collide.UpdateParameter(1, weakenedStateParam);
    }

    public bool ConditionalAudio(System.Action method, bool condition)
    {
        if (!condition || !enabled) return false;
        method();
        return true;
    }

    public void PlayWalkAudio(Surface.SurfaceType surfaceType)
    {
        parameters = new float[] { 0, (int)surfaceType, weakenedStateParam };
        footstep.PlayDetached(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, null, parameters);
    }

    public void PlayLandAudio(float downwardVelocity)
    {
        parameters = new float[] { 0, downwardVelocity.Normalize(2, 30, 0, 1), weakenedStateParam };
        land.PlayDetached(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, null, parameters);
    }

    public void PlayJumpAudio()
    {
        parameters = new float[] { 0, weakenedStateParam };
        jump.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
    }

    public void PlayDashAudio()
    {
        parameters = new float[] { 0, weakenedStateParam };
        dash.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
    }

    public void PlayCollideAudio()
    {
        parameters = new float[] { 0, weakenedStateParam };
        collide.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
    }

    public void Dispose()
    {
        // Dispose FMOD instances
        footstep.Dispose();
        land.Dispose();
        jump.Dispose();
        dash.Dispose();
        collide.Dispose();

        // Unsubscribe
        GameManager.TetherManager.OnDisconnected -= StartWeakened;
        GameManager.TetherManager.OnReconnected -= StopWeakened;
    }

    private void StartWeakened(TetherJoint joint)
    {
        increaseWeakenedValue = true;
    }

    private void StopWeakened(TetherJoint joint)
    {
        increaseWeakenedValue = false;
    }

    #endregion
}