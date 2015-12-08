using UnityEngine;


public class SoftheadEnemy : WanderingEnemy
{
    private bool stompedFlag = false;
    private float deathY;

    public override void OnDeath(bool swapModel)
    {
        if(stompedFlag)
        {
            animatorComp.SetBool("squished", true);
        }
        base.OnDeath(swapModel);
    }

    public void Stomped()
    {
        stompedFlag = true;
        OnDeath(!stompedFlag);
        deathY = this.riggedModel.transform.localPosition.y;
    }

    protected override void Update()
    {
        if(stompedFlag)
        this.riggedModel.transform.localPosition = new Vector3(this.riggedModel.transform.localPosition.x, Mathf.Lerp(deathY, 0f, animatorComp.GetCurrentAnimatorStateInfo(0).normalizedTime), this.riggedModel.transform.localPosition.z);
        base.Update();
    }
}

