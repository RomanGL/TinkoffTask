using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TinkoffTask.Controls
{
    [TemplateVisualState(GroupName = CommonStateGroupName, Name = NormalStateName)]
    [TemplateVisualState(GroupName = CommonStateGroupName, Name = LoadingStateName)]
    [TemplateVisualState(GroupName = CommonStateGroupName, Name = ErrorStateName)]
    [TemplateVisualState(GroupName = CommonStateGroupName, Name = NoDataStateName)]
    [TemplatePart(Name = ReloadButtonName, Type = typeof(Button))]
    public class ContentContainer : ContentControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State), typeof(ContentState), typeof(ContentContainer), new PropertyMetadata(ContentState.None, OnStateChanged));

        public static readonly DependencyProperty ReloadCommandProperty =
            DependencyProperty.Register(nameof(ReloadCommand), typeof(ICommand), typeof(ContentContainer), new PropertyMetadata(null));

        public static readonly DependencyProperty ReloadCommandParameterProperty =
            DependencyProperty.Register(nameof(ReloadCommandParameter), typeof(object), typeof(ContentContainer), new PropertyMetadata(null));

        public static readonly DependencyProperty ReloadButtonTextProperty =
            DependencyProperty.Register(nameof(ReloadButtonText), typeof(string), typeof(ContentContainer), new PropertyMetadata("Reload"));

        public static readonly DependencyProperty LoadingTextProperty =
            DependencyProperty.Register(nameof(LoadingText), typeof(string), typeof(ContentContainer), new PropertyMetadata("Loading"));

        public static readonly DependencyProperty ErrorTextProperty =
            DependencyProperty.Register(nameof(ErrorText), typeof(string), typeof(ContentContainer), new PropertyMetadata("Error"));

        public static readonly DependencyProperty NoDataTextProperty =
            DependencyProperty.Register(nameof(NoDataText), typeof(string), typeof(ContentContainer), new PropertyMetadata("Empty"));

        #endregion

        private const string CommonStateGroupName = "CommonStates";        
        private const string NormalStateName = "NormalState";
        private const string LoadingStateName = "LoadingState";
        private const string NoDataStateName = "NoDataState";
        private const string ErrorStateName = "ErrorState";
        private const string ReloadButtonName = "ReloadButton";

        private Button _reloadButton;

        public event EventHandler<RoutedEventArgs> ReloadButtonClick;

        public ContentContainer()
        {
            this.DefaultStyleKey = typeof(ContentContainer);
        }
                
        public ContentState State
        {
            get => (ContentState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public ICommand ReloadCommand
        {
            get => (ICommand)GetValue(ReloadCommandProperty);
            set => SetValue(ReloadCommandProperty, value);
        }

        public object ReloadCommandParameter
        {
            get => GetValue(ReloadCommandParameterProperty);
            set => SetValue(ReloadCommandParameterProperty, value);
        }

        public string ReloadButtonText
        {
            get => (string)GetValue(ReloadButtonTextProperty);
            set => SetValue(ReloadButtonTextProperty, value);
        }

        public string LoadingText
        {
            get => (string)GetValue(LoadingTextProperty);
            set => SetValue(LoadingTextProperty, value);
        }

        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        public string NoDataText
        {
            get => (string)GetValue(NoDataTextProperty);
            set => SetValue(NoDataTextProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualState();

            _reloadButton = GetTemplateChild(ReloadButtonName) as Button;
            if (_reloadButton == null) return;

            _reloadButton.Click += (s, e) =>
            {
                ReloadButtonClick?.Invoke(this, e);

                if (ReloadCommand != null && ReloadCommand.CanExecute(ReloadCommandParameter))
                {
                    ReloadCommand.Execute(ReloadCommandParameter);
                }
            };
        }

        private void UpdateVisualState()
        {
            string stateName = NormalStateName;
            switch (State)
            {
                case ContentState.Loading:
                    stateName = LoadingStateName;
                    break;
                case ContentState.Error:
                    stateName = ErrorStateName;
                    break;
                case ContentState.NoData:
                    stateName = NoDataStateName;
                    break;
            }

            VisualStateManager.GoToState(this, stateName, true);
        }

        private static void OnStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((ContentContainer)obj).UpdateVisualState();
        }
    }
}
