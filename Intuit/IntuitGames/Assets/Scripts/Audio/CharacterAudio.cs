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
    public SoundClip startHeavy = new SoundClip();
    public SoundClip endHeavy = new SoundClip();

    // FMOD PARAMS
    private float playerMoveSpeed
    {
        get { return Mathf.Lerp(0, 1, character.currentMoveSpeed / character.sprintMoveSpeed); }
    }
    private float weakenedState = 0;

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
        startHeavy.Initialize();
        endHeavy.Initialize();

        GameManager.TetherManager.OnDisconnected += StartWeakened;
        GameManager.TetherManager.OnReconnected += StopWeakened;
    }

    void Update()
    {
        weakenedState = Mathf.Clamp(weakenedState + (increaseWeakenedValue ? Time.deltaTime : -Time.deltaTime), 0, 1);
    }

    public void PlayWalkAudio(Surface.SurfaceTypes surfaceType, bool condition = true)
    {
        if (!condition || !enabled || (int)surfaceType < 0) return;

        parameters = new float[] { 0, (int)surfaceType, weakenedState };
        footstep.PlayDetached(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, null, parameters);
    }

    public void PlayLandAudio(float downwardVelocity, bool condition = true)
    {
        if (!condition || !enabled) return;

        parameters = new float[] { 0, downwardVelocity.Normalize(2, 30, 0, 1), weakenedState };
        land.PlayDetached(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, null, parameters);
    }

    public void PlayJumpAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        parameters = new float[] { 0, weakenedState };
        jump.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
    }

    public void PlayDashAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        parameters = new float[] { 0, weakenedState };
        dash.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume, parameters);
    }

    public void PlayStartHeavyAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        startHeavy.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume);
    }

    public void PlayEndHeavyAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        endHeavy.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume);
    }

    public void Dispose()
    {
        // Dispose FMOD instances
        footstep.Dispose();
        land.Dispose();
        jump.Dispose();
        dash.Dispose();
        startHeavy.Dispose();
        endHeavy.Dispose();

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