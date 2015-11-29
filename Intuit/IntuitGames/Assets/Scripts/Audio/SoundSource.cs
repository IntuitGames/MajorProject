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

    public bool enableWeakenedParameter = false;
    public int weakenedParameterIndex = 0;
    private float weakenedStateParam;
    private bool increaseWeakenedValue = false;

    void Awake()
    {
        GameManager.TetherManager.OnDisconnected += StartWeakened;
        GameManager.TetherManager.OnReconnected += StopWeakened;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        music.Initialize();
        if (playOnStart) Play();
    }

    void Update()
    {
        // Update weakened state param
        weakenedStateParam = Mathf.Clamp(weakenedStateParam + (increaseWeakenedValue ? Time.deltaTime : -Time.deltaTime), 0, 1);

        // Update settings
        music.Update(audioSource, AudioManager.GetFMODAttribute(transform, Vector3.zero), 1);

        // Update weakened param settings
        if (enableWeakenedParameter)
            music.UpdateParameter(weakenedParameterIndex, weakenedStateParam);
    }

    void OnDestroy()
    {
        music.Dispose();

        // Unsubscribe
        GameManager.TetherManager.OnDisconnected -= StartWeakened;
        GameManager.TetherManager.OnReconnected -= StopWeakened;
    }

    public void Play()
    {
        music.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, Vector3.zero), 1);
    }

    private void StartWeakened(TetherJoint joint)
    {
        increaseWeakenedValue = true;
    }

    private void StopWeakened(TetherJoint joint)
    {
        increaseWeakenedValue = false;
    }
}