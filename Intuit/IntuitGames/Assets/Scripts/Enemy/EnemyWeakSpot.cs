using UnityEngine;
using System.Collections;

public class EnemyWeakSpot : MonoBehaviour {

	public int enteredCount;
	Enemy enemy;
	bool sliced = false;
	float deathTimer;
	public float MaxDeathTimer = 1.0f;

	// Use this for initialization
	void Start () {
		enemy = transform.parent.gameObject.GetComponent<Enemy>();
		deathTimer = MaxDeathTimer;
	}
	
	// Update is called once per frame
	void Update () {
		if(sliced)
		{
			if(deathTimer > 0.0f)
			{
				deathTimer -= Time.deltaTime;
			}
			else
			{
				Debug.Log("Enemy Death");
				enemy.Death();
				sliced = false;
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{

		if(other.gameObject.layer == LayerMask.NameToLayer("Tether"))
		{
			TetherJoint joint = other.GetComponent<TetherJoint>();
			if(!joint.passingThroughWeakSpot && !joint.IsSevered())
			{
//				Physics.IgnoreCollision(other, enemy.body, true);	//Make tether piece entering the weakspot ignore collision with the body
//				Debug.Log("Enemy Weak Spot collided with " + other.gameObject.name);
				enteredCount++;
				joint.passingThroughWeakSpot = true;
				sliced = true;
			}
			else
			{
				joint.passingThroughWeakSpot = false;
			}
		}
	}
	void OnTriggerExit(Collider other)
	{
		if(other.gameObject.layer == LayerMask.NameToLayer("Tether"))
		{
			TetherJoint joint = other.GetComponent<TetherJoint>();
			if(!joint.passingThroughWeakSpot)
			{
//				Physics.IgnoreCollision(other, enemy.body, false);
				enteredCount--;
			}
//			if(enteredCount <= 0)
//			{
//				enemy.Death();
//				Debug.Log ("Tether passed through weakspot entirely");
//			}
		}
		 
	}
}
