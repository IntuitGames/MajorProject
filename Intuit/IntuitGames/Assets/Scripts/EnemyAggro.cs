using UnityEngine;
using System.Collections;

public class EnemyAggro : MonoBehaviour {

	int playerObsInRange;
	Enemy enemy;
	
	// Use this for initialization
	void Start () {
		enemy = transform.parent.gameObject.GetComponent<Enemy>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Whenever a player or tether piece enters the aggro trigger, set the enemy to the aggro state and add to the count of how many tethers/players have entered 
	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == LayerMask.NameToLayer("Player") || other.gameObject.layer == LayerMask.NameToLayer("Tether"))
		{
			enemy.isAggro = true;
			playerObsInRange++;
		}
	}

	//Each time a player/tether objects leaves the aggro range, subtract from the number of them that have entered.
	//This allows the enemy to stay aggro'd to the players as long as theres something within its range
	void OnTriggerExit(Collider other)
	{
		if(other.gameObject.layer == LayerMask.NameToLayer("Player") || other.gameObject.layer == LayerMask.NameToLayer("Tether"))
		{
			playerObsInRange--;
			if(playerObsInRange <= 0)
			{
				enemy.isAggro = false;
			}
		}
	}
}
