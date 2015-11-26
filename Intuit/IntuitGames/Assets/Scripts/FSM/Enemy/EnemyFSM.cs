using System;
using System.Collections.Generic;

public class EnemyFSM<T, U> : FSM<T, U> where T : EnemyCoreState<U>
{
    public EnemyFSM(U owner) : base(owner) { }
    public EnemyFSM(T startState, U owner) : base(startState, owner) { }

    public void SendAggressionChange(bool becomeAggressive)
    {
        getCurrentState().RecieveAggressionChange(fsmOwner, becomeAggressive);
    }
}

