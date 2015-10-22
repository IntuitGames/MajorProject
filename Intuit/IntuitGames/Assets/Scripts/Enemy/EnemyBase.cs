using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    public EnemyBody[] bodyParts;
    public EnemyWeakSpot[] weakSpots;
    public EnemyAggro aggroHandler;

    [Header("Death")]
    public Rigidbody[] BodyParts;
    public float gibForce;
}

