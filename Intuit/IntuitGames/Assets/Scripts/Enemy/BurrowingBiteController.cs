using UnityEngine;
using System.Collections;
using System;

public class BurrowingBiteController : Trigger {

    public Animator animatorComp;

    private bool isBiting = false;

    protected override bool canBeTriggered
    {
        get
        {
            return !TetherManager.TetherManager.disconnected && isBiting;
        }
    }

    protected override void OnTrigger(GameObject triggerObject)
    {
        if(isBiting)
        {
            TetherManager.TetherManager.Disconnect(TetherManager.TetherManager.jointCount / 2);
        }
    }

    public void Bite()
    {
        isBiting = true;
        this.animatorComp.SetBool("biting", true);
    }

    public void StopBite()
    {
        isBiting = false;
        this.animatorComp.SetBool("biting", false);
    }
}
