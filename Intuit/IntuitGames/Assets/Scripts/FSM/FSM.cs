using System.Collections;
using System.Collections.Generic;
using System;

public class FSM {

	private Stack<Action> stack = new Stack<Action>();

	public void Update()
	{
		if(getCurrentState() != null)
		{
			getCurrentState().Invoke();
		}
	}

	public void popState()
	{
		stack.Pop();
	}

	public void pushState(Action state)
	{
		stack.Push( state );
	}

	private Action getCurrentState()
	{
		return stack.Count > 0 ? stack.Peek() : null;
	}
}
