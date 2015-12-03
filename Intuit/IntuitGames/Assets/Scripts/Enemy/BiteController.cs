using UnityEngine;
using System.Collections;
using System;

public class BiteController : Trigger {

    public Animator animatorComp;
    private bool triggerEnabled = false;
    private bool coroutineRunning = false;
    private IEnumerator coroutine;

    protected override bool canBeTriggered
    {
        get
        {
            return triggerEnabled;
        }
    }

    protected override void OnTrigger(GameObject triggerObject)
    {
        if (triggerEnabled)     
        {
            TetherManager.TetherManager.Disconnect(TetherManager.TetherManager.jointCount / 2);
            hasBeenTriggered = false;
        }
    }

    public void Bite(float duration, float hang)
    {
        if (!coroutineRunning)
        {
            coroutine = BiteEffect(duration, hang);
            StartCoroutine(coroutine);
        }
    }

    public void StopBiteEffect()
    {
        if (coroutineRunning)
        {
            StopCoroutine(coroutine);
            triggerEnabled = false;
        }
    }

    IEnumerator BiteEffect(float duration, float hang)
    {
        coroutineRunning = true;
        SpriteRenderer renderer = animatorComp.GetComponent<SpriteRenderer>();
        Color start = renderer.color;
        Color end = start;
        end.a = 0.9f;
        for (float t = 0; t < duration; t+=Time.deltaTime)
        {
            renderer.color = Color.Lerp(start, end, t / duration);
            yield return null;
        }
        renderer.color = end;
        animatorComp.SetBool("startBite", true);
        triggerEnabled = true;
        yield return new WaitForSeconds(hang);  //After holding the bite trigger there for a time, reverse the proecess
        triggerEnabled = false;        
        start = end;
        end.a = 0;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            renderer.color = Color.Lerp(start, end, t / duration);
            yield return null;
        }
        renderer.color = end;
        animatorComp.SetBool("startBite", false);
        coroutineRunning = false;
    }
}
