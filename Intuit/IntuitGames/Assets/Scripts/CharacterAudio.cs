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
    public SoundClip walk = new SoundClip();
    public SoundClip land = new SoundClip();
    public SoundClip jump = new SoundClip();
    public SoundClip dash = new SoundClip();
    public SoundClip startHeavy = new SoundClip();
    public SoundClip endHeavy = new SoundClip();
    public SoundClip tetherDisconnect = new SoundClip();
    public SoundClip tetherConnect = new SoundClip();

    // FMOD PARAMS
    private float playerMoveSpeed
    {
        get { return Mathf.Lerp(0, 1, character.currentMoveSpeed / character.sprintMoveSpeed); }
    }

    #endregion

    #region METHODS

    // Initializes components and sound clips
    void Start()
    {
        if (!character)
            character = GetComponent<Character>();
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
        if (!rigidbodyComp)
            rigidbodyComp = GetComponent<Rigidbody>();

        walk.Initialize();
        land.Initialize();
        jump.Initialize();
        dash.Initialize();
        startHeavy.Initialize();
        endHeavy.Initialize();
        tetherDisconnect.Initialize();
        tetherConnect.Initialize();
    }

    public void PlayWalkAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        walk.PlayDetached(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, null, playerMoveSpeed, 0);
    }

    public void PlayLandAudio(float downwardVelocity, bool condition = true)
    {
        if (!condition || !enabled) return;

        land.PlayDetached(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, null, 0, downwardVelocity.Normalize(2, 30, 0, 1));
    }

    public void PlayJumpAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        jump.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume);
    }

    public void PlayDashAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        dash.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume);
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

    public void PlayTetherDisconnectAudio(Transform jointTransform, bool condition = true)
    {
        if (!condition || !enabled || !jointTransform) return;

        tetherDisconnect.PlayDetached(audioSource, AudioManager.GetFMODAttribute(jointTransform, rigidbodyComp.velocity), volume, jointTransform);
    }

    public void PlayTetherConnectAudio(Transform jointTransform, bool condition = true)
    {
        if (!condition || !enabled || !jointTransform) return;

        tetherConnect.PlayDetached(audioSource, AudioManager.GetFMODAttribute(jointTransform, rigidbodyComp.velocity), volume, jointTransform);
    }

    // Dispose FMOD instances
    public void Dispose()
    {
        walk.Dispose();
        land.Dispose();
        jump.Dispose();
        dash.Dispose();
        startHeavy.Dispose();
        endHeavy.Dispose();
        tetherDisconnect.Dispose();
        tetherConnect.Dispose();
    }

    #endregion
}