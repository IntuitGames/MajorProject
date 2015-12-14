﻿using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    public bool showStateDebugs = false;
    [Header("Base Components")]
    public Animator animatorComp;
    public EnemyAudio audioDataComp;
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
            if (animatorComp.gameObject.activeInHierarchy && animatorComp.GetCurrentAnimatorStateInfo(0).IsTag("dead") && animatorComp.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                riggedModel.transform.position = Camera.main.transform.position - Vector3.one * 4; //Apparently this actually does get Exits called on colliders and triggers, yay.
                deadMove = true;
            }
        }
        else UpdateAnimator();
    }

    protected virtual void OnDestroy()
    {
        audioDataComp.Dispose();
    }

    public virtual void DeathEffect()
    {
        riggedModel.SetActive(false);
        deathParticle.Play();
        audioDataComp.PlayDeathAudio();
        if (swapModel)
            deathModel.SetActive(true);
        deadMove = false;
    }

    protected virtual void UpdateAnimator()
    {
            
    }

    public virtual void OnDeath(bool swapModel)
    {
        audioDataComp.PlayVocaliseAudio();
        animatorComp.SetTrigger("dead");
        isDead = true;
        this.swapModel = swapModel;
    }

    public virtual void DestroyMe()
    {
        this.gameObject.SetActive(false);
        audioDataComp.Dispose();
        //Destroy(this.gameObject, 5f);
    }

    public abstract Vector3 GetVelocity();

    public abstract void SendAggroMessage(bool becomeAggro);
    
}

