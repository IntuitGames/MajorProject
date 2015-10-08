using UnityEngine;
using System.Collections;

public class Aggressive : FSMState<Enemy> {

//	Aggressive (FSM<Enemy> fsmOwner) : base(fsmOwner);

	public override void Begin(Enemy obj)
	{
		Debug.Log ("<color=green>Wander State Begun</color>");
		obj.agent.SetDestination (obj.transform.position);
	}
	public override void Update(Enemy obj)
	{
		//Move Towards the player's tether
	}
	public override void End(Enemy obj)
	{
		Debug.Log ("<color=green>Wander State Ended</color>");
	}
}
