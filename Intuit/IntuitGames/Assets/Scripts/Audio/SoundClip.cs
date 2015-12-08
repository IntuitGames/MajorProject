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
    private List<CueInstance> FMODCues = new List<CueInstance>();

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
            try
            {
                FMODEvent.getParameterCount(out paramCount);
            }
            catch
            {
                paramCount = 0;
            }

            for (int i = 0; i < paramCount; i++)
            {
                ParameterInstance newParamter;
                if (FMODEvent.getParameterByIndex(i, out newParamter) == FMOD.RESULT.OK)
                    FMODParameters.Add(newParamter);
            }

            // Find all its cues (KEYOFFS)
            int cueCount;
            try
            {
                FMODEvent.getCueCount(out cueCount);
            }
            catch
            {
                cueCount = 0;
            }

            for (int i = 0; i < cueCount; i++)
            {
                CueInstance newCue;
                if (FMODEvent.getCueByIndex(i, out newCue) == FMOD.RESULT.OK)
                    FMODCues.Add(newCue);
            }
        }

        isInitialized = true;
    }

    // Plays the sound clip on specified audio source
    // Returns if it was successful in playing the sound
    public bool PlayAttached(AudioSource audioSource, ATTRIBUTES_3D attributes, float volumeMulti, float[] parameters = null)
    {
        if (!isInitialized) return false;

        // Initialization and volume checking
        float finalVolume = GameManager.AudioManager.GetFinalVolume(this, volumeMulti);

        // Play Unity Sound
        if (player == AudioManager.Player.Unity && audioSource)
        {
            audioSource.PlayClipAttached(unityClip);
            audioSource.volume = finalVolume;
            audioSource.pitch = pitch;
        }
        else if (player == AudioManager.Player.FMOD && FMODEvent != null) // Play FMOD Sound
        {
            // Set the FMOD parameters
            if (parameters != null)
                for (int i = 0; i < parameters.Length && i < FMODParameters.Count; i++)
                {
                    FMODParameters[i].setValue(parameters[i]);
                }
            FMODEvent.setPitch(pitch);
            FMODEvent.set3DAttributes(attributes);
            FMODEvent.setVolume(finalVolume);
            FMODEvent.start();
        }
        return true;
    }

    // Plays the sound clip on a detached clone audio source
    // Returns the new audio source if applicable
    public AudioSource PlayDetached(AudioSource audioSource, ATTRIBUTES_3D attributes, float volumeMulti, Transform target, float[] parameters = null)
    {
        if (!isInitialized) return null;

        // Initialization and volume checking
        float finalVolume = GameManager.AudioManager.GetFinalVolume(this, volumeMulti);

        if (player == AudioManager.Player.Unity && audioSource) // Play Unity Sound
        {
            AudioSource newSource = audioSource.PlayClipDetached(unityClip, target);
            newSource.volume = finalVolume;
            newSource.pitch = pitch;
            return newSource;
        }
        else if (player == AudioManager.Player.FMOD && FMODEvent != null)
        {
            // Set the FMOD parameters
            if (parameters != null)
                for (int i = 0; i < parameters.Length && i < FMODParameters.Count; i++)
                {
                    FMODParameters[i].setValue(parameters[i]);
                }
            FMODEvent.setPitch(pitch);
            FMODEvent.set3DAttributes(attributes);
            FMODEvent.setVolume(finalVolume);
            FMODEvent.start();
        }
        return null;
    }

    // Applies sound clip values to the audio source if able
    // Returns if it was successful in updating settings
    public bool Update(AudioSource audioSource, ATTRIBUTES_3D attributes, float volumeMulti, float[] parameters = null)
    {
        float finalVolume = GameManager.AudioManager.GetFinalVolume(this, volumeMulti);

        if (player == AudioManager.Player.Unity && audioSource && audioSource.isPlaying)
        {
            audioSource.volume = finalVolume;
            audioSource.pitch = pitch;
            return true;
        }
        else if (player == AudioManager.Player.FMOD && FMODEvent != null)
        {
            PLAYBACK_STATE playState;
            FMODEvent.getPlaybackState(out playState);
            if (playState == PLAYBACK_STATE.STOPPED) return false;

            if (parameters != null)
                for (int i = 0; i < parameters.Length && i < FMODParameters.Count; i++)
                {
                    FMODParameters[i].setValue(parameters[i]);
                }
            FMODEvent.set3DAttributes(attributes);
            FMODEvent.setVolume(finalVolume);
            FMODEvent.setPitch(pitch);
            return true;
        }

        return false;
    }

    // Updates a single FMOD param
    public void UpdateParameter(int paramIndex, float paramValue)
    {
        if (FMODParameters.SafeGet(paramIndex, true).isValid())
            FMODParameters.SafeGet(paramIndex, true).setValue(paramValue);
    }

    public void TriggerCue(int cueIndex)
    {
        if (FMODCues.SafeGet(cueIndex) != null)
            FMODCues.SafeGet(cueIndex).trigger();
    }

    public void Stop(STOP_MODE stopMode)
    {
        FMODEvent.stop(stopMode);
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