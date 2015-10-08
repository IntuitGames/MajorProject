using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    private AudioSource audioSource;

    public SoundClip music = new SoundClip(AudioManager.Player.FMOD, AudioManager.Type.BackgroundMusic, AudioManager.Group.Game);

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        music.Initialize();
        music.Play(audioSource, AudioManager.GetFMODAttribute(transform, Vector3.zero), detach: false);
    }

#if UNITY_EDITOR
    void Update()
    {
        music.UpdateSettings(audioSource);
    }
#endif

    void OnDestroy()
    {
        music.Dispose();
    }
}