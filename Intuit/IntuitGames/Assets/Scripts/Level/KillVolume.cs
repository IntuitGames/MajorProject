using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;public class KillVolume : Trigger{
    public bool killBoth = false;

    protected override void OnTrigger(GameObject triggerObject)
    {
        if (killBoth || !GameManager.PlayerManager.isWeakened)
            GameManager.PlayerManager.DeathAction();
        else
            GameManager.PlayerManager.SingleDeath(triggerObject.GetComponent<Character>().isPlayerOne);
    }
}