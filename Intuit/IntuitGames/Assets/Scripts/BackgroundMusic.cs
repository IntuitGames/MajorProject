using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public SoundClip music = new SoundClip(AudioManager.Player.FMOD, AudioManager.Type.BackgroundMusic, AudioManager.Group.Game);

    void Start()
    {
        music.Initialize();
        music.Play(GetComponent<AudioSource>(), AudioManager.GetFMODAttribute(transform, Vector3.zero), detach: false);
    }

    void OnDestroy()
    {
        music.Dispose();
    }
}