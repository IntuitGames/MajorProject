using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;
using CustomExtensions;
using FMOD.Studio;

/// <summary>
/// Basic sound data.
/// </summary>
[System.Serializable]
public class SoundClip : IDisposable
{
    // INSPECTOR
    public AudioManager.Player player = AudioManager.Player.FMOD;
    public AudioManager.Type type = AudioManager.Type.SoundEffect;
    public AudioManager.Group group = AudioManager.Group.Game;
    [Range(0, 1)]
    public float volume = 1;
    [Range(-3, 3)]
    public float pitch = 1;
    public AudioClip unityClip;
    public FMODAsset FMODAsset;

    // PRIVATES
    private bool isInitialized;
    private EventInstance FMODEvent;
    private List<ParameterInstance> FMODParameters = new List<ParameterInstance>();

    // CONSTRUCTORS
    public SoundClip() { }

    public SoundClip(AudioManager.Player Player, AudioManager.Type Type, AudioManager.Group Group)
    { player = Player; type = Type; group = Group; }

    public void Initialize()
    {
        // Sets up the FMOD event
        if (FMODAsset)
        {
            FMODEvent = FMOD_StudioSystem.instance.GetEvent(FMODAsset);

            // Find all its parameters
            int paramCount;
            FMODEvent.getParameterCount(out paramCount);

            for (int i = 0; i < paramCount; i++)
            {
                ParameterInstance newParamter;
                if (FMODEvent.getParameterByIndex(i, out newParamter) == FMOD.RESULT.OK)
                    FMODParameters.Add(newParamter);
            }
        }

        isInitialized = true;
    }

    // Plays the sound clip (Pitch does nothing atm)
    public void Play(AudioSource audioSource, ATTRIBUTES_3D attributes, float volumeMulti = 1, bool detach = true, params float[] parameters)
    {
        float finalVolume = GameManager.AudioManager.GetFinalVolume(this, volumeMulti);
        if (finalVolume <= 0 || !isInitialized) return;

        if (player == AudioManager.Player.Unity && audioSource)
        {
            audioSource.PlayClipSource(unityClip, detach, finalVolume).pitch = pitch;
        }
        else if (player == AudioManager.Player.FMOD && FMODEvent != null)
        {
            // Set the FMOD parameters
            for (int i = 0; i < parameters.Length && i < FMODParameters.Count; i++)
            {
                FMODParameters[i].setValue(parameters[i]);
            }
            FMODEvent.setPitch(pitch);
            FMODEvent.set3DAttributes(attributes);
            FMODEvent.setVolume(finalVolume);
            FMODEvent.start();
        }
    }

    // Updates the volume value where necessary
    public void UpdateSettings(AudioSource audioSource)
    {
        float finalVolume = GameManager.AudioManager.GetFinalVolume(this);

        if (player == AudioManager.Player.Unity && audioSource)
        {
            audioSource.volume = finalVolume;
        }
        else if (player == AudioManager.Player.FMOD && FMODEvent != null)
        {
            FMODEvent.setVolume(finalVolume);
        }
    }

    // Dispose FMOD instances
    public void Dispose()
    {
        if (FMODEvent != null)
        {
            FMODEvent.stop(STOP_MODE.IMMEDIATE);
            FMODEvent.release();
        }
    }
}