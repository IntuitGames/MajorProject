using System;
using UnityEngine;

public class EnemyBodyPart : Trigger{

    public Enemy parentEnemy;

    private ContactPoint contact;

    protected override bool canBeTriggered
    {
        get
        {
            return !parentEnemy.isDead;
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        contact = collision.contacts[0];
        base.OnCollisionEnter(collision);
    }

    protected override void OnTrigger(GameObject triggerObject)
    {
        Vector3 knockbackdir = (contact.point - transform.position).normalized;
        parentEnemy.audioDataComp.PlayCollideAudio();
        triggerObject.GetComponent<Character>().Knockback(knockbackdir);
    }
}
