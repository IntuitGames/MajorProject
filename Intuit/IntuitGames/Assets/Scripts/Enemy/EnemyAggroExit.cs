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
    void OnTriggerExit(Collider other)
    {
        if (!enemyAggro.enemy.isDead)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")) enemyAggro.ObjectLeftRange(other.gameObject);
            //{
            //    if (!other.GetComponent<Character>().isWeakened)
            //    {
                   
            //    }
            //}
            else if (other.gameObject.layer == LayerMask.NameToLayer("Tether")) enemyAggro.ObjectLeftRange(other.gameObject);

            //{
            //    if (!other.GetComponent<TetherJoint>().IsSevered())
            //    {
            //    }

            //}
        }
    }
}
