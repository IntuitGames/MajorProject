using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    public bool showStateDebugs = false;
    [Header("Base Components")]
    public Animator animatorComp;
    [Header("Aggressive")]
    public EnemyAggro aggroHandler;

    [Header("Death")]
    public GameObject riggedModel;
    public GameObject deathModel;
    public ParticleSystem deathParticle;
    public float gibForce;
    public float fadeTime;
    private bool swapModel;
    private bool deadMove = false;
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
        if(isDead)
        {
            if (deadMove) DeathEffect();            
            if (animatorComp.gameObject.activeInHierarchy && animatorComp.GetCurrentAnimatorStateInfo(0).IsTag("dead") && animatorComp.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f)
            {
                riggedModel.transform.position = Camera.main.transform.position - Vector3.one * 4; //Apparently this actually does get Exits called on colliders and triggers, yay.
                deadMove = true;
            }
        }
        else UpdateAnimator();
    }

    public virtual void DeathEffect()
    {
        riggedModel.SetActive(false);
        deathParticle.Play();
        if (swapModel)
            deathModel.SetActive(true);
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

