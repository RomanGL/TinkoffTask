using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace TinkoffTask.Controls
{
    public sealed class CarouselViewItem : ContentControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty ActiveOpacityProperty =
            DependencyProperty.Register(nameof(ActiveOpacity), typeof(float), typeof(CarouselViewItem), new PropertyMetadata(1f));

        public static readonly DependencyProperty InactiveOpacityProperty =
            DependencyProperty.Register(nameof(InactiveOpacity), typeof(float), typeof(CarouselViewItem), new PropertyMetadata(0.3f));

        public static readonly DependencyProperty PointerOverMaskOpacityProperty =
            DependencyProperty.Register(nameof(PointerOverMaskOpacity), typeof(float), typeof(CarouselViewItem), new PropertyMetadata(0.2f));

        public static readonly DependencyProperty PointerPressedMaskOpacityProperty =
            DependencyProperty.Register(nameof(PointerPressedMaskOpacity), typeof(float), typeof(CarouselViewItem), new PropertyMetadata(0.1f));

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(CarouselViewItem), new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

        public static readonly DependencyProperty MaskAnimationDurationProperty =
            DependencyProperty.Register(nameof(MaskAnimationDuration), typeof(TimeSpan), typeof(CarouselViewItem), new PropertyMetadata(TimeSpan.FromMilliseconds(250)));

        #endregion

        private const string RootGridName = "RootGrid";
        private const string MaskRectName = "MaskRect";

        private Compositor _compositor;
        private Visual _rootVisual;
        private Visual _maskRectVisual;

        public CarouselViewItem()
        {
            this.DefaultStyleKey = typeof(CarouselViewItem);
        }

        public float ActiveOpacity
        {
            get => (float)GetValue(ActiveOpacityProperty);
            set => SetValue(ActiveOpacityProperty, value);
        }

        public float InactiveOpacity
        {
            get => (float)GetValue(InactiveOpacityProperty);
            set => SetValue(InactiveOpacityProperty, value);
        }

        public float PointerOverMaskOpacity
        {
            get => (float)GetValue(PointerOverMaskOpacityProperty);
            set => SetValue(PointerOverMaskOpacityProperty, value);
        }

        public float PointerPressedMaskOpacity
        {
            get => (float)GetValue(PointerPressedMaskOpacityProperty);
            set => SetValue(PointerPressedMaskOpacityProperty, value);
        }

        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public TimeSpan MaskAnimationDuration
        {
            get => (TimeSpan)GetValue(MaskAnimationDurationProperty);
            set => SetValue(MaskAnimationDurationProperty, value);
        }

        public void Activate() => AnimateOpacity(_rootVisual, _compositor, ActiveOpacity, AnimationDuration);
        public void Deactivate() => AnimateOpacity(_rootVisual, _compositor, InactiveOpacity, AnimationDuration);

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var rootGrid = GetTemplateChild(RootGridName) as FrameworkElement;
            var maskRect = GetTemplateChild(MaskRectName) as FrameworkElement;

            _rootVisual = ElementCompositionPreview.GetElementVisual(rootGrid);
            _maskRectVisual = ElementCompositionPreview.GetElementVisual(maskRect);
            _maskRectVisual.Opacity = 0f;
            _compositor = _rootVisual.Compositor;
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            AnimateOpacity(_maskRectVisual, _compositor, PointerOverMaskOpacity, MaskAnimationDuration);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            AnimateOpacity(_maskRectVisual, _compositor, 0f, MaskAnimationDuration);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            AnimateOpacity(_maskRectVisual, _compositor, PointerPressedMaskOpacity, MaskAnimationDuration);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            AnimateOpacity(_maskRectVisual, _compositor, 0f, MaskAnimationDuration);
        }

        private static void AnimateOpacity(Visual visual, Compositor compositor, float toOpacity, TimeSpan duration)
        {
            if (visual == null || compositor == null)
            {
                return;
            }

            var animation = compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1f, toOpacity);
            animation.Duration = duration;

            visual.StartAnimation("Opacity", animation);
        }
    }
}
