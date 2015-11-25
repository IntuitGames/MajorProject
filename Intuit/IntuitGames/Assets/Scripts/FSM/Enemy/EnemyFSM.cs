using System;
using System.Collections.Generic;

public class EnemyFSM<T, U> : FSM<T, U> where T : EnemyCoreState<U>
{
    public EnemyFSM(U owner) : base(owner) { }
    public EnemyFSM(T startState, U owner) : base(startState, owner) { }

    public override void Update()
    {
        getCurrentState().Update(fsmOwner);
    }
    public override void popState()
    {
        stack.Pop().End( fsmOwner );
        getCurrentState().Begin(fsmOwner);
    }
    public override void pushState(T state)
    {
        if (stack.Count > 0) getCurrentState().End(fsmOwner);
        stack.Push(state);
        getCurrentState().Begin(fsmOwner);
    }

    protected override T getCurrentState()
    {
        return stack.Count > 0 ? stack.Peek() : null;
    }

    public void SendAggressionChange(bool becomeAggressive)
    {
        getCurrentState().RecieveAggressionChange(fsmOwner, becomeAggressive);
    }
}

