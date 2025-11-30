namespace Runtime.Infrastructure.States
{
    public interface IStateFactory
    {
        TState CreateState<TState>() where TState : IExitableState;
    }
}

