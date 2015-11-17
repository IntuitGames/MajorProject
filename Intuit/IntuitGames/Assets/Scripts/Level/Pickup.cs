using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// Basic player pickups that trigger when touched.
/// </summary>public class Pickup : Trigger{
    public float jellyValue = 1;
    public bool allowPickupOnMaxJelly = true;
    [Range(0, 50)]
    public float drawRadius = 5;
    public float drawSpeed = 10;
    public bool dynamicDrawSpeed = true;
    public bool returnToInitialPosition = true;
    public SoundClip soundEffect = new SoundClip(AudioManager.Player.FMOD, AudioManager.Type.SoundEffect, AudioManager.Group.Game);

    private Vector3 initialPosition;

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

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        float dist1 = Vector3.Distance(GameManager.PlayerManager.character1Pos, transform.position),
            dist2 = Vector3.Distance(GameManager.PlayerManager.character2Pos, transform.position);

        Vector3 targetPos = (dist1 >= dist2 ? GameManager.PlayerManager.character2Pos : GameManager.PlayerManager.character1Pos);
        Vector3 targetDirection = targetPos - transform.position;

        if (targetDirection.magnitude <= drawRadius)
        {
            transform.position += targetDirection.normalized * Time.deltaTime *
                (dynamicDrawSpeed ? Vector3.Distance(targetPos, transform.position).Normalize(0, drawRadius, drawSpeed, 0) : drawSpeed);
        }
        else if (returnToInitialPosition)
        {
            transform.position += Vector3.ClampMagnitude((initialPosition - transform.position), 1) * Time.deltaTime * drawSpeed / 2;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, drawRadius);
    }

    void OnDestroy()
    {
        soundEffect.Dispose();
    }
}