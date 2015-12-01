using UnityEngine;
using System.Collections;

public class weStateIdle : EnemyCoreState<WanderingEnemy> {

    public weStateIdle(EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> owner) : base(owner) { }

    private float idleTimer;

    public override void RecieveAggressionChange(WanderingEnemy owner, bool becomeAggressive)
    {
        if (!becomeAggressive) return;
        else
        {
            owner.fsm.pushState(new weStateSuprise(ownerFSM));
        }
    }

    public override void Begin(WanderingEnemy obj)
    {
        if (obj.showStateDebugs) Debug.Log(this.GetType().ToString() + " has begun!");
        obj.agent.velocity = Vector3.zero;
        obj.agent.Stop();
        idleTimer = Random.Range(obj.minIdleTime, obj.maxIdleTime);
    }

    public override void Update(WanderingEnemy obj)
    {
        if (idleTimer > 0)
            idleTimer -= Time.deltaTime;
        else
        {
            ownerFSM.popState();
        }
    }

    public override void End(WanderingEnemy obj)
    {
        obj.agent.Resume();
    }

    
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
