using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

// Main functionality for the Totem and Softhead Enemies
[RequireComponent(typeof(NavMeshAgent))]
public class WanderingEnemy : EnemyBase {

    public NavMeshAgent agent;
    public EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> fsm;

    [Header("Wandering")]
    public float wanderSpeed;
    public float wanderRadius;
    public float wanderDistanceBuffer;
    protected Vector3 startLocation;

    [Header("Chasing")]
    public float chasingSpeed;

    protected override void Start()
    {
        base.Start();
        fsm = new EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy>(this);
        fsm.pushState(new weStateWander(fsm));
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Update()
    {
        if (!this.isDead)
        {
            fsm.Update();
        }
    }

    public Vector3 getStartLocation() {
        return this.startLocation;
    }

    public override void SendAggroMessage(bool becomeAggro)
    {
        fsm.SendAggressionChange(becomeAggro);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        if (startLocation != Vector3.zero) Gizmos.DrawWireSphere(startLocation, wanderRadius); else Gizmos.DrawWireSphere(transform.position, wanderRadius);
        if (agent != null)
        {
            Gizmos.DrawWireCube(agent.destination, Vector3.one);
        }

    }

}
