using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CapsuleCollider), typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour {

	[HideInInspector]
	public Collider body;

	private bool _isAggro;
	public bool isAggro
	{
		get{ return _isAggro; }

		set
		{
			//If the enemy is already aggressive towards the player and we're told to no longer be, pop the Aggressive state
			if (_isAggro && !value)
			{
				fsm.popState();
			}
			//If we are not aggressive and we are told to be, push the aggressive state
			else if (!_isAggro && value)
			{
				agent.destination = transform.position;			//for testing purposes, stop wandering
				fsm.pushState(Aggressive);
			}
			_isAggro = value;

		}
	}

	[Tooltip("The radius of the circle from the enemy's starting location in which they'll wander")]
	public int wanderRadius;
	[Tooltip("How close the enemy will get to their wandering position before finding a new position to wander towards.")]
	public float wanderDistBuffer;
	private Vector3 startLocation;
	private NavMeshAgent agent;
	private FSM fsm;


	void Awake () {
		body = GetComponent<CapsuleCollider>();
		agent = GetComponent<NavMeshAgent>();
		fsm = new FSM();
	}

	void Start () {
		startLocation = transform.position;
		fsm.pushState(Wander);
	}
	
	void Update () {
		fsm.Update();
	}

	void OnCollisionEnter(Collision collision)
	{
		if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Tether"))
		{
			Debug.Log("Tether Collide, damage player");
		}
	}

	public void Death()
	{
		Debug.Log ("Enemy Death called!");
		Renderer rend = GetComponent<Renderer>();
		if(rend.material.color == Color.red)
		{
			rend.material.color = Color.grey;
		}
		else
		{
			rend.material.color = Color.red;
		}
	}


	#region States
	void Wander()
	{
		if(Vector3.Distance(transform.position, agent.destination) <= wanderDistBuffer)
		{
			bool foundPathable = false;
			Vector3 wanderTarget = new Vector3();
			while(!foundPathable)
			{
				Vector2 randPos = Random.insideUnitCircle * wanderRadius;
				NavMeshPath path = new NavMeshPath();
				wanderTarget = startLocation + (new Vector3(randPos.x, agent.destination.y, randPos.y));
				agent.CalculatePath(wanderTarget, path);
				//Debug.Log(path.status.ToString());
				if(path.status == NavMeshPathStatus.PathComplete)
				{
					foundPathable = true;
				}
			}

			agent.SetDestination(wanderTarget);
		}
	}

	void Aggressive()
	{
		//try and cut the player's tether
	}
	#endregion
}
