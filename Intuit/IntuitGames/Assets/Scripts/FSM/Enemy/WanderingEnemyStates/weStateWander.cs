using UnityEngine;
using System.Collections;

public class weStateWander : EnemyCoreState<WanderingEnemy> {

    public weStateWander(EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> owner) : base(owner) {  }
    
    public override bool RecieveAggressionChange(WanderingEnemy owner, bool becomeAggressive)
    {
        if (becomeAggressive)
			owner.fsm.pushState(new weStateSuprise(ownerFSM));
        return true;
    }

    public override void Begin(WanderingEnemy obj)
    {
        base.Begin(obj);
        obj.agent.speed = obj.wanderSpeed;
		obj.agent.SetDestination (getPathablePosition(obj));
    }
    public override void Update(WanderingEnemy obj)
    {
        if (Vector3.Distance(obj.transform.position, obj.agent.destination) <= obj.wanderDistanceBuffer)
        {
            ownerFSM.pushState(new weStateIdle(ownerFSM));
        }
    }
    public override void End(WanderingEnemy obj)
    {
    }

	private Vector3 getPathablePosition(WanderingEnemy obj)
	{
        bool foundPathable = false;
        Vector3 pos = new Vector3 ();
        do
        {
            Vector2 randPos = Random.insideUnitCircle * obj.wanderRadius;
            NavMeshPath path = new NavMeshPath();
            pos = obj.getStartLocation() + (new Vector3(randPos.x, obj.transform.position.y, randPos.y));
            obj.agent.CalculatePath(pos, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                foundPathable = true;
            }
        } while (!foundPathable);

        return pos;
	}
}
