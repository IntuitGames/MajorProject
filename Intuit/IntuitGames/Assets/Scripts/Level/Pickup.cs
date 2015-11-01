using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Basic player pickups that trigger when touched.
/// </summary>public class Pickup : Trigger{
    public float jellyValue = 1;
    public bool allowPickupOnMaxJelly = true;
    public SoundClip soundEffect = new SoundClip(AudioManager.Player.FMOD, AudioManager.Type.SoundEffect, AudioManager.Group.Game);

    protected override bool canBeTriggered
    {
        get
        {
            return allowPickupOnMaxJelly ? true : GameManager.PlayerManager.jellyPercentage < 1;
        }
    }

    protected override void OnTrigger(GameObject triggerObject)
    {
        GameManager.PlayerManager.AddJelly(jellyValue);
        soundEffect.PlayAttached(GetComponent<AudioSource>(), AudioManager.GetFMODAttribute(transform, Vector3.zero), 1);
        gameObject.SetActive(false);
        Destroy(gameObject, 5);
    }

    void OnDestroy()
    {
        soundEffect.Dispose();
    }
}