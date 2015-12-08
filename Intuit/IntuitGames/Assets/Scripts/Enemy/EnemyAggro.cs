using System.Collections.Generic;
using UnityEngine;

public class EnemyAggro : MonoBehaviour
{
    public EnemyAggroEnter enterZone;
    public EnemyAggroExit exitZone;

    public Enemy enemy;

    public List<GameObject> enteredObs = new List<GameObject>();

    [ReadOnly]
    public int playerObsInRange = 0;

    void Start()
    {
        if (enemy == null) enemy = GetComponentInParent<Enemy>();
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

