using System;
using System.Collections.Generic;

public class EnemyFSM<T, U> : FSM<T, U> where T : EnemyCoreState<U> where U : Enemy
{
    public EnemyFSM(U owner) : base(owner) { }
    public EnemyFSM(T startState, U owner) : base(startState, owner) { }

    private bool hasAggroMessage = false;
    private bool storedAggroMessage = false;

    public override void popState()
    {
        base.popState();
        if (hasAggroMessage) hasAggroMessage = !getCurrentState().RecieveAggressionChange(fsmOwner, storedAggroMessage);
    }

    public override void pushState(T state)
    {
        base.pushState(state);
        if (hasAggroMessage) hasAggroMessage = !getCurrentState().RecieveAggressionChange(fsmOwner, storedAggroMessage);
    }

    public void SendAggressionChange(bool becomeAggressive)
    {
        storedAggroMessage = becomeAggressive;
        if(getCurrentState().RecieveAggressionChange(fsmOwner, becomeAggressive))
        {
            hasAggroMessage = false;
        }
    }


}

