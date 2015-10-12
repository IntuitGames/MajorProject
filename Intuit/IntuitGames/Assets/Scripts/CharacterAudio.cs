using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;

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
    private AudioSource audioSource;
    private Rigidbody rigidbodyComp;
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

    #endregion

    #region METHODS

    // Initializes components and sound clips
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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

    public void PlayWalkAudio(float moveSpeed, bool condition = true)
    {
        if (!condition || !enabled) return;

        walk.Play(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, true, moveSpeed, 0);
    }

    public void PlayLandAudio(float fallSpeed, bool condition = true)
    {
        if (!condition || !enabled) return;

        land.Play(audioSource, AudioManager.GetFMODAttribute(feetTransform, rigidbodyComp.velocity), volume, true, 0, fallSpeed);
    }

    public void PlayJumpAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        jump.Play(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume);
    }

    public void PlayDashAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        dash.Play(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume);
    }

    public void PlayStartHeavyAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        startHeavy.Play(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume);
    }

    public void PlayEndHeavyAudio(bool condition = true)
    {
        if (!condition || !enabled) return;

        endHeavy.Play(audioSource, AudioManager.GetFMODAttribute(transform, rigidbodyComp.velocity), volume);
    }

    public void PlayTetherDisconnectAudio(Transform jointTransform, bool condition = true)
    {
        if (!condition || !enabled || !jointTransform) return;

        tetherDisconnect.Play(audioSource, AudioManager.GetFMODAttribute(jointTransform, rigidbodyComp.velocity), volume);
    }

    public void PlayTetherConnectAudio(Transform jointTransform, bool condition = true)
    {
        if (!condition || !enabled || !jointTransform) return;

        tetherConnect.Play(audioSource, AudioManager.GetFMODAttribute(jointTransform, rigidbodyComp.velocity), volume);
    }

    // Dispose FMOD instances
    public void Dispose()
    {
        walk.Dispose();
        land.Dispose();
        jump.Dispose();
    }

    #endregion
}