using R3;
using Runtime.UI.Core;

namespace Runtime.GameModes.Wizard
{
    public sealed class UltimateSettingsViewModel : BaseViewModel, ISpecificModeSettingsViewModel
    {
        private readonly ReactiveProperty<IGameModeConfig> _config = new(new UltimateModeConfig());
        private readonly ReactiveProperty<bool> _isValid = new(true);

        public ReadOnlyReactiveProperty<IGameModeConfig> Config => _config;
        public ReadOnlyReactiveProperty<bool> IsValid => _isValid;

        protected override void OnDispose()
        {
            _config.Dispose();
            _isValid.Dispose();
            base.OnDispose();
        }
    }
}
