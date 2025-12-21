using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using R3;
using Runtime.Localization;

namespace Tests.EditMode.Localization
{
    [Category("Unit")]
    public class LocalizationServiceTests
    {
        private LocalizationService _service;
        private ILocalizationStore _mockStore;
        private ILocalizationLoader _mockLoader;
        private JsonLocalizationParser _parser; // Use real parser instead of mock
        private ILocalizationCatalog _mockCatalog;
        private ILocalizationPolicy _mockPolicy;
        private ITextFormatter _mockFormatter;
        private ILocaleStorage _mockStorage;

        private LocaleId _enUs;
        private LocaleId _ruRu;
        private TextTableId _uiTable;
        private TextTableId _gameplayTable;
        private TextKey _testKey;

        [SetUp]
        public void Setup()
        {
            _enUs = new LocaleId("en-US");
            _ruRu = new LocaleId("ru-RU");
            _uiTable = new TextTableId("UI");
            _gameplayTable = new TextTableId("Gameplay");
            _testKey = new TextKey("Test.Key");

            _mockStore = Substitute.For<ILocalizationStore>();
            _mockLoader = Substitute.For<ILocalizationLoader>();
            _parser = new JsonLocalizationParser(); // Real parser
            _mockCatalog = Substitute.For<ILocalizationCatalog>();
            _mockPolicy = Substitute.For<ILocalizationPolicy>();
            _mockFormatter = Substitute.For<ITextFormatter>();
            _mockStorage = Substitute.For<ILocaleStorage>();

            _mockPolicy.DefaultLocale.Returns(_enUs);
            _mockPolicy.UseMissingKeyPlaceholders.Returns(true);
            _mockCatalog.GetSupportedLocales().Returns(new[] { _enUs, _ruRu });
            _mockCatalog.GetStartupTables().Returns(new[] { _uiTable });
            _mockCatalog.GetAssetKey(Arg.Any<LocaleId>(), Arg.Any<TextTableId>()).Returns("mock-asset-key");
            _mockStorage.LoadAsync().Returns(UniTask.FromResult<LocaleId?>(null));

            _service = new LocalizationService(
                _mockStore,
                _mockLoader,
                _parser, // Use real parser
                _mockCatalog,
                _mockPolicy,
                _mockFormatter,
                _mockStorage);
        }

        [TearDown]
        public void TearDown() => _service?.Dispose();

        [Test]
        public void WhenInitializeAsync_ThenLoadsStartupTables()
        {
            // Arrange
            const string json = @"{""locale"":""en-US"",""table"":""UI"",""entries"":{""Test.Key"":""Test Value""}}";
            var bytes = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));

            _mockLoader.LoadBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(bytes));

            // Act
            var task = _service.InitializeAsync(CancellationToken.None);
            task.GetAwaiter().GetResult();

            // Assert
            _mockLoader.Received(1).LoadBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _mockStore.Received(1).Put(Arg.Any<LocalizationTable>());
            _mockStore.Received(1).SetActiveLocale(_enUs);
        }

        [Test]
        public void WhenInitializeAsyncWithSavedLocale_ThenRestoresLocale()
        {
            // Arrange
            _mockStorage.LoadAsync().Returns(UniTask.FromResult<LocaleId?>(_ruRu));

            const string json = @"{""locale"":""ru-RU"",""table"":""UI"",""entries"":{""Test.Key"":""Test Value""}}";
            var bytes = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));

            _mockLoader.LoadBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(bytes));

            // Act
            var task = _service.InitializeAsync(CancellationToken.None);
            task.GetAwaiter().GetResult();

            // Assert
            _mockStore.Received(1).SetActiveLocale(_ruRu);
            _service.CurrentLocale.CurrentValue.Should().Be(_ruRu);
        }

        [Test]
        public void WhenInitializeAsyncWithUnsupportedSavedLocale_ThenUsesDefault()
        {
            // Arrange
            var unsupportedLocale = new LocaleId("xx");
            _mockStorage.LoadAsync().Returns(UniTask.FromResult<LocaleId?>(unsupportedLocale));

            const string json = @"{""locale"":""en-US"",""table"":""UI"",""entries"":{""Test.Key"":""Test Value""}}";
            var bytes = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));

            _mockLoader.LoadBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(bytes));

            LocalizationError? capturedError = null;
            _service.Errors.Subscribe(e => capturedError = e);

            // Act
            var task = _service.InitializeAsync(CancellationToken.None);
            task.GetAwaiter().GetResult();

            // Assert
            _mockStore.Received(1).SetActiveLocale(_enUs);
            capturedError.Should().NotBeNull();
            capturedError.Value.Code.Should().Be(LocalizationErrorCode.UnsupportedLocale);
        }

        [Test]
        public void WhenResolve_ThenDelegatesToStoreAndFormatter()
        {
            // Arrange
            InitializeService();

            _mockStore.TryResolveTemplate(_uiTable, _testKey, out Arg.Any<string>())
                .Returns(ci =>
                {
                    ci[2] = "Test Template";
                    return true;
                });
            
            _mockStore.GetActiveLocale().Returns(_enUs);
            _mockFormatter.Format("Test Template", _enUs, null).Returns("Formatted Text");

            // Act
            var result = _service.Resolve(_uiTable, _testKey);

            // Assert
            result.Should().Be("Formatted Text");
            _mockFormatter.Received(1).Format("Test Template", _enUs, null);
        }

        [Test]
        public void WhenResolveWithMissingKey_ThenReturnsPlaceholder()
        {
            // Arrange
            InitializeService();

            _mockStore.TryResolveTemplate(_uiTable, _testKey, out Arg.Any<string>())
                .Returns(false);
            
            _mockStore.GetActiveLocale().Returns(_enUs);

            LocalizationError? capturedError = null;
            _service.Errors.Subscribe(e => capturedError = e);

            // Act
            var result = _service.Resolve(_uiTable, _testKey);

            // Assert
            result.Should().Be($"⟦Missing: {_uiTable.Name}.{_testKey.Value}⟧");
            capturedError.Should().NotBeNull();
            capturedError.Value.Code.Should().Be(LocalizationErrorCode.MissingKey);
        }

        [Test]
        public void WhenResolveWithArgs_ThenFormatsTemplate()
        {
            // Arrange
            InitializeService();

            var args = new Dictionary<string, object> { { "name", "Bob" } };

            _mockStore.TryResolveTemplate(_uiTable, _testKey, out Arg.Any<string>())
                .Returns(ci =>
                {
                    ci[2] = "Hello, {name}!";
                    return true;
                });
            
            _mockStore.GetActiveLocale().Returns(_enUs);
            _mockFormatter.Format("Hello, {name}!", _enUs, args).Returns("Hello, Bob!");

            // Act
            var result = _service.Resolve(_uiTable, _testKey, args);

            // Assert
            result.Should().Be("Hello, Bob!");
        }

        [Test]
        public void WhenSetLocaleAsync_ThenUpdatesCurrentLocale()
        {
            // Arrange
            InitializeService();

            const string json = @"{""locale"":""ru-RU"",""table"":""UI"",""entries"":{""Test.Key"":""Test Value""}}";
            var bytes = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));

            _mockLoader.LoadBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(bytes));

            // Act
            var task = _service.SetLocaleAsync(_ruRu, CancellationToken.None);
            task.GetAwaiter().GetResult();

            // Assert
            _mockStore.Received().SetActiveLocale(_ruRu);
            _service.CurrentLocale.CurrentValue.Should().Be(_ruRu);
        }

        [Test]
        public void WhenSetLocaleAsyncMultipleTimes_ThenCancelsPreviousRequest()
        {
            // Arrange
            InitializeService();

            var tcs1 = new UniTaskCompletionSource<ReadOnlyMemory<byte>>();
            var tcs2 = new UniTaskCompletionSource<ReadOnlyMemory<byte>>();

            var callCount = 0;
            
            _mockLoader.LoadBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    callCount++;
                    return callCount == 1 ? tcs1.Task : tcs2.Task;
                });

            // Act
            var task1 = _service.SetLocaleAsync(_ruRu, CancellationToken.None);
            var task2 = _service.SetLocaleAsync(_enUs, CancellationToken.None);

            // Complete second task
            tcs2.TrySetResult(new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }));
            task2.GetAwaiter().GetResult();

            // Assert - first task should be cancelled, second should succeed
            _service.CurrentLocale.CurrentValue.Should().Be(_enUs);
        }

        [Test]
        public void WhenSetLocaleAsyncWithUnsupportedLocale_ThenEmitsError()
        {
            // Arrange
            InitializeService();

            var unsupportedLocale = new LocaleId("xx");
            LocalizationError? capturedError = null;
            _service.Errors.Subscribe(e => capturedError = e);

            // Act
            var task = _service.SetLocaleAsync(unsupportedLocale, CancellationToken.None);
            task.GetAwaiter().GetResult();

            // Assert
            capturedError.Should().NotBeNull();
            capturedError.Value.Code.Should().Be(LocalizationErrorCode.UnsupportedLocale);
            _service.CurrentLocale.CurrentValue.Should().Be(_enUs); // Should remain unchanged
        }

        [Test]
        public void WhenPreloadAsyncFails_ThenEmitsError()
        {
            // Arrange
            InitializeService();

            _mockLoader.LoadBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns<UniTask<ReadOnlyMemory<byte>>>(_ => throw new Exception("Load failed"));

            LocalizationError? capturedError = null;
            _service.Errors.Subscribe(e => capturedError = e);

            // Act
            var task = _service.PreloadAsync(_ruRu, new[] { _gameplayTable }, CancellationToken.None);
            task.GetAwaiter().GetResult();

            // Assert
            capturedError.Should().NotBeNull();
            capturedError.Value.Code.Should().Be(LocalizationErrorCode.AddressablesLoadFailed);
            capturedError.Value.Message.Should().Contain("Failed to preload");
        }

        [Test]
        public void WhenObserveWithDynamicArgs_ThenUpdatesOnArgChange()
        {
            // Arrange
            InitializeService();

            var argsSubject = new Subject<IReadOnlyDictionary<string, object>>();
            var args1 = new Dictionary<string, object> { { "name", "Alice" } };
            var args2 = new Dictionary<string, object> { { "name", "Bob" } };

            _mockStore.TryResolveTemplate(_uiTable, _testKey, out Arg.Any<string>())
                .Returns(ci =>
                {
                    ci[2] = "Hello, {name}!";
                    return true;
                });
            
            _mockStore.GetActiveLocale().Returns(_enUs);
            _mockFormatter.Format("Hello, {name}!", _enUs, null).Returns("");
            _mockFormatter.Format("Hello, {name}!", _enUs, args1).Returns("Hello, Alice!");
            _mockFormatter.Format("Hello, {name}!", _enUs, args2).Returns("Hello, Bob!");

            var results = new List<string>();

            // Act
            var observable = _service.Observe(_uiTable, _testKey, argsSubject);
            using var subscription = observable.Subscribe(text => results.Add(text));

            argsSubject.OnNext(args1);
            argsSubject.OnNext(args2);

            // Assert
            results.Should().HaveCount(3);
            results[0].Should().Be(""); // Initial emit with null args
            results[1].Should().Be("Hello, Alice!");
            results[2].Should().Be("Hello, Bob!");
        }

        [Test]
        public void WhenCurrentLocaleChanges_ThenObserveUpdates()
        {
            // Arrange
            InitializeService();

            _mockStore.TryResolveTemplate(_uiTable, _testKey, out Arg.Any<string>())
                .Returns(ci =>
                {
                    ci[2] = "Template";
                    return true;
                });
            
            _mockStore.GetActiveLocale().Returns(_enUs, _ruRu);
            _mockFormatter.Format("Template", _enUs, null).Returns("English");
            _mockFormatter.Format("Template", _ruRu, null).Returns("Russian");

            var results = new List<string>();

            // Act
            var observable = _service.Observe(_uiTable, _testKey, (IReadOnlyDictionary<string, object>)null);
            using var subscription = observable.Subscribe(text => results.Add(text));

            // Trigger locale change by calling SetLocaleAsync
            const string json = @"{""locale"":""ru-RU"",""table"":""UI"",""entries"":{""Test.Key"":""Test Value""}}";
            var bytes = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));
            
            _mockLoader.LoadBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(bytes));

            var task = _service.SetLocaleAsync(_ruRu, CancellationToken.None);
            task.GetAwaiter().GetResult();

            // Assert
            results.Should().HaveCount(2);
            results[0].Should().Be("English");
            results[1].Should().Be("Russian");
        }

        private void InitializeService()
        {
            const string json = @"{""locale"":""en-US"",""table"":""UI"",""entries"":{""Test.Key"":""Test Value""}}";
            var bytes = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));

            _mockLoader.LoadBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(bytes));

            var task = _service.InitializeAsync(CancellationToken.None);
            task.GetAwaiter().GetResult();
        }
    }
}
