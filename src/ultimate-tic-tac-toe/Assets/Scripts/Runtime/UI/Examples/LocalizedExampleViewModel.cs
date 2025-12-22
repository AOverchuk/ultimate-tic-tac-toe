using System.Collections.Generic;
using R3;
using Runtime.Localization;
using Runtime.UI.Core;

namespace Runtime.UI.Examples
{
    /// <summary>
    /// ⚠️ ПРИМЕР ПАТТЕРНОВ ЛОКАЛИЗАЦИИ - НЕ PRODUCTION ШАБЛОН ⚠️
    /// 
    /// Этот класс демонстрирует корректное использование ILocalizationService.Observe с proper disposal,
    /// НО содержит неоптимальные решения для hot path:
    /// - Dictionary создаётся на каждое изменение реактивного свойства (GC allocation)
    /// - CombineLatest создаёт дополнительные аллокации при объединении стримов
    /// 
    /// ДЛЯ PRODUCTION HOT PATH используйте:
    /// - ObjectPool&lt;Dictionary&gt; для переиспользования словарей
    /// - Struct-based args вместо Dictionary где возможно
    /// - Кэширование форматированных строк при необходимости
    /// 
    /// Этот код корректен для UI (меню, настройки), где производительность не критична.
    /// </summary>
    public sealed class LocalizedExampleViewModel : BaseViewModel
    {
        private readonly ILocalizationService _localization;
        private readonly ReactiveProperty<string> _title = new();
        private readonly ReactiveProperty<string> _playButton = new();
        private readonly ReactiveProperty<string> _scoreText = new();
        private readonly ReactiveProperty<string> _playerTurnText = new();
        private readonly ReactiveProperty<int> _currentScore = new(0);
        private readonly ReactiveProperty<string> _currentPlayerName = new("Player 1");

        public ReadOnlyReactiveProperty<string> Title => _title;
        public ReadOnlyReactiveProperty<string> PlayButton => _playButton;
        public ReadOnlyReactiveProperty<string> ScoreText => _scoreText;
        public ReadOnlyReactiveProperty<string> PlayerTurnText => _playerTurnText;
        public ReadOnlyReactiveProperty<int> CurrentScore => _currentScore;
        public ReadOnlyReactiveProperty<string> CurrentPlayerName => _currentPlayerName;

        public LocalizedExampleViewModel(ILocalizationService localization) =>
            _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));

        public override void Initialize()
        {
            // ✅ Простая локализация без аргументов
            AddDisposable(_localization
                .Observe(TextTableId.UI, "MainMenu.Title")
                .Subscribe(text => _title.Value = text));

            // ✅ Простая локализация без аргументов (альтернативный синтаксис)
            AddDisposable(_localization
                .Observe(new TextTableId("UI"), new TextKey("MainMenu.Play"))
                .Subscribe(text => _playButton.Value = text));

            // ✅ Локализация с реактивными аргументами
            // Текст обновляется при смене локали ИЛИ при изменении CurrentScore
            var scoreArgsObservable = _currentScore
                .Select(score => new Dictionary<string, object> { { "score", score } } as IReadOnlyDictionary<string, object>);

            AddDisposable(_localization
                .Observe(new TextTableId("UI"), new TextKey("Game.Score"), scoreArgsObservable)
                .Subscribe(text => _scoreText.Value = text));

            // ✅ Комбинирование нескольких реактивных параметров
            var playerTurnArgs = Observable.CombineLatest(
                _currentPlayerName,
                _currentScore,
                (playerName, score) => new Dictionary<string, object>
                {
                    { "playerName", playerName },
                    { "score", score }
                } as IReadOnlyDictionary<string, object>
            );

            AddDisposable(_localization
                .Observe(new TextTableId("UI"), new TextKey("Game.Turn.Player"), playerTurnArgs)
                .Subscribe(text => _playerTurnText.Value = text));
        }

        public void UpdateScore(int newScore) => _currentScore.Value = newScore;

        public void UpdatePlayerName(string playerName) => _currentPlayerName.Value = playerName;

        /// <summary>
        /// Пример синхронного резолва (использовать только для non-UI логики).
        /// </summary>
        public string GetLocalizedErrorMessage(string errorCode) =>
            _localization.Resolve(
                new TextTableId("Errors"),
                new TextKey($"Error.{errorCode}"));

        protected override void OnDispose()
        {
            // Не нужно явно dispose-ить подписки - они добавлены через AddDisposable(...)
            // BaseViewModel автоматически вызовет Dispose() для всех зарегистрированных объектов
            _title?.Dispose();
            _playButton?.Dispose();
            _scoreText?.Dispose();
            _playerTurnText?.Dispose();
            _currentScore?.Dispose();
            _currentPlayerName?.Dispose();
        }
    }
}
