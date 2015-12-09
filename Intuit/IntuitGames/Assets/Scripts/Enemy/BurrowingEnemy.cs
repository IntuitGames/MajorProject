using UnityEngine;
using System.Collections.Generic;
using System;
using CustomExtensions;

[RequireComponent(typeof(NavMeshAgent))]
public class BurrowingEnemy : Enemy {

    public EnemyFSM<EnemyCoreState<BurrowingEnemy>, BurrowingEnemy> fsm;
    [Header("Component References")]
    public NavMeshAgent agentComp;
    public BurrowingBiteController biteComp;
    public SimpleAnimationControl dizzyStars;
    public ParticleSystem burrowingEffect;
    public BurrowerStomp stompController;

    [HideInInspector]
    public Vector3 startLocation;
    [HideInInspector]
    public Vector3 modelStartLocation;

    [Header("Passive")]
    [Range(0.1f, 10), Tooltip("As the burrower moves back to its starting spot after losing the players, how fast should it move?")]
    public float idleSpeed;
    [Range(1, 10), Tooltip("How Close the enemy will get to its starting position before stoppinf when returning to idle")]
    public float idleDistanceBuffer;


    [Header("Chasing")]
    [Range(1, 10), Tooltip("How fast the enemy goes to and from the underground")]
    public float burrowRate = 5f;
    [Range(1, 10)]
    public float chasingSpeed;
    [Range(1, 10), Tooltip("How Close the enemy will get before starting to charge and bite.")]
    public float chasingDistanceBuffer;

    [Header("Charging")]
    [Range(1, 10)]
    public float chargingSpeed;
    [Range(1, 50), Tooltip("How far beyond the tether's position the enemy will charge towards")]
    public float chargeThroughDist;
    [Range(1, 10)]
    public float chargeDistanceBuffer;

    [Header("Stunned")]
    [Range(1, 10)]
    public float stunDuration = 3f;
    private bool _isStunned;
    public bool isStunned
    {
        get { return _isStunned; }
        set
        {
            if (!_isStunned && value)
            {
                _isStunned = value;
                fsm.pushState(new beStateStunned(fsm));
            }
            if (_isStunned && !value)
            {
                _isStunned = value;
                player1Primed = false;
                player2Primed = false;
            }
        }
    }
    private bool player1Primed = false;
    private bool player2Primed = false;

    public bool fullSurface { get { return riggedModel.transform.position.y >= 0f; } }

    public bool fullUnderground { get { return riggedModel.transform.position.y <= modelStartLocation.y; } }

    protected override void Start()
    {
        base.Start();
        startLocation = this.transform.position;
        modelStartLocation = riggedModel.transform.localPosition;
        fsm = new EnemyFSM<EnemyCoreState<BurrowingEnemy>, BurrowingEnemy>(this);
        fsm.pushState(new beStateIdle(fsm));
    }

    protected override void Update()
    {
        base.Update();
        if (!this.isDead)
        {
            fsm.Update();
            
        }
        if (stompController.playersInside && !isStunned)
            isStunned = checkStunConditions(stompController.playersEntered);
    }

    protected override void UpdateAnimator()
    {
        if (riggedModel.activeInHierarchy)
        {
            base.UpdateAnimator();
            animatorComp.SetFloat("speed", this.agentComp.velocity.IgnoreY2().magnitude);
        }
    }
    public override void DeathEffect()
    {
        base.DeathEffect();
        burrowingEffect.Stop();
    }

    public override void OnDeath(bool swapModel)
    {
        base.OnDeath(swapModel);
        riggedModel.transform.localPosition = new Vector3(0, 2.5f, 0);
    }

    public void StopAgent()
    {
        agentComp.velocity = Vector3.zero;
        agentComp.Stop();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        if (agentComp != null)
        {
            Gizmos.DrawWireCube(agentComp.destination, Vector3.one);
        }
    }

    public override void SendAggroMessage(bool becomeAggro)
    {
        fsm.SendAggressionChange(becomeAggro);
    }

    public bool TranslateModel(bool up)
    {
        if (up)
            if (!fullSurface)
            {
                if (!burrowingEffect.isPlaying) burrowingEffect.Play();
                riggedModel.transform.Translate(new Vector3(0,burrowRate * Time.deltaTime,0));
                return false;
            }
            else return true;
        else
        {
            if (!fullUnderground)
            {
                riggedModel.transform.Translate(new Vector3(0, -burrowRate * Time.deltaTime, 0));
                return false;
            }
            else
            {
                if (burrowingEffect.isPlaying) burrowingEffect.Stop();
                return true;
            }
        }
    }

    private bool checkStunConditions(List<Character> players)
    {
        bool shouldStun = false;
        foreach(Character player in players)
        {
            if (player.isPlayerOne)
            {
                if (!player1Primed)
                    player1Primed = checkIfHeavyInAir(player);
                else if (checkIfHeavyOnGround(player))
                    return shouldStun = true;
            }
            else
            {
                if (!player2Primed)
                    player2Primed = checkIfHeavyInAir(player);
                else if (checkIfHeavyOnGround(player))
                    return shouldStun = true;
            }
        }
        return shouldStun;
    }

    private bool checkIfHeavyInAir(Character character)
    {
        return !character.isGrounded && character.isHeavy;
    }

    private bool checkIfHeavyOnGround(Character character)
    {
        return character.isGrounded && character.isHeavy;
    }
}
