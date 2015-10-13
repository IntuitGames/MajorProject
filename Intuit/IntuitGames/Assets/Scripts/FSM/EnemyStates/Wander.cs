using UnityEngine;
using System.Collections;

public class Wander : FSMState<Enemy> {

	public Wander (FSM<Enemy> fsmOwner) : base(fsmOwner)
	{

	}

		
	public override void Begin(Enemy obj)
	{
		obj.agent.speed = obj.wanderSpeed;
	}
	public override void Update(Enemy obj)
	{
		if(!obj.isDead)
		{
			if(Vector3.Distance(obj.transform.position, obj.agent.destination) <= obj.wanderDistBuffer)
			{
				bool foundPathable = false;
				Vector3 wanderTarget = new Vector3();
				while(!foundPathable)
				{
					Vector2 randPos = Random.insideUnitCircle * obj.wanderRadius;
					NavMeshPath path = new NavMeshPath();
					wanderTarget = obj.startLocation + (new Vector3(randPos.x, obj.agent.destination.y, randPos.y));
					obj.agent.CalculatePath(wanderTarget, path);
					//Debug.Log(path.status.ToString());
					if(path.status == NavMeshPathStatus.PathComplete)
					{
						foundPathable = true;
					}
				}
				
				obj.agent.SetDestination(wanderTarget);
			}
		}
	}
	public override void End(Enemy obj)
	{
	}
}
