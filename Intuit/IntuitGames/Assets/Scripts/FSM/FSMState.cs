using UnityEngine;
using System.Collections;

public abstract class FSMState<T> {

//	private FSM<T> ownerFSM;
//
//	protected FSMState (FSM<T> owner)
//	{
//		ownerFSM = owner;
//	}

	public abstract void Begin(T obj);
	public abstract void Update(T obj);
	public abstract void End(T obj);
}
