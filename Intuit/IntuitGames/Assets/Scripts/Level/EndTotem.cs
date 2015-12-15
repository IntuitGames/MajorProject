using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;public class EndTotem : Trigger{
    public float victoryDelay = 0.5f;
    public bool loadNewScene = true;
    public int sceneIndex = 0;
    public bool noDashingRequired = true;
    public bool requireBothToDash = false;
    public bool requireNotWeakened = true;
    public SoundClip cutSound = new SoundClip(AudioManager.Player.FMOD, AudioManager.Type.SoundEffect, AudioManager.Group.Game);

    protected override bool canBeTriggered
    {
        get
        {
            if (requireNotWeakened && GameManager.PlayerManager.isWeakened) return false;
            if (noDashingRequired)
                return true;
            else if (requireBothToDash)
                return GameManager.PlayerManager.character1.isDashing && GameManager.PlayerManager.character2.isDashing;
            else
                return GameManager.PlayerManager.character1.isDashing || GameManager.PlayerManager.character2.isDashing;
        }
    }

    void Start()
    {
        cutSound.Initialize();
    }

    protected override void OnTrigger(GameObject triggerObject)
    {
        cutSound.PlayAttached(GetComponent<AudioSource>(), AudioManager.GetFMODAttribute(transform, Vector3.zero), 1);

        if (loadNewScene)
        {
            if (victoryDelay > 0)
                TimerPlus.Create(victoryDelay, () => GameManager.LoadLevel(sceneIndex));
            else
                GameManager.LoadLevel(sceneIndex);
        }
    }
}