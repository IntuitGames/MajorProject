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

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        music.Initialize();
        if (playOnStart) Play();
    }

    void Update()
    {
        music.Update(audioSource, AudioManager.GetFMODAttribute(transform, Vector3.zero), 1);
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