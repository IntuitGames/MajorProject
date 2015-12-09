using UnityEngine;
using System.Collections;

public class weStateAttack : EnemyCoreState<WanderingEnemy>
{

    public weStateAttack(EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> owner) : base(owner) { }

    private float biteTimer;

    public override bool RecieveAggressionChange(WanderingEnemy owner, bool becomeAggressive)
    {
        //if(!becomeAggressive)
        //{
        //    ownerFSM.popState();    //Pop the attacking state
        //    ownerFSM.popState();    //Pop the chasing state, which must be below this as the only way to ever arrive at weStateAttack is through Chase
        //    owner.animatorComp.SetBool("aggressive", false);
        //}
        return false;
    }

    public override void Begin(WanderingEnemy obj)
    {
        base.Begin(obj);
        obj.StopAgent();
        obj.animatorComp.SetBool("attacking", true);
        obj.biteController.Bite(obj.biteEffectDuration, obj.biteHang);
        biteTimer = (obj.biteEffectDuration * 2) + obj.biteHang; //the total duration of the biting process, should change this to instead respond to a message from the biteController but w/e haha
    }
    public override void Update(WanderingEnemy obj)
    {
        Vector3 lookatpos = GameManager.TetherManager.joints[(GameManager.TetherManager.joints.Count / 2)].transform.position;
        lookatpos.y = obj.transform.position.y;
        obj.transform.LookAt(lookatpos);
        if (biteTimer > 0) biteTimer -= Time.deltaTime;
        else ownerFSM.popState();
    }
    public override void End(WanderingEnemy obj)
    {
        obj.agent.Resume();
        obj.animatorComp.SetBool("attacking", false);
    }
}

