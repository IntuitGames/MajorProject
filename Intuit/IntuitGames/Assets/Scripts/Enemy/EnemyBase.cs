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
    public GameObject riggedModel;
    public GameObject deathModel;
    public ParticleSystem deathParticle;
    public WeakSpot weakSpot;
    public float gibForce;
    public float fadeTime;
    private bool movedModel = false;
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
            if(movedModel)
            {
                riggedModel.SetActive(false);
                deathParticle.Play();
                deathModel.SetActive(true);
                movedModel = false;
            }
            if (animatorComp.gameObject.activeInHierarchy && animatorComp.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                riggedModel.transform.position = Camera.main.transform.position - new Vector3(0, 0, -3);    //Move the model out of view for a frame to call any OnTriggerExits
                movedModel = true;                
            }
        }
    }

    protected virtual void UpdateAnimator()
    {
        if(riggedModel.activeInHierarchy)
        animatorComp.SetBool("dead", isDead);
    }

    public virtual void OnDeath()
    {
        isDead = true;
    }

    public virtual void DestroyMe()
    {
        Destroy(this.gameObject);
    }

    public abstract void SendAggroMessage(bool becomeAggro);
    
}

