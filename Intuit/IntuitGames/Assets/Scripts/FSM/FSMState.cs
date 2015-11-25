public abstract class FSMState<T> {
    public abstract void Begin(T obj);
    public abstract void Update(T obj);
    public abstract void End(T obj);
}
