using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnemyAggroExit : MonoBehaviour
{
    public EnemyAggro enemyAggro;

    void Start()
    {
        if (enemyAggro == null) enemyAggro = transform.parent.GetComponent<EnemyAggro>();
    }

    //Each time a player/tether objects leaves the aggro range, subtract from the number of them that have entered.
    //This allows the enemy to stay aggro'd to the players as long as theres something within its range
    void OnTriggerEnter(Collider other)
    {
        if (!enemyAggro.enemy.isDead)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (!other.GetComponent<Character>().isWeakened)
                {
                    enemyAggro.ObjectLeftRange();
                }
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Tether"))
            {
                if (!other.GetComponent<TetherJoint>().IsSevered())
                {
                    enemyAggro.ObjectLeftRange();
                }

            }
        }
    }
}
