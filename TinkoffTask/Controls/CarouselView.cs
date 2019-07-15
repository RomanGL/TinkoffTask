using System;
using System.Collections;
using System.Linq;
using System.Windows.Input;
using TinkoffTask.Extensions;
using Windows.Devices.Input;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace TinkoffTask.Controls
{
    public sealed class CarouselView : Control
    {
        #region Dependency Properties

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(CarouselView), new PropertyMetadata(-1));

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(CarouselView), new PropertyMetadata(300d));

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(CarouselView), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(CarouselView), new PropertyMetadata(default(object), OnItemsSourceChanged));

        public static readonly DependencyProperty IsAutoSwitchEnableProperty =
            DependencyProperty.Register(nameof(IsAutoSwitchEnabled), typeof(bool), typeof(CarouselView), new PropertyMetadata(true, OnIsAutoSwitchEnabledChanged));

        public static readonly DependencyProperty AutoSwitchIntervalProperty =
            DependencyProperty.Register(nameof(AutoSwitchInterval), typeof(TimeSpan), typeof(CarouselView), new PropertyMetadata(TimeSpan.FromSeconds(4), OnAutoSwitchIntervalChanged));

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(CarouselView), new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(CarouselView), new PropertyMetadata(default(ICommand)));

        #endregion

        private const string RootGridName = "RootGrid";
        private const string ItemsPanelName = "ItemsPanel";
        private const string IndicatorRectName = "IndicatorRect";
        private const string IndexListBoxName = "IndexListBox";

        private const string OffsetX_AnimationPropertyName = "Offset.X";
        private const int CarouselViewItemsCount = 5;

        private Compositor _touchAreaCompositor;
        private Visual _touchAreaVisual;
        private Visual _indicatorVisual;

        private CarouselViewItem[] _items;
        private Visual[] _itemsVisuals;
        private ExpressionAnimation[] _itemsAnimations;
        private ScalarKeyFrameAnimation _indicatorAnimation;

        private ListBox _indexListBox;
        private Panel _itemsPanel;
        private Panel _rootGrid;
        private Rectangle _indicatorRect;

        private DispatcherTimer _autoSwitchTimer;

        private float _x;
        private int _selectedItemIndex;
        private bool _isAnimationRunning;

        public event EventHandler<CarouselViewItemClickEventArgs> ItemClick;

        public CarouselView()
        {
            this.DefaultStyleKey = typeof(CarouselView);
            _selectedItemIndex = 2;

            _autoSwitchTimer = new DispatcherTimer { Interval = AutoSwitchInterval };
            _autoSwitchTimer.Tick += AutoSwitchTimer_Tick;
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public bool IsAutoSwitchEnabled
        {
            get => (bool)GetValue(IsAutoSwitchEnableProperty);
            set => SetValue(IsAutoSwitchEnableProperty, value);
        }

        public TimeSpan AutoSwitchInterval
        {
            get => (TimeSpan)GetValue(AutoSwitchIntervalProperty);
            set => SetValue(AutoSwitchIntervalProperty, value);
        }

        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rootGrid = GetTemplateChild(RootGridName) as Panel;
            _itemsPanel = GetTemplateChild(ItemsPanelName) as Panel;
            _indicatorRect = GetTemplateChild(IndicatorRectName) as Rectangle;
            _indexListBox = GetTemplateChild(IndexListBoxName) as ListBox;

            _indicatorVisual = ElementCompositionPreview.GetElementVisual(_indicatorRect);
            _touchAreaVisual = ElementCompositionPreview.GetElementVisual(_itemsPanel);
            _touchAreaCompositor = _touchAreaVisual.Compositor;

            _items = _itemsPanel.GetDescendantsOfType<CarouselViewItem>().ToArray();
            if (_items.Length != CarouselViewItemsCount)
            {
                throw new ArgumentException("Must be exactly 5 carousel items.");
            }

            _itemsVisuals = new Visual[CarouselViewItemsCount];
            for (int i = 0; i < _items.Length; i++)
            {
                var item = _items[i];
                item.Tapped += CarouselViewItem_Tapped;
                _itemsVisuals[i] = ElementCompositionPreview.GetElementVisual(item);
            }

            _itemsPanel.ManipulationMode = ManipulationModes.TranslateX;

            _itemsPanel.ManipulationStarted += Canvas_ManipulationStarted;
            _itemsPanel.ManipulationDelta += Canvas_ManipulationDelta;
            _itemsPanel.ManipulationCompleted += Canvas_ManipulationCompleted;
            _itemsPanel.PointerWheelChanged += Canvas_PointerWheelChanged;

            _rootGrid.SizeChanged += RootGrid_SizeChanged;
            this.Loaded += CarouselView_Loaded;

            if (IsAutoSwitchEnabled)
            {
                StartAutoSwitchTimer();
            }
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MeasureItemsPosition(_selectedItemIndex);

            var clipRect = RectHelper.FromCoordinatesAndDimensions(0, 0, (float)e.NewSize.Width, (float)e.NewSize.Height);
            _itemsPanel.Clip = new RectangleGeometry { Rect = clipRect };
        }

        private void CarouselView_Loaded(object sender, RoutedEventArgs e)
        {
            SetItemsDataSource();
            SetSelectedAppearance();
        }

        private void CarouselViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int index = _items.IndexOf(sender as CarouselViewItem);
            int diff = Math.Abs(index - _selectedItemIndex);

            if (diff > 1)
            {
                return;
            }

            if (index >= GetIndexUpperBound(_selectedItemIndex + 1, _items.Length))
            {
                GoToNext();
            }
            else if (index <= GetIndexLowerBound(_selectedItemIndex - 1, _items.Length))
            {
                GoToPrevious();
            }
            else
            {
                var dataContext = (sender as CarouselViewItem)?.DataContext;
                if (dataContext != null)
                {
                    ItemClick?.Invoke(this, new CarouselViewItemClickEventArgs(dataContext));
                    if (Command != null && Command.CanExecute(dataContext))
                    {
                        Command.Execute(dataContext);
                    }
                }
            }
        }

        private void PrepareAnimations()
        {
            _itemsAnimations = new ExpressionAnimation[_itemsVisuals.Length];

            for (int i = 0; i < _itemsVisuals.Length; i++)
            {
                float offsetX = _itemsVisuals[i].Offset.X;

                var animation = _touchAreaCompositor.CreateExpressionAnimation("touch.Offset.X + self");
                animation.SetScalarParameter("self", offsetX);
                animation.SetReferenceParameter("touch", _indicatorVisual);

                _itemsAnimations[i] = animation;
            }
        }

        private void SetItemsDataSource()
        {
            var itemsSource = ItemsSource as IList;
            if (itemsSource == null || _items == null || itemsSource.Count == 0 || _indexListBox == null || _indexListBox.ItemsSource == null)
                return;

            if (SelectedIndex < 0)
            {
                SelectedIndex = 0;
            }

            // ItemsSource indexes
            int sourceIndex0 = 0;
            int sourceIndex1 = 0;
            int sourceIndex2 = 0;
            int sourceIndex3 = 0;
            int sourceIndex4 = 0;

            if (itemsSource.Count > 1)
            {
                sourceIndex0 = GetIndexLowerBound(SelectedIndex - 2, itemsSource.Count);
                sourceIndex1 = GetIndexLowerBound(SelectedIndex - 1, itemsSource.Count);
                sourceIndex2 = SelectedIndex;
                sourceIndex3 = GetIndexUpperBound(SelectedIndex + 1, itemsSource.Count);
                sourceIndex4 = GetIndexUpperBound(SelectedIndex + 2, itemsSource.Count);
            }

            // UI elements indexes
            int elementIndex0 = GetIndexLowerBound(_selectedItemIndex - 2, _items.Length);
            int elementIndex1 = GetIndexLowerBound(_selectedItemIndex - 1, _items.Length);
            int elementIndex2 = _selectedItemIndex;
            int elementIndex3 = GetIndexUpperBound(_selectedItemIndex + 1, _items.Length);
            int elementIndex4 = GetIndexUpperBound(_selectedItemIndex + 2, _items.Length);

            _items[elementIndex0].DataContext = itemsSource[sourceIndex0];
            _items[elementIndex1].DataContext = itemsSource[sourceIndex1];
            _items[elementIndex2].DataContext = itemsSource[sourceIndex2];
            _items[elementIndex3].DataContext = itemsSource[sourceIndex3];
            _items[elementIndex4].DataContext = itemsSource[sourceIndex4];
            _indexListBox.SelectedIndex = SelectedIndex;
        }

        private void Canvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            StopAutoSwitchTimer();

            StopAnimations();
            ResetIndicatorPosition();

            PrepareAnimations();
            StartAnimations();
        }

        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Mouse)
            {
                _x += (float)e.Delta.Translation.X;
                _indicatorVisual.SetOffsetX(_x);
            }
        }

        private void Canvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == PointerDeviceType.Mouse)
            {
                return;
            }

            var itemsSource = ItemsSource as IList;
            double itemWidth = _items[0].ActualWidth;
            double threshold = itemWidth / 8;

            int oldSelectedIndex = _selectedItemIndex;

            var cha = _indicatorVisual.Offset.X;
            if (cha <= -threshold)
            {
                _selectedItemIndex = GetIndexUpperBound(_selectedItemIndex + 1, _items.Length);
                SelectedIndex = GetIndexUpperBound(SelectedIndex + 1, itemsSource.Count);
            }
            if (cha >= threshold)
            {
                _selectedItemIndex = GetIndexLowerBound(_selectedItemIndex - 1, _items.Length);
                SelectedIndex = GetIndexLowerBound(SelectedIndex - 1, _items.Length);
            }

            MeasureItemsPosition(_selectedItemIndex, oldSelectedIndex);
            if (IsAutoSwitchEnabled)
            {
                StartAutoSwitchTimer();
            }
        }

        private void MeasureItemsPosition(int index, int oldIndex = -1)
        {
            if (_itemsPanel == null)
            {
                return;
            }

            double containerWidth = _itemsPanel.ActualWidth;
            if (containerWidth < ItemWidth)
            {
                foreach (var item in _items)
                {
                    item.Width = containerWidth;
                }
            }
            else if (_items[0].ActualWidth < ItemWidth)
            {
                foreach (var item in _items)
                {
                    item.Width = ItemWidth;
                }
            }

            double itemWidth = _items[0].Width;
            double cLeft = (containerWidth - itemWidth) / 2;
            double lLeft = -(itemWidth - cLeft);
            double rLeft = containerWidth - cLeft;

            int index0 = GetIndexLowerBound(index - 2, _items.Length);
            int index1 = GetIndexLowerBound(index - 1, _items.Length);
            int index2 = index;
            int index3 = GetIndexUpperBound(index + 1, _items.Length);
            int index4 = GetIndexUpperBound(index + 2, _items.Length);

            if (oldIndex == -1)
            {
                // Reset items position
                _itemsVisuals[index0].SetOffsetX((float)(lLeft - itemWidth));
                _itemsVisuals[index1].SetOffsetX((float)lLeft);
                _itemsVisuals[index2].SetOffsetX((float)cLeft);
                _itemsVisuals[index3].SetOffsetX((float)rLeft);
                _itemsVisuals[index4].SetOffsetX((float)(rLeft + itemWidth));

                // Because animation is not needed
                return;
            }

            _indicatorAnimation = _touchAreaCompositor.CreateScalarKeyFrameAnimation();
            _indicatorAnimation.Duration = AnimationDuration;

            int diff = index - oldIndex;
            // New selected item equals to current item
            if (diff == 0)
            {
                _indicatorAnimation.InsertKeyFrame(1f, 0f);
            }
            // New selected item is the right item of current item
            if (diff == 1 || diff < -1)
            {
                _indicatorAnimation.InsertKeyFrame(1f, (float)-itemWidth);
            }
            // New selected item is the left one of current item
            if (diff == -1 || diff > 1)
            {
                _indicatorAnimation.InsertKeyFrame(1f, (float)itemWidth);
            }

            _isAnimationRunning = true;

            var backScopedBatch = _touchAreaCompositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _indicatorVisual.StartAnimation(OffsetX_AnimationPropertyName, _indicatorAnimation);

            SetSelectedAppearance();

            backScopedBatch.End();
            backScopedBatch.Completed += (ss, ee) =>
            {
                // Reset items position
                _itemsVisuals[index0].SetOffsetX((float)(lLeft - itemWidth));
                _itemsVisuals[index1].SetOffsetX((float)lLeft);
                _itemsVisuals[index2].SetOffsetX((float)cLeft);
                _itemsVisuals[index3].SetOffsetX((float)rLeft);
                _itemsVisuals[index4].SetOffsetX((float)(rLeft + itemWidth));

                SetItemsDataSource();
                _isAnimationRunning = false;
            };

            if (IsAutoSwitchEnabled)
            {
                StartAutoSwitchTimer();
            }
            else
            {
                StopAutoSwitchTimer();
            }
        }

        private void Canvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            if (_isAnimationRunning)
            {
                return;
            }

            var mouseWheelDelta = e.GetCurrentPoint(_itemsPanel).Properties.MouseWheelDelta;
            if (mouseWheelDelta > 0)
            {
                GoToPrevious();
            }
            else if (mouseWheelDelta < 0)
            {
                GoToNext();
            }
        }

        private void AutoSwitchTimer_Tick(object sender, object e)
        {
            GoToNext();
        }

        private void GoToNext()
        {
            var itemsSource = ItemsSource as IList;
            if (itemsSource == null || itemsSource.Count == 0)
            {
                return;
            }

            StopAnimations();
            ResetIndicatorPosition();

            PrepareAnimations();
            StartAnimations();

            int oldItemIndex = _selectedItemIndex;

            _selectedItemIndex = GetIndexUpperBound(_selectedItemIndex + 1, _items.Length);
            SelectedIndex = GetIndexUpperBound(SelectedIndex + 1, itemsSource.Count);

            MeasureItemsPosition(_selectedItemIndex, oldItemIndex);
        }

        private void GoToPrevious()
        {
            var itemsSource = ItemsSource as IList;
            if (itemsSource == null || itemsSource.Count == 0)
            {
                return;
            }

            StopAnimations();
            ResetIndicatorPosition();

            PrepareAnimations();
            StartAnimations();

            int oldItemIndex = _selectedItemIndex;

            _selectedItemIndex = GetIndexLowerBound(_selectedItemIndex - 1, _items.Length);
            SelectedIndex = GetIndexLowerBound(SelectedIndex - 1, itemsSource.Count);

            MeasureItemsPosition(_selectedItemIndex, oldItemIndex);
        }

        private void ResetIndicatorPosition()
        {
            _x = 0.0f;
            _indicatorVisual.SetOffsetX(_x);
        }

        private void StartAnimations()
        {
            for (int i = 0; i < _itemsVisuals.Length; i++)
            {
                _itemsVisuals[i].StartAnimation(OffsetX_AnimationPropertyName, _itemsAnimations[i]);
            }
        }

        private void StopAnimations()
        {
            for (int i = 0; i < _itemsVisuals.Length; i++)
            {
                _itemsVisuals[i].StopAnimation(OffsetX_AnimationPropertyName);
            }
        }

        private void SetSelectedAppearance()
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == _selectedItemIndex)
                {
                    _items[i].Activate();
                }
                else
                {
                    _items[i].Deactivate();
                }
            }
        }

        private void StartAutoSwitchTimer() => _autoSwitchTimer.Start();
        private void StopAutoSwitchTimer() => _autoSwitchTimer.Stop();

        private static int GetIndexLowerBound(int index, int count) => index < 0 ? index + count : index;
        private static int GetIndexUpperBound(int index, int count) => index >= count ? index - count : index;

        private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var carousel = obj as CarouselView;
                carousel?.SetItemsDataSource();
                carousel?.MeasureItemsPosition(carousel._selectedItemIndex, carousel._selectedItemIndex);
            }
        }

        private static void OnIsAutoSwitchEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var carousel = obj as CarouselView;
                if ((bool)e.NewValue)
                {
                    carousel?.StartAutoSwitchTimer();
                }
                else
                {
                    carousel?.StopAutoSwitchTimer();
                }
            }
        }

        private static void OnAutoSwitchIntervalChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var carousel = obj as CarouselView;
                carousel.StopAutoSwitchTimer();
                carousel._autoSwitchTimer.Interval = (TimeSpan)e.NewValue;

                if (carousel.IsAutoSwitchEnabled)
                {
                    carousel?.StartAutoSwitchTimer();
                }
            }
        }
    }
}