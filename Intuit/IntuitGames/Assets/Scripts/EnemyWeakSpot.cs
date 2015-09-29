using UnityEngine;
using System.Collections;

public class EnemyWeakSpot : MonoBehaviour {

	int enteredCount;
	Enemy enemy;

	// Use this for initialization
	void Start () {
		enemy = transform.parent.gameObject.GetComponent<Enemy>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other)
	{

		if(other.gameObject.layer == LayerMask.NameToLayer("Tether"))
		{
			TetherJoint joint = other.GetComponent<TetherJoint>();
			if(!joint.passingThroughWeakSpot)
			{
				Physics.IgnoreCollision(other, enemy.body, true);	//Make tether piece entering the weakspot ignore collision with the body
				Debug.Log("Enemy Weak Spot collided with " + other.gameObject.name);
				enteredCount++;
				joint.passingThroughWeakSpot = true;
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
				Physics.IgnoreCollision(other, enemy.body, false);
				enteredCount--;
			}
			if(enteredCount <= 0)
			{
				enemy.Death();
				Debug.Log ("Tether passed through weakspot entirely");
			}
		}
		 
	}
}
