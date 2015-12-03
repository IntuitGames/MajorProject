using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
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
    public SoundClip heavyOn = new SoundClip();
    public SoundClip heavyOff = new SoundClip();

    // FMOD PARAMS
    private float playerMoveSpeed
    {
        get { return Mathf.Lerp(0, 1, character.currentMoveSpeed / character.sprintMoveSpeed); }
    }

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
        heavyOn.Initialize();
        heavyOff.Initialize();
    }

    void Update()
    {
        // Update weakened params
        footstep.UpdateParameter(3, GameManager.TetherManager.weakenedParam);
        land.UpdateParameter(2, GameManager.TetherManager.weakenedParam);
        jump.UpdateParameter(1, GameManager.TetherManager.weakenedParam);
        dash.UpdateParameter(1, GameManager.TetherManager.weakenedParam);
        collide.UpdateParameter(1, GameManager.TetherManager.weakenedParam);
        heavyOn.UpdateParameter(0, GameManager.TetherManager.weakenedParam);
        heavyOff.UpdateParameter(1, GameManager.TetherManager.weakenedParam);
    }

    public bool ConditionalAudio(System.Action method, bool condition)
    {
        if (!condition || !enabled) return false;
        method();
        return true;
    }

    public void PlayWalkAudio(Surface.SurfaceType surfaceType, bool isWet)
    {
        float[] parameters = { 0, isWet ? 0.7f : 0, (int)surfaceType, GameManager.TetherManager.weakenedParam };
        footstep.PlayDetached(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, null, parameters);
    }

    public void PlayLandAudio(float downwardVelocity)
    {
        float[] parameters = { 0, downwardVelocity.Normalize(2, 30, 0, 1), GameManager.TetherManager.weakenedParam };
        land.PlayDetached(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, null, parameters);
    }

    public void PlayJumpAudio()
    {
        float[] parameters = { 0, GameManager.TetherManager.weakenedParam };
        jump.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
    }

    public void PlayDashAudio()
    {
        float[] parameters = { 0, GameManager.TetherManager.weakenedParam };
        dash.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
    }

    public void PlayCollideAudio()
    {
        float[] parameters = { 0, GameManager.TetherManager.weakenedParam };
        collide.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
    }

    public void PlayHeavyAudio(bool isOn)
    {
        float[] parameters = { isOn ? GameManager.TetherManager.weakenedParam : 0, isOn ? 0 : GameManager.TetherManager.weakenedParam };
        if (isOn)
            heavyOn.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
        else
            heavyOff.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
    }

    public void Dispose()
    {
        // Dispose FMOD instances
        footstep.Dispose();
        land.Dispose();
        jump.Dispose();
        dash.Dispose();
        collide.Dispose();
    }

    #endregion
}