using UnityEngine;
using System.Collections;

public class weStateWander : EnemyCoreState<WanderingEnemy> {

    public weStateWander(EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> owner) : base(owner) { Debug.Log("State Constructed"); }

    public override void RecieveAggressionChange(WanderingEnemy owner, bool becomeAggressive)
    {
        if (!becomeAggressive) return;
        else
        {
            owner.fsm.pushState(new weStateChase(owner.fsm));
        }
    }

    public override void Begin(WanderingEnemy obj)
    {
        obj.agent.speed = obj.wanderSpeed;
        Debug.Log(this.GetType().ToString() + " has Begun");
    }
    public override void Update(WanderingEnemy obj)
    {
        //if (!obj.isDead)
        //{
        //    if (Vector3.Distance(obj.transform.position, obj.agent.destination) <= obj.wanderDistanceBuffer)
        //    {
        //        bool foundPathable = false;
        //        Vector3 wanderTarget = new Vector3();
        //        while (!foundPathable)
        //        {
        //            Vector2 randPos = Random.insideUnitCircle * obj.wanderRadius;
        //            NavMeshPath path = new NavMeshPath();
        //            wanderTarget = obj.getStartLocation() + (new Vector3(randPos.x, obj.agent.destination.y, randPos.y));
        //            obj.agent.CalculatePath(wanderTarget, path);
        //            //Debug.Log(path.status.ToString());
        //            if (path.status == NavMeshPathStatus.PathComplete)
        //            {
        //                foundPathable = true;
        //            }
        //        }

        //        obj.agent.SetDestination(wanderTarget);
        //    }
        //}
    }
    public override void End(WanderingEnemy obj)
    {
        Debug.Log(this.GetType().ToString() + " has Ended");
    }

}
