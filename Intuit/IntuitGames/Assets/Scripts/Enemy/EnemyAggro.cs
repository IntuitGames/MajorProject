using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnemyAggro : MonoBehaviour
{
    public EnemyAggroEnter enterZone;
    public EnemyAggroExit exitZone;

    public Enemy enemy;

    [ReadOnly]
    public int playerObsInRange = 0;

    void Start()
    {
        if(enemy == null) enemy = transform.parent.gameObject.GetComponent<Enemy>();
    }

    public void ObjectEnteredRange()
    {
        if (!enemy.isAggro) enemy.isAggro = true;
        playerObsInRange++;
    }

    public void ObjectLeftRange()
    {
        playerObsInRange--;
        if (playerObsInRange <= 0 && enemy.isAggro)
        {
            enemy.isAggro = false;
        }
    }

    public void StopAggro()
    {
        playerObsInRange = 0;
    }
}

