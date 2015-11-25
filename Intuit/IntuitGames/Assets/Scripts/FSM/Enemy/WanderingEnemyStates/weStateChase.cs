using UnityEngine;
using System.Collections;

public class weStateChase : EnemyCoreState<WanderingEnemy>
{

    public weStateChase(EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> owner) : base(owner) { }

    public override void RecieveAggressionChange(WanderingEnemy owner, bool becomeAggressive)
    {

    }

    public override void Begin(WanderingEnemy obj)
    {
        obj.agent.speed = obj.chasingSpeed;
        Debug.Log(this.GetType().ToString() + " has Begun");
    }
    public override void Update(WanderingEnemy obj)
    {
        obj.agent.SetDestination(GetCenterJointPos());
    }
    public override void End(WanderingEnemy obj)
    {
        Debug.Log(this.GetType().ToString() + " has Ended");
    }

    Vector3 GetCenterJointPos()
    {
        return GameManager.TetherManager.joints[(GameManager.TetherManager.joints.Count / 2)].transform.position;
    }
}
