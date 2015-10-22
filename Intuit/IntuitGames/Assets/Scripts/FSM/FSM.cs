using System.Collections;
using System.Collections.Generic;
using System;

public class FSM<T> {

	T fsmOwner;

	private Stack<FSMState<T>> stack = new Stack<FSMState<T>>();

	public FSM (T owner)
	{
		fsmOwner = owner;
	}

	public FSM(T owner, FSMState<T> startState)
	{
		fsmOwner = owner;

		stack.Push (startState);
	}

	public void Update()
	{
		if(getCurrentState() != null)
		{
			getCurrentState().Update(fsmOwner);
		}
	}

	public FSMState<T> popState()
	{
		getCurrentState ().End (fsmOwner);
		return stack.Pop();
	}

	public void pushState(FSMState<T> state)
	{
        if (stack.Count > 0) getCurrentState().End(fsmOwner);
		stack.Push( state );
		getCurrentState ().Begin (fsmOwner);
	}

	private FSMState<T> getCurrentState()
	{
		return stack.Count > 0 ? stack.Peek() : null;
	}
}
