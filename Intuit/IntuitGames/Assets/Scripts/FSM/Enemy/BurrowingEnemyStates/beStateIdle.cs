using UnityEngine;
using System.Collections;
using System;

public class beStateIdle : EnemyCoreState<BurrowingEnemy> {

    public beStateIdle(EnemyFSM<EnemyCoreState<BurrowingEnemy>, BurrowingEnemy> owner) : base(owner) { }

    private bool underground;
    private bool stopped = false;

    public override void Begin(BurrowingEnemy obj)
    {
        base.Begin(obj);
        stopped = false;
        underground = obj.fullUnderground;
        obj.animatorComp.speed = 0.2f;
        obj.agentComp.SetDestination(obj.startLocation);
        obj.agentComp.speed = obj.idleSpeed;
    }

    public override void Update(BurrowingEnemy obj)
    {
        if (!underground) underground = obj.TranslateModel(false);
        
        if (!stopped)
            if (Vector3.Distance(obj.transform.position, obj.agentComp.destination) <= obj.idleDistanceBuffer)
            {
                obj.StopAgent();
                stopped = true;
            }
        
    }

    public override void End(BurrowingEnemy obj)
    {
        base.End(obj);
        obj.animatorComp.speed = 1f;
        if (stopped)
            obj.agentComp.Resume();
    }

    public override bool RecieveAggressionChange(BurrowingEnemy owner, bool becomeAggressive)
    {
        if (becomeAggressive)
            ownerFSM.pushState(new beStateChasing(ownerFSM));
        return true;
    }
}
