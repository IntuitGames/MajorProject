using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class SoundSource : MonoBehaviour
{
    private AudioSource audioSource;

    public bool playOnStart = true;
    public SoundClip music = new SoundClip(AudioManager.Player.FMOD, AudioManager.Type.BackgroundMusic, AudioManager.Group.Game);

    public bool useWeakenedParam = false;
    public int weakenedParamIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        music.Initialize();
        if (playOnStart) Play();
    }

    void Update()
    {
        // Update settings
        music.Update(audioSource, AudioManager.GetFMODAttribute(transform, Vector3.zero), 1);

        // Update weakened param settings
        if (useWeakenedParam)
            music.UpdateParameter(weakenedParamIndex, GameManager.TetherManager.weakenedParam);
    }

    void OnDestroy()
    {
        music.Dispose();
    }

    public void Play()
    {
        music.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, Vector3.zero), 1);
    }
}