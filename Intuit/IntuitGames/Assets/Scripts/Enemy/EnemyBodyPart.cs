using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBodyPart : EnemyBodyBase {

    List<Collision> collisionList = new List<Collision>();

	protected override void Start () 
	{
        base.Start();
		this.GetComponent<Rigidbody>().isKinematic = true;
	}

	void OnCollisionEnter(Collision collision)
	{
		if(!parentEnemy.isDead)
		{
			if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
                ContactPoint contact = collision.contacts[0];
				Vector3 knockbackdir = (contact.point - transform.position).normalized;
				collision.collider.GetComponent<Rigidbody>().velocity = knockbackdir * parentEnemy.knockbackForce;

			}
			else if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Tether"))
			{
				if(!collision.collider.GetComponent<TetherJoint>().passingThroughWeakSpot)
				{
                    collisionList.Add(collision);
                    collision.collider.GetComponent<TetherJoint>().DisconnectAtThisJoint();
                    parentEnemy.SendAggroMessage(false);
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
