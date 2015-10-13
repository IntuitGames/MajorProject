using UnityEngine;
using System.Collections;

public class Aggressive : FSMState<Enemy> {

	public Aggressive (FSM<Enemy> fsmOwner) : base(fsmOwner)
	{
		
	}

	public override void Begin(Enemy obj)
	{
		obj.agent.speed = obj.aggroSpeed;
		Vector3 velocity = obj.agent.velocity;
		velocity.y = CalculateJumpHeightSpeed(obj.aggroJumpHeight);
		obj.agent.velocity = velocity;
	}
	public override void Update(Enemy obj)
	{
		if(!obj.isDead) obj.agent.SetDestination (GetCenterJointPos());
	}
	public override void End(Enemy obj)
	{
	}

	Vector3 GetCenterJointPos()
	{
		return GameManager.TetherManager.joints[(GameManager.TetherManager.joints.Count/2)].transform.position;
	}

	float CalculateJumpHeightSpeed(float aimHeight)
	{
		float yVelo = Mathf.Sqrt(Mathf.Abs( 2f * aimHeight * Physics.gravity.y));
		return yVelo;
	}
}
