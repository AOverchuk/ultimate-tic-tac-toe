using System;

namespace Runtime.UI.MainMenu
{
    public interface IMainMenuCoordinator : IDisposable
    {
        void Initialize(MainMenuViewModel viewModel);
    }
}

