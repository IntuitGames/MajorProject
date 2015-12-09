using UnityEngine;
using System.Collections.Generic;
using System;

public class BurrowerStomp : Trigger {

    public BurrowingEnemy parentEnemy;

    public List<Character> playersEntered = new List<Character>();

    public bool playersInside { get { return playersEntered.Count > 0; } }

    protected override bool canBeTriggered
    {
        get
        {
            return !parentEnemy.isDead;
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (triggerLayer == (triggerLayer | (1 << other.gameObject.layer)))
            playersEntered.Remove(other.GetComponent<Character>());
    }

    protected override void OnTrigger(GameObject triggerObject)
    {
        playersEntered.Add(triggerObject.GetComponent<Character>());
    }
}
