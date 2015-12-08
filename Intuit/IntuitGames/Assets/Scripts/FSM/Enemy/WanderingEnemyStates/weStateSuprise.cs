using System;
using UnityEngine;


public class weStateSuprise : EnemyCoreState<WanderingEnemy>
{
    public weStateSuprise(EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> owner) : base(owner) { }

    public override void RecieveAggressionChange(WanderingEnemy owner, bool becomeAggressive)
    {
        if (!becomeAggressive)
        {
            if(owner.showStateDebugs)Debug.Log("Suprised and leaving aggressive!");
            owner.animatorComp.SetBool("aggressive", false);
            ownerFSM.popState();
        }
    }

    public override void Begin(WanderingEnemy obj)
    {
        if (obj.showStateDebugs) Debug.Log(this.GetType().ToString() + " has begun!");
        obj.agent.velocity = Vector3.zero;
        obj.agent.Stop();
        obj.animatorComp.SetBool("suprised", true);
        obj.animatorComp.SetBool("aggressive", true);
    }

    public override void Update(WanderingEnemy obj)
    {
        if (obj.animatorComp.GetCurrentAnimatorStateInfo(0).IsName("Suprised") && obj.animatorComp.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            ownerFSM.popState();    //remove this state from the stack because at no point should we really be becoming suprised again unless its coming from wander/idle
            ownerFSM.pushState(new weStateChase(ownerFSM));
        }
    }

    public override void End(WanderingEnemy obj)
    {
        obj.agent.Resume();
        obj.animatorComp.SetBool("suprised", false);
    }
}
