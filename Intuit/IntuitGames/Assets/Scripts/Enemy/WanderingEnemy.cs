using UnityEngine;
using CustomExtensions;
using System;

// Main functionality for the Totem and Softhead Enemies
[RequireComponent(typeof(NavMeshAgent))]
public class WanderingEnemy : Enemy {

    public EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy> fsm;

    [Header("Wandering Components")]
    public NavMeshAgent agent;

    [Header("Wandering")]
    [Range(0, 2f)]
    public float wanderSpeed;
    public float wanderRadius;
    public float wanderDistanceBuffer;
    [Range(0f, 10f), Tooltip("Upon reaching their next wandering destination, what is the minimum time this creature will idle?")]
    public float minIdleTime;
    [Range(1f, 10f), Tooltip("Upon reaching their next wandering destination, what is the maximum time this creature will idle?")]
    public float maxIdleTime;
    protected Vector3 startLocation;

    [Header("Chasing")]
    [Range(1, 10)]
    public float chasingSpeed;
    [Range(1, 5), Tooltip("Close the enemy will get before attempting to bite.")]
    public float chasingDistanceBuffer;

    [Header("Bite")]
    public BiteController biteController;
    [Tooltip("How long the trigger for biting the tether will stick around for"), Range(0f, 1f)]
    public float biteHang;
    [Tooltip("How long it takes for the enemy to bite the tether upon reaching it"), Range(0f, 2f)]
    public float biteEffectDuration;

    protected override void Start()
    {
        base.Start();
        this.startLocation = this.transform.position;
        fsm = new EnemyFSM<EnemyCoreState<WanderingEnemy>, WanderingEnemy>(this);
        fsm.pushState(new weStateWander(fsm));
        agent = GetComponent<NavMeshAgent>();
    }

    protected override void Update()
    {
        base.Update();
        if (!this.isDead)
        {
            fsm.Update();
        }
    }

    protected override void UpdateAnimator()
    {
        if (riggedModel.activeInHierarchy)
        {
            base.UpdateAnimator();
            this.animatorComp.SetFloat("speed", this.agent.velocity.IgnoreY2().magnitude);
        }
    }

    public override void OnDeath(bool swapModel)
    {
        biteController.StopBiteEffect();
        StopAgent();
        base.OnDeath(swapModel);
    }

    public Vector3 getStartLocation() {
        return this.startLocation;
    }

    public override void SendAggroMessage(bool becomeAggro)
    {
        fsm.SendAggressionChange(becomeAggro);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        if (startLocation != Vector3.zero) Gizmos.DrawWireSphere(startLocation, wanderRadius); else Gizmos.DrawWireSphere(transform.position, wanderRadius);
        if (agent != null)
        {
            Gizmos.DrawWireCube(agent.destination, Vector3.one);
        }

    }

    public override Vector3 GetVelocity()
    {
        return agent.velocity;
    }

    public void StopAgent()
    {
        agent.velocity = Vector3.zero;
        agent.Stop();
    }

}
