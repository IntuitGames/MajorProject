using UnityEngine;
using System.Collections;

public class SimpleAnimationControl : MonoBehaviour {

    public Animator animComp;
    public bool playOnAwake;

    void Awake()
    {
        if (animComp == null)
            animComp = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (playOnAwake)
            Play();
    }

    public void Play()
    {
        animComp.speed = 1;
    }

    public void Stop()
    {
        animComp.speed = 0;
    }
}
