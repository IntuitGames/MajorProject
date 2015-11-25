using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnemyAggro : MonoBehaviour
{
    public EnemyAggroEnter enterZone;
    public EnemyAggroExit exitZone;

    public EnemyBase enemy;

    [ReadOnly]
    public int playerObsInRange = 0;

    void Start()
    {
        if (enemy == null) enemy = GetComponentInParent<EnemyBase>();
    }

    public void ObjectEnteredRange()
    {
        enemy.SendAggroMessage(true);
        playerObsInRange++;
    }

    public void ObjectLeftRange()
    {
        playerObsInRange--;
        if (playerObsInRange <= 0)
        {
            enemy.SendAggroMessage(false);
        }
    }

    public void StopAggro()
    {
        playerObsInRange = 0;
    }
}

