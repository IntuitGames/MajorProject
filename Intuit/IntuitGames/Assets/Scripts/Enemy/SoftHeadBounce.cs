using UnityEngine;
using System.Collections;
using System;

public class SoftHeadBounce : Trigger
{
    public SoftheadEnemy parentEnemy;
    
    protected override bool canBeTriggered
    {
        get
        {
            return !parentEnemy.isDead;
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (triggerLayer == (triggerLayer | (1 << collision.collider.gameObject.layer)) && collision.contacts[0].point.y > transform.position.y)    //make sure coming from above
            CallTrigger(collision.collider.gameObject);
    }

    protected override void OnTrigger(GameObject triggerObject)
    {
        parentEnemy.Stomped();
    }
    
}
