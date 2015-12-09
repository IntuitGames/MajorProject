using System;
using UnityEngine;
using CustomExtensions;

public class beStateAttacking : EnemyCoreState<BurrowingEnemy>
{
    public beStateAttacking(EnemyFSM<EnemyCoreState<BurrowingEnemy>, BurrowingEnemy> owner) : base(owner) { }

    private bool onSurface = false;

    public override void Begin(BurrowingEnemy obj)
    {
        base.Begin(obj);
        onSurface = obj.fullSurface;
        obj.agentComp.SetDestination(Unity.GetPointAtDistAlongLine(obj.transform.position, TetherManager.TetherManager.joints[TetherManager.TetherManager.joints.Count / 2].transform.position, obj.chargeThroughDist, false));
        obj.agentComp.speed = obj.chargingSpeed;
        obj.biteComp.Bite();
    }

    public override void Update(BurrowingEnemy obj)
    {
        if (!onSurface) onSurface = obj.TranslateModel(true);
        if (Vector3.Distance(obj.transform.position, obj.agentComp.destination) <= obj.chargeDistanceBuffer)
        {
            ownerFSM.popState();
        }
    }

    public override void End(BurrowingEnemy obj)
    {
        base.End(obj);
        obj.biteComp.StopBite();
    }

    public override bool RecieveAggressionChange(BurrowingEnemy owner, bool becomeAggressive)
    {
        return false;
    }
}

