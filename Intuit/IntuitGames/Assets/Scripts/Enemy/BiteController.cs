using UnityEngine;
using System.Collections;
using System;

public class BiteController : Trigger {

    public Animator animatorComp;
    private bool triggerEnabled = false;

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
        StartCoroutine(BiteEffect(duration, hang));
    }

    IEnumerator BiteEffect(float duration, float hang)
    {
        //if (beginBite) animatorComp.gameObject.SetActive(beginBite);
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
        // After this didnt work out too well, decided to just do it manually. Running a coroutine from within a coroutine didnt work reliably
        //if (beginBite)
        //{
        //    yield return new WaitForSeconds(hang);
        //    StartCoroutine(BiteEffect(false, 0f, duration, hang)); // apply the same effect in reverse 
        //}
        //else animatorComp.gameObject.SetActive(beginBite);

    }
}
