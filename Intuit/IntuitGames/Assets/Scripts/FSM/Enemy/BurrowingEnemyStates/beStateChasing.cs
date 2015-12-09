using UnityEngine;
using System.Collections;
using System;

public class beStateChasing : EnemyCoreState<BurrowingEnemy> {

    public beStateChasing(EnemyFSM<EnemyCoreState<BurrowingEnemy>, BurrowingEnemy> owner) : base(owner) { }

    private bool onSurface = false;

    public override void Begin(BurrowingEnemy obj)
    {
        base.Begin(obj);
        onSurface = obj.fullSurface;
        obj.agentComp.speed = obj.chasingSpeed;
    }

    public override void Update(BurrowingEnemy obj)
    {
        if (!onSurface) onSurface = obj.TranslateModel(true); 

        obj.agentComp.SetDestination(GameManager.TetherManager.joints[(GameManager.TetherManager.joints.Count / 2)].transform.position);

        if(TetherManager.TetherManager.disconnected)
        {
            ownerFSM.popState();
        }

        if (Vector3.Distance(obj.transform.position, obj.agentComp.destination) <= obj.chasingDistanceBuffer)
        {
            ownerFSM.pushState(new beStateAttacking(ownerFSM));
        }

    }

    public override void End(BurrowingEnemy obj)
    {
        base.End(obj);
    }

    public override bool RecieveAggressionChange(BurrowingEnemy owner, bool becomeAggressive)
    {
        if (!becomeAggressive)
            ownerFSM.popState();
        return true;
    }
}
