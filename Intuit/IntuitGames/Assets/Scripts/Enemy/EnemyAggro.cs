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

    public List<GameObject> enteredObs = new List<GameObject>();

    [ReadOnly]
    public int playerObsInRange = 0;

    void Start()
    {
        if (enemy == null) enemy = GetComponentInParent<EnemyBase>();
    }

    public void ObjectEnteredRange(GameObject enteredOb)
    {
        if(!enteredObs.Contains(enteredOb)) enteredObs.Add(enteredOb);
        if (enteredObs.Count == 1)  //Once the first player-related object enters the radius, tell enemy to become aggro
            enemy.SendAggroMessage(true);
    }

    public void ObjectLeftRange(GameObject enteredOb)
    {
        enteredObs.Remove(enteredOb);
        if (enteredObs.Count == 0)
        {
            enemy.SendAggroMessage(false);
        }
    }

    public void StopAggro()
    {
        playerObsInRange = 0;
    }
}

