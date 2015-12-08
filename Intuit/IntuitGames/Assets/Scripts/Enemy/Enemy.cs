using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    [Header("Base Components")]
    public Animator animatorComp;
    [Header("Aggressive")]
    public EnemyAggro aggroHandler;

    [Header("Death")]
    public GameObject riggedModel;
    public GameObject deathModel;
    public ParticleSystem deathParticle;
    public WeakSpot weakSpot;
    public float gibForce;
    public float fadeTime;
    private bool swapModel;
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
        if(isDead)
        {
            if (animatorComp.gameObject.activeInHierarchy && animatorComp.GetCurrentAnimatorStateInfo(0).IsTag("dead") && animatorComp.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f)
            {                
                riggedModel.SetActive(false);
                deathParticle.Play();
                if (swapModel)
                    deathModel.SetActive(true);
            }
        }
    }

    protected virtual void UpdateAnimator()
    {
            
    }

    public virtual void OnDeath(bool swapModel)
    {
        animatorComp.SetTrigger("dead");
        isDead = true;
        this.swapModel = swapModel;
    }

    public virtual void DestroyMe()
    {
        this.gameObject.SetActive(false);
        //Destroy(this.gameObject, 5f);
    }

    public abstract void SendAggroMessage(bool becomeAggro);
    
}

