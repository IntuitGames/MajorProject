using UnityEngine;
using System.Collections;

public class EnemyWeakSpot : EnemyBodyBase {

    [ReadOnly]
	bool sliced = false;

	// Use this for initialization
	protected override void Start () {
        base.Start();
	}

	void OnTriggerEnter(Collider other)
	{

        if (!parentEnemy.isDead)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Tether"))
            {
                TetherJoint joint = other.GetComponent<TetherJoint>();
                if (!joint.passingThroughWeakSpot && !joint.IsSevered())
                {
                    joint.passingThroughWeakSpot = true;
                    if(!sliced)
					{
						parentEnemy.OnDeath();
						sliced = true;
					}
                }
            }
        }
	}
}
