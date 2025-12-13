using System;
using System.Collections;
using FluentAssertions;
using NUnit.Framework;
using R3;
using Runtime.UI.MainMenu;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Tests.PlayMode
{
    [TestFixture]
    public class MainMenuViewTests
    {
        private GameObject _gameObject;
        private UIDocument _uiDocument;
        private MainMenuView _view;
        private MainMenuViewModel _viewModel;
        private VisualTreeAsset _mainMenuUxml;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _mainMenuUxml = Resources.Load<VisualTreeAsset>("MainMenuTest");

            if (_mainMenuUxml == null)
                throw new InvalidOperationException("MainMenuTest.uxml not found under Assets/Scripts/Tests/Resources");
        }

        [UnitySetUp]
        public IEnumerator Setup()
        {
            _gameObject = new GameObject("MainMenuViewTestObject");
            _uiDocument = _gameObject.AddComponent<UIDocument>();
            _uiDocument.visualTreeAsset = _mainMenuUxml;
            _view = _gameObject.AddComponent<MainMenuView>();
            _viewModel = new MainMenuViewModel();

            yield return null;

            var root = _uiDocument.rootVisualElement;
            Assert.IsNotNull(root.Q<Label>("Title"), "UXML должен содержать Label с name='Title'");
            Assert.IsNotNull(root.Q<Button>("StartButton"), "UXML должен содержать Button с name='StartButton'");
            Assert.IsNotNull(root.Q<Button>("ExitButton"), "UXML должен содержать Button с name='ExitButton'");

            _view.SetViewModel(_viewModel);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _viewModel?.Dispose();

            if (_gameObject != null)
                Object.Destroy(_gameObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator WhenSetViewModel_ThenUIElementsUpdateWithViewModelData()
        {
            GetTitleLabel().text.Should().Be("Ultimate Tic-Tac-Toe");
            GetStartButton().text.Should().Be("Start Game");
            GetExitButton().text.Should().Be("Exit");

            yield return null;
        }

        [UnityTest]
        public IEnumerator WhenViewModelIsInteractableChanges_ThenButtonsEnabledStateUpdates()
        {
            var startButton = GetStartButton();
            var exitButton = GetExitButton();

            startButton.enabledSelf.Should().BeTrue();
            exitButton.enabledSelf.Should().BeTrue();

            _viewModel.SetInteractable(false);
            yield return new WaitForEndOfFrame();

            startButton.enabledSelf.Should().BeFalse();
            exitButton.enabledSelf.Should().BeFalse();
        }

        [UnityTest]
        public IEnumerator WhenStartButtonClicked_ThenViewModelStartGameRequestedTriggered()
        {
            var triggered = false;
            var disposable = _viewModel.StartGameRequested.Subscribe(_ => triggered = true);

            _view.OnStartButtonClicked();

            yield return null;

            disposable.Dispose();

            triggered.Should().BeTrue();
        }

        [UnityTest]
        public IEnumerator WhenExitButtonClicked_ThenViewModelExitRequestedTriggered()
        {
            var triggered = false;
            var disposable = _viewModel.ExitRequested.Subscribe(_ => triggered = true);

            _view.OnExitButtonClicked();

            yield return null;

            disposable.Dispose();

            triggered.Should().BeTrue();
        }

        private Label GetTitleLabel() => _uiDocument.rootVisualElement.Q<Label>("Title");
        private Button GetStartButton() => _uiDocument.rootVisualElement.Q<Button>("StartButton");
        private Button GetExitButton() => _uiDocument.rootVisualElement.Q<Button>("ExitButton");
    }
}