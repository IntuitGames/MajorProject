using System.Collections;
using System.Collections.Generic;

// When inheriting from FSM, T is intended to be the type of State being used
// Ideally, would be defined as something along the lines of:
//      where T:FSMState
// 
public class FSM<T, U> where T : FSMState<U> {
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

    public virtual void Update()
	{
		getCurrentState().Update(fsmOwner);
	}
    public virtual void popState()
	{
		stack.Pop().End( fsmOwner );
		getCurrentState().Begin(fsmOwner);
	}
	public virtual void pushState(T state)
	{
		if (stack.Count > 0) getCurrentState().End(fsmOwner);
		stack.Push(state);
		getCurrentState().Begin(fsmOwner);	

	}
    protected virtual T getCurrentState()
	{
		return stack.Count > 0 ? stack.Peek() : null;
	}
}
