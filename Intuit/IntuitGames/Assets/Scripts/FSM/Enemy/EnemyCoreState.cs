using System;

public abstract class EnemyCoreState<T> : FSMState<T> where T : Enemy
{
    protected EnemyFSM<EnemyCoreState<T>, T> ownerFSM;

    public EnemyCoreState(EnemyFSM<EnemyCoreState<T>, T> owner)
    {
        ownerFSM = owner;
    }

    public override void Begin(T obj)
    {
        if (obj.showStateDebugs) UnityEngine.Debug.Log("<color=#04B404><b>" + this.GetType().ToString() + "</b> on <b>" + obj.gameObject.name + "</b> has begun!</color>");
    }

    public override void End(T obj)
    {
        if (obj.showStateDebugs) UnityEngine.Debug.Log("<color=#FA8258><b>" + this.GetType().ToString() + "</b> on <b>" + obj.gameObject.name + "</b> has ended!</color>");
    }

    public abstract bool RecieveAggressionChange(T owner, bool becomeAggressive);
}

