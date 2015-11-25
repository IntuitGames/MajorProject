public abstract class EnemyCoreState<T> : FSMState<T>
{
    protected EnemyFSM<EnemyCoreState<T>, T> ownerFSM;

    public EnemyCoreState(EnemyFSM<EnemyCoreState<T>, T> owner)
    {
        ownerFSM = owner;
    }

    public abstract void RecieveAggressionChange(T owner, bool becomeAggressive);
}

