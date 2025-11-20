using VContainer;

namespace Infrastructure.States
{
    public class StateFactory : IStateFactory
    {
        private readonly IObjectResolver _container;

        public StateFactory(IObjectResolver container)
        {
            _container = container;
        }

        public TState CreateState<TState>() where TState : IExitableState
        {
            return _container.Resolve<TState>();
        }
    }
}

