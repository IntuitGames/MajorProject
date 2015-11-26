using UnityEngine;
using System.Collections;

public class weStateWander : EnemyCoreState<WanderingEnemy> {

    public weStateWander(EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> owner) : base(owner) { Debug.Log("State Constructed"); }

    public override void RecieveAggressionChange(WanderingEnemy owner, bool becomeAggressive)
    {
        if (!becomeAggressive) return;
        else
        {
			owner.fsm.pushState(new weStateChase(ownerFSM));
        }
    }

    public override void Begin(WanderingEnemy obj)
    {
        obj.agent.speed = obj.wanderSpeed;
		obj.agent.SetDestination (new Vector3 (124, -14.14f, 32.21f));
		NavMeshPath path = new NavMeshPath ();
		obj.agent.CalculatePath (new Vector3 (124, -14.14f, 32.21f), path);
		Debug.Log (path.status.ToString ());
    }
    public override void Update(WanderingEnemy obj)
    {
//        if (!obj.isDead)
//        {
//            if (Vector3.Distance(obj.transform.position, obj.agent.destination) <= obj.wanderDistanceBuffer)
//            {
//                obj.agent.SetDestination(getPathablePosition(obj));
//            }
//        }
    }
    public override void End(WanderingEnemy obj)
    {
        Debug.Log(this.GetType().ToString() + " has Ended");
    }

	private Vector3 getPathablePosition(WanderingEnemy obj)
	{
		bool foundPathable = false;
		Vector3 pos = new Vector3 ();
		int loopCount = 0;
		do {
			Vector2 randPos = Random.insideUnitCircle * obj.wanderRadius;
	        NavMeshPath path = new NavMeshPath();
	        pos = obj.getStartLocation() + (new Vector3(randPos.x, obj.transform.position.y, randPos.y));
	        obj.agent.CalculatePath(pos, path);
	        if (path.status == NavMeshPathStatus.PathPartial || path.status == NavMeshPathStatus.PathComplete)
	        {
	            foundPathable = true;
	        }
			loopCount++;

		} while(!foundPathable || loopCount > 5);

		return pos;
	}

}
