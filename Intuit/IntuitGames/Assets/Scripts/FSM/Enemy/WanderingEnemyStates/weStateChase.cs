using UnityEngine;
using System.Collections;

public class weStateChase : EnemyCoreState<WanderingEnemy>
{

    public weStateChase(EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> owner) : base(owner) { }

    public override void RecieveAggressionChange(WanderingEnemy owner, bool becomeAggressive)
    {
        if (!becomeAggressive)
        {
            owner.animatorComp.SetBool("aggressive", false);
            ownerFSM.popState();
        }
    }

    public override void Begin(WanderingEnemy obj)
    {
        if (obj.showStateDebugs) Debug.Log(this.GetType().ToString() + " has begun!");
        obj.agent.speed = obj.chasingSpeed;
    }
    public override void Update(WanderingEnemy obj)
    {
        if(TetherManager.TetherManager.disconnected)
        {
            obj.animatorComp.SetBool("aggressive", false);
            obj.aggroHandler.enteredObs.Clear();
            ownerFSM.popState();
        }
        obj.agent.SetDestination(GetCenterJointPos());
        if (Vector3.Distance(obj.transform.position, obj.agent.destination) <= obj.chasingDistanceBuffer)
        {
            ownerFSM.pushState(new weStateAttack(ownerFSM));
        }
    }
    public override void End(WanderingEnemy obj)
    {
    }

    Vector3 GetCenterJointPos()
    {
        return GameManager.TetherManager.joints[(GameManager.TetherManager.joints.Count / 2)].transform.position;
    }
}
