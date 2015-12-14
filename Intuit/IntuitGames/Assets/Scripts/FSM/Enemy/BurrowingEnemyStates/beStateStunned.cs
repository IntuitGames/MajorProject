﻿using UnityEngine;
using System.Collections;
using System;
using CustomExtensions;

public class beStateStunned : EnemyCoreState<BurrowingEnemy> {

    public beStateStunned(EnemyFSM<EnemyCoreState<BurrowingEnemy>, BurrowingEnemy> owner) : base(owner) { }

    private bool onSurface = false;

    public override void Begin(BurrowingEnemy obj)
    {
		base.Begin (obj);
        TimerPlus.Create(obj.stunDuration, TimerPlus.Presets.OneTimeUse, () => EndStun(obj));
        onSurface = obj.fullSurface;
        obj.StopAgent();
        obj.animatorComp.SetBool("stunned", true);
        obj.dizzyStars.gameObject.SetActive(true);
        obj.audioDataComp.PlayStunnedAudio();
    }

    public override void Update(BurrowingEnemy obj)
    {
        if (!onSurface) onSurface = obj.TranslateModel(true);
    }

    public override void End(BurrowingEnemy obj)
    {
        base.End(obj);
        obj.dizzyStars.gameObject.SetActive(false);
        if (!obj.isDead)
        {
            obj.animatorComp.SetBool("stunned", false);
            obj.isStunned = false;
            obj.agentComp.Resume();
        }
      
    }

    public override bool RecieveAggressionChange(BurrowingEnemy owner, bool becomeAggressive)
    {
        return false;
    }

    void EndStun(BurrowingEnemy obj) 
    {
        if (obj != null)
            if (!obj.isDead && obj.gameObject.activeInHierarchy)
                ownerFSM.popState();
    }
}
