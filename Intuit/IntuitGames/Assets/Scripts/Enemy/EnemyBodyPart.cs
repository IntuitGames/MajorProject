using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBodyPart : MonoBehaviour {

	Enemy enemy;
	Rigidbody rigidBody;
    List<Collision> collisionList = new List<Collision>();

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
			if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
                ContactPoint contact = collision.contacts[0];
				Vector3 knockbackdir = (contact.point - transform.position).normalized;
				collision.collider.GetComponent<Rigidbody>().velocity = knockbackdir * enemy.knockbackForce;

			}
			else if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Tether"))
			{
				if(!collision.collider.GetComponent<TetherJoint>().passingThroughWeakSpot)
				{
                    collisionList.Add(collision);
                    collision.collider.GetComponent<TetherJoint>().DisconnectAtThisJoint();
                    enemy.isAggro = false;
				}
			}
		}
	}

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Tether"))
        {
            if (!collision.collider.GetComponent<TetherJoint>().passingThroughWeakSpot)
            {
                collisionList.Remove(collision);
                //collision.collider.GetComponent<TetherJoint>().DisconnectAtThisJoint();
                //enemy.isAggro = false;
            }
        }
    }

    void BeginBite()
    { 

    }
}
