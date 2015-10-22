using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : EnemyBase {

	private bool _isAggro;
	public bool isAggro
	{
		get{ return _isAggro; }

		set
		{
			//If the enemy is already aggressive towards the player and we're told to no longer be, pop the Aggressive state
			if (_isAggro && !value)
			{
                aggroHandler.StopAggro();
				fsm.popState();
			}
			//If we are not aggressive and we are told to be, push the aggressive state
			else if (!_isAggro && value)
			{
				fsm.pushState(new Aggressive(fsm));
			}
			_isAggro = value;

		}
	}
	[ReadOnlyAttribute]
	public bool isDead = false;
	[Header("Wandering")]
	public float wanderSpeed = 1;
	[Tooltip("The radius of the circle from the enemy's starting location in which they'll wander")]
	public int wanderRadius = 5;
	[Tooltip("How close the enemy will get to their wandering position before finding a new position to wander towards.")]
	public float wanderDistBuffer = 0.5f;

	[Header("Aggressive")]
	public float aggroSpeed = 4;
	public float aggroJumpHeight = 2;
	[Tooltip("How hard the enemy will push the player when they collide with the enemy")]
	public float knockbackForce;

    [Header("Attack")]


	
	[HideInInspector]
	public Vector3 startLocation;
	[HideInInspector]
	public NavMeshAgent agent;
	private FSM<Enemy> fsm;


	void Awake () {
		agent = GetComponent<NavMeshAgent>();
		fsm = new FSM<Enemy>(this);
        aggroHandler = GetComponentInChildren<EnemyAggro>();

	}

	void Start () {
		startLocation = transform.position;
		fsm.pushState(new Wander(fsm));
	}
	
	void Update () {
		fsm.Update();
	}



	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		if(startLocation != Vector3.zero) Gizmos.DrawWireSphere(startLocation, wanderRadius); else Gizmos.DrawWireSphere(transform.position, wanderRadius);
		if(agent != null)
		{
			Gizmos.DrawWireCube(agent.destination, Vector3.one);
		}

	}

	public void Death()
	{
		foreach(Rigidbody rb in BodyParts)
		{
			rb.isKinematic = false;
			rb.AddForce(Random.insideUnitSphere * gibForce);
			agent.enabled = false;
			isDead = true;
		}
	}
}
