using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    //   public EnemyBodyPart[] bodyParts;
    //   public EnemyWeakSpot[] weakSpots;
    //   public EnemyAggro aggroHandler;
    //public EnemyBodyBase[] baseParts;
    [Header("Base Components")]
    public Animator animatorComp;
    [Header("Aggressive")]
    public EnemyAggro aggroHandler;
    public float knockbackForce;

    [Header("Death")]
    public float gibForce;
    public float fadeTime;
    [HideInInspector]
    public bool isDead;

    protected virtual void Awake()
    {
        if (animatorComp == null) animatorComp = GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        UpdateAnimator();
    }

    protected virtual void UpdateAnimator()
    {
        
    }

    public void OnDeath()
    {
        EnemyBodyBase[] parts = GetComponentsInChildren<EnemyBodyBase>();
        foreach (EnemyBodyBase part in parts)
        {
            part.Death();
        }
    }

    public abstract void SendAggroMessage(bool becomeAggro);
    
}

