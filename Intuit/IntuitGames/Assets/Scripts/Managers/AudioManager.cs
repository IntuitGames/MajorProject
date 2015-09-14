using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;using FMOD.Studio;
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
    }}/// <summary>
/// Basic sound data.
/// </summary>[System.Serializable]public class SoundClip : IDisposable
{
    // INSPECTOR
    public AudioManager.Player player = AudioManager.Player.FMOD;
    public AudioManager.Type type = AudioManager.Type.SoundEffect;
    public AudioManager.Group group = AudioManager.Group.Game;
    [Range(0, 1)]
    public float volume = 1;
    [Range(-1, 1)]
    public float pitch = 0;
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
        if(FMODAsset)
        {
            FMODEvent = FMOD_StudioSystem.instance.GetEvent(FMODAsset);

            // Find all its parameters
            int paramCount;
            FMODEvent.getParameterCount(out paramCount);

            for(int i = 0; i < paramCount; i++)
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
            audioSource.PlayClip(unityClip, true, finalVolume);
        }
        else if (player == AudioManager.Player.FMOD && FMODEvent != null)
        {
            // Set the FMOD parameters
            for (int i = 0; i < parameters.Length && i < FMODParameters.Count; i++)
            {
                FMODParameters[i].setValue(parameters[i]);
            }

            FMODEvent.set3DAttributes(attributes);
            FMODEvent.setVolume(finalVolume);
            FMODEvent.start();
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