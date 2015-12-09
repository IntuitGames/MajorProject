using UnityEngine;
using System.Collections;
using CustomExtensions;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour {
    #region VARIABLES

    [Range(0, 1)]
    public float volume = 1;

    // COMPONENTS
    [HideInInspector]
    public Enemy enemy;
    [HideInInspector]
    public AudioSource audioSource;
    public Transform feetTransform;

    // SOUND EFFECTS
    [Header("Sound Effects")]
    public SoundClip footstep = new SoundClip();
    public SoundClip vocalise = new SoundClip();
    public SoundClip attack = new SoundClip();
    public SoundClip death = new SoundClip();
    public SoundClip collide = new SoundClip();   

    #endregion

    #region METHODS

    // Initializes components and sound clips
    void Awake()
    {
        if (!enemy)
            enemy = GetComponent<Enemy>();
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();  
    }

    void Start()
    {
        footstep.Initialize();
        vocalise.Initialize();
        attack.Initialize();
        death.Initialize();
        collide.Initialize();
    }

    void Update()
    {
        // Update weakened params
        footstep.UpdateParameter(1, GameManager.TetherManager.weakenedParam);
        vocalise.UpdateParameter(0, GameManager.TetherManager.weakenedParam);
        attack.UpdateParameter(0, GameManager.TetherManager.weakenedParam);
        death.UpdateParameter(0, GameManager.TetherManager.weakenedParam);
        collide.UpdateParameter(0, GameManager.TetherManager.weakenedParam);
    }

    public bool ConditionalAudio(System.Action method, bool condition)
    {
        if (!condition || !enabled) return false;
        method();
        return true;
    }
    

    public void Dispose()
    {
        // Dispose FMOD instances
        footstep.Dispose();
        vocalise.Dispose();
        attack.Dispose();
        death.Dispose();
        collide.Dispose();
    }

    public void PlayWalkAudio()
    {
        float[] parameters = { 0, GameManager.TetherManager.weakenedParam };
        footstep.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, enemy.GetVelocity()), volume, parameters);
        //footstep.PlayDetached(audioSource, AudioManager.GetFMODAttribute(feetTransform, enemy.GetVelocity()), volume, null, parameters);
    }

    public void PlayVocaliseAudio()
    {
        float[] parameters = { 0, GameManager.TetherManager.weakenedParam };
        vocalise.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, enemy.GetVelocity()), volume, parameters);
    }

    public void PlayAttackAudio()
    {
        float[] parameters = { 0, GameManager.TetherManager.weakenedParam };
        attack.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, enemy.GetVelocity()), volume, parameters);
    }

    public void PlayDeathAudio()
    {
        float[] parameters = { 0, GameManager.TetherManager.weakenedParam };
        death.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, enemy.GetVelocity()), volume, parameters);
    }

    public void PlayCollideAudio()
    {
        float[] parameters = { 0, GameManager.TetherManager.weakenedParam };
        collide.PlayAttached(audioSource, AudioManager.GetFMODAttribute(transform, enemy.GetVelocity()), volume, parameters);
    }
    #endregion
}
