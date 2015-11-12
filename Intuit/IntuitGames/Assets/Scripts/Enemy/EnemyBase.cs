using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    public EnemyBodyPart[] bodyParts;
    public EnemyWeakSpot[] weakSpots;
    public EnemyAggro aggroHandler;

    [Header("Death")]
    public Rigidbody[] BodyParts;
    public float gibForce;
	public float fadeTime;

	public void Death()
	{

	}


}

