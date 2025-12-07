using Runtime.UI.Core;

namespace Tests.PlayMode.Fakes
{
    public class TestViewModel : BaseViewModel
    {
        public bool InitializeCalled { get; private set; }
        public int InitializeCallCount { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            InitializeCalled = true;
            InitializeCallCount++;
        }

        public override void Reset()
        {
            base.Reset();
            InitializeCalled = false;
            InitializeCallCount = 0;
        }
    }
}

