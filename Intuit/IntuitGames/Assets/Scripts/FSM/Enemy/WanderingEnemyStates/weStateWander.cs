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
        Vector3 pos = new Vector3 ();		
        Vector2 randPos = Random.insideUnitCircle * obj.wanderRadius;
        NavMeshPath path = new NavMeshPath();
        pos = obj.getStartLocation() + (new Vector3(randPos.x, 0, randPos.y));
        obj.agent.CalculatePath(pos, path);
        switch(path.status)
        {
            case NavMeshPathStatus.PathComplete:
                {
                    break;
                }
            case NavMeshPathStatus.PathPartial:
                {
                    TimerPlus.Create(3f, TimerPlus.Presets.OneTimeUse, ()=>FindNewPath(obj) );
                    break;
                }
            case NavMeshPathStatus.PathInvalid:
                {
                    pos = obj.getStartLocation();
                    //pos = getPathablePosition(obj);                   
                    break;
                }
            default:
                break;        
        } 
        return pos;
	}

    void FindNewPath(WanderingEnemy obj)
    {
        obj.agent.SetDestination(getPathablePosition(obj));
    }
}
