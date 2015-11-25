using UnityEngine;
using System.Collections;

public class EnemyAggroEnter : MonoBehaviour {

    public EnemyAggro enemyAggro;

    void Start()
    {
        if (enemyAggro == null) enemyAggro = transform.parent.GetComponent<EnemyAggro>();
    }

	//Whenever a player or tether piece enters the aggro trigger, set the enemy to the aggro state and add to the count of how many tethers/players have entered 
    void OnTriggerEnter(Collider other)
    {
        if (!enemyAggro.enemy.isDead)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (!other.GetComponent<Character>().isWeakened)
                {
                    enemyAggro.ObjectEnteredRange();
                }
            }

            else if (other.gameObject.layer == LayerMask.NameToLayer("Tether"))
            {
                if (!other.GetComponent<TetherJoint>().IsSevered())
                {
                    enemyAggro.ObjectEnteredRange();
                }
            }
        }
    }
}
