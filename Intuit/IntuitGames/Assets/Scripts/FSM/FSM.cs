using System.Collections;
using System.Collections.Generic;

// When inheriting from FSM, T is intended to be the type of State being used
// Ideally, would be defined as something along the lines of:
//      where T:FSMState
// 
public abstract class FSM<T, U> {
    protected U fsmOwner;
    protected Stack<T> stack = new Stack<T>();

    public FSM (U owner)
    {
        fsmOwner = owner;
    }

    public FSM(T startState, U owner)
    {
        fsmOwner = owner;
        stack.Push(startState);
    }

    public abstract void Update();
    public abstract void popState();
    public abstract void pushState(T state);
    protected abstract T getCurrentState();
}
