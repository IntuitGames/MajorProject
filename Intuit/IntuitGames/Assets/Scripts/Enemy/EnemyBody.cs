using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBody : MonoBehaviour {

	Enemy enemy;
	Rigidbody rigidBody;

	void Start () 
	{
		enemy = transform.parent.gameObject.GetComponent<Enemy>();
		rigidBody = GetComponent<Rigidbody>();
		rigidBody.isKinematic = true;
	}

	void OnCollisionEnter(Collision collision)
	{
		if(!enemy.isDead)
		{
			ContactPoint contact = collision.contacts[0];
			if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
				Vector3 knockbackdir = (contact.point - transform.position).normalized;
				collision.collider.GetComponent<Rigidbody>().velocity = knockbackdir * enemy.knockbackForce;

			}
			else if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Tether"))
			{
				if(!collision.collider.GetComponent<TetherJoint>().passingThroughWeakSpot)
				{
					collision.collider.GetComponent<TetherJoint>().DisconnectAtThisJoint();
					enemy.isAggro = false;
				}
			}
		}
	}
}
