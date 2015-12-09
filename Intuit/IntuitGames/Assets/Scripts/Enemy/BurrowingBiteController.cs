using UnityEngine;
using System.Collections;
using System;

public class BurrowingBiteController : Trigger {

    public Animator animatorComp;

    private bool triggerEnabled = false;

    protected override void OnTrigger(GameObject triggerObject)
    {
        if(triggerEnabled)
        {
            TetherManager.TetherManager.Disconnect(TetherManager.TetherManager.jointCount / 2);
            hasBeenTriggered = false;
        }
    }

    public void Bite()
    {
        triggerEnabled = true;
        this.animatorComp.SetBool("biting", true);
    }

    public void StopBite()
    {
        triggerEnabled = false;
        this.animatorComp.SetBool("biting", false);
    }
}
