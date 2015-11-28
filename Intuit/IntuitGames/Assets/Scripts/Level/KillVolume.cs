using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;public class KillVolume : Trigger{
    protected override void OnTrigger(GameObject triggerObject)
    {
        GameManager.PlayerManager.DeathAction();
    }
}