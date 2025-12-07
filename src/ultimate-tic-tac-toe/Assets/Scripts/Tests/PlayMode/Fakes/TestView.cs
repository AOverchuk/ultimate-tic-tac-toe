using System;
using R3;
using Runtime.UI.Core;
using UnityEngine.UIElements;

namespace Tests.PlayMode.Fakes
{
    public class TestView : BaseView<TestViewModel>
    {
        public bool BindViewModelCalled { get; private set; }
        public int BindViewModelCallCount { get; private set; }

        [Runtime.UI.Core.UxmlElement] private Button _testButton;
        [Runtime.UI.Core.UxmlElement] private Label _testLabel;
        
        [Runtime.UI.Core.UxmlElement("TestVisibilityElement")] 
        private VisualElement _testVisibilityElement;

        public Button TestButton => _testButton;
        public Label TestLabel => _testLabel;
        public VisualElement TestVisibilityElement => _testVisibilityElement;

        // Публичные свойства для тестирования protected полей
        public VisualElement PublicRoot => Root;
        public TestViewModel PublicViewModel => ViewModel;

        protected override void BindViewModel()
        {
            BindViewModelCalled = true;
            BindViewModelCallCount++;
        }

        // Публичные методы для тестирования protected методов
        public void TestBindText<T>(Observable<T> source, VisualElement element) => 
            BindText(source, element);

        public void TestBindVisibility(Observable<bool> source, VisualElement element) => 
            BindVisibility(source, element);

        public void TestBindEnabled(Observable<bool> source, VisualElement element) => 
            BindEnabled(source, element);

        public void TestAddDisposable(IDisposable disposable) => 
            AddDisposable(disposable);

        public void ResetTestFlags()
        {
            BindViewModelCalled = false;
            BindViewModelCallCount = 0;
        }
    }
}

