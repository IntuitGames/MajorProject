using UnityEngine;

public class EnemyBodyPart : MonoBehaviour{

    public EnemyBase parentEnemy;

	void Start () 
	{

	}

	void OnCollisionEnter(Collision collision)
	{
		if(!parentEnemy.isDead && collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
            ContactPoint contact = collision.contacts[0];
			Vector3 knockbackdir = (contact.point - transform.position).normalized;
			collision.collider.GetComponent<Rigidbody>().velocity = knockbackdir * parentEnemy.knockbackForce;
		}
	}
}
