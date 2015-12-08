using System;
using UnityEngine;


public class WeakSpot : Trigger
{
    public Enemy ownerEnemy;

    protected override bool canBeTriggered
    {
        get
        {
            return !TetherManager.TetherManager.disconnected && (GameManager.PlayerManager.character1.isDashing || GameManager.PlayerManager.character2.isDashing); // When it works nicely without dashing, 
        }
    }

    protected override void OnTrigger(GameObject triggerObject)
    {
        ownerEnemy.OnDeath(true);
    }
}
