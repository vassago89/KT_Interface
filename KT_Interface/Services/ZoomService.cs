using KT_Interface.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KT_Interface.Services
{
    public class ZoomService : DependencyObject
    {
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(
                "Scale",
                typeof(double),
                typeof(ZoomService));

        public static readonly DependencyProperty TranslateXProperty =
            DependencyProperty.Register(
                "TranslateX",
                typeof(double),
                typeof(ZoomService));

        public static readonly DependencyProperty TranslateYProperty =
            DependencyProperty.Register(
                "TranslateY",
                typeof(double),
                typeof(ZoomService));

        public static readonly DependencyProperty OverlayThicknessProperty =
            DependencyProperty.Register(
                "OverlayThickness",
                typeof(double),
                typeof(ZoomService));

        public static readonly DependencyProperty FontThicknessProperty =
            DependencyProperty.Register(
                "FontThickness",
                typeof(double),
                typeof(ZoomService));

        public static readonly DependencyProperty FontSmallThicknessProperty =
            DependencyProperty.Register(
                "FontSmallThickness",
                typeof(double),
                typeof(ZoomService));

        const double _defaultOverlayThickness = 2;
        const double _defaultFontThickness = 24;

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set
            {
                SetValue(OverlayThicknessProperty, _defaultOverlayThickness / Math.Abs(value));
                SetValue(FontThicknessProperty, _defaultFontThickness / Math.Abs(value));

                SetValue(ScaleProperty, value);
            }
        }

        public double TranslateX
        {
            get => (double)GetValue(TranslateXProperty);
            set
            {
                SetValue(TranslateXProperty, value);
                System.Diagnostics.Debug.WriteLine($"{value}");
            }
        }

        public double TranslateY
        {
            get => (double)GetValue(TranslateYProperty);
            set
            {
                SetValue(TranslateYProperty, value);
            }
        }

        public double OverlayThickness
        {
            get => (double)GetValue(OverlayThicknessProperty);
            set
            {
                if (value < 1)
                    return;

                if (value > 35791.3940666666)
                    return;

                SetValue(OverlayThicknessProperty, value);
            }
        }

        public double FontThickness
        {
            get => (double)GetValue(FontThicknessProperty);
            set
            {
                if (value < 1)
                    return;

                if (value > 35791.3940666666)
                    return;

                SetValue(FontThicknessProperty, value);
            }
        }

        public ZoomService()
        {
            Scale = 1;
        }

        private void CheckValues(ref double scale, ref double translateX, ref double translateY, ref double overlayThickness, ref double fontThickness)
        {
            if (overlayThickness < 1)
                overlayThickness = 1;
            else if (overlayThickness > 35791.3940666666)
                overlayThickness = 35791.3940666665;

            if (fontThickness < 1)
                fontThickness = 1;
            else if (fontThickness > 35791.3940666666)
                fontThickness = 35791.3940666665;
        }

        private async Task UpdateValue(double scale, double translateX, double translateY, bool isAnimated, int duration)
        {
            if (Dispatcher.CheckAccess())
            {
                var overlayThickness = _defaultOverlayThickness / Math.Abs(scale);
                var fontThickness = _defaultFontThickness / Math.Abs(scale);

                CheckValues(ref scale, ref translateX, ref translateY, ref overlayThickness, ref fontThickness);

                if (isAnimated)
                {
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(duration);

                    Storyboard storyboard = new Storyboard();

                    var scaleAnimation = new DoubleAnimation(scale, timeSpan);
                    Storyboard.SetTarget(scaleAnimation, this);
                    Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleProperty));
                    storyboard.Children.Add(scaleAnimation);

                    var translateXAnimation = new DoubleAnimation(translateX, timeSpan);
                    Storyboard.SetTarget(translateXAnimation, this);
                    Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath(TranslateXProperty));
                    storyboard.Children.Add(translateXAnimation);

                    var translateYAnimation = new DoubleAnimation(translateY, timeSpan);
                    Storyboard.SetTarget(translateYAnimation, this);
                    Storyboard.SetTargetProperty(translateYAnimation, new PropertyPath(TranslateYProperty));
                    storyboard.Children.Add(translateYAnimation);

                    var overlayThicknessAnimation = new DoubleAnimation(overlayThickness, timeSpan);
                    Storyboard.SetTarget(overlayThicknessAnimation, this);
                    Storyboard.SetTargetProperty(overlayThicknessAnimation, new PropertyPath(OverlayThicknessProperty));
                    storyboard.Children.Add(overlayThicknessAnimation);

                    var fontThicknessAnimation = new DoubleAnimation(fontThickness, timeSpan);
                    Storyboard.SetTarget(fontThicknessAnimation, this);
                    Storyboard.SetTargetProperty(fontThicknessAnimation, new PropertyPath(FontThicknessProperty));
                    storyboard.Children.Add(fontThicknessAnimation);

                    await storyboard.BeginAsync();
                }
                else
                {
                    Scale = scale;

                    TranslateX = translateX;
                    TranslateY = translateY;

                    OverlayThickness = overlayThickness;
                    FontThickness = fontThickness;
                }
            }
            else
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await UpdateValue(scale, translateX, translateY, isAnimated, duration);
                });
            }
        }

        public async void ZoomIn(double presentorWidth, double presentorHeight)
        {
            await ExecuteZoom(presentorWidth / 2, presentorHeight / 2, 1.1f);
        }

        public async void ZoomOut(double presentorWidth, double presentorHeight)
        {
            await ExecuteZoom(presentorWidth / 2, presentorHeight / 2, 0.9f);
        }

        public async Task ExecuteZoom(double centerX, double centerY, double zoomFactor, bool isAnimated = false, int duration = 500)
        {
            double prevX = (centerX - TranslateX);
            double prevY = (centerY - TranslateY);

            var translateX = TranslateX + prevX - (prevX * zoomFactor);
            var translateY = TranslateY + prevY - (prevY * zoomFactor);

            var scale = Scale * zoomFactor;

            await UpdateValue(scale, translateX, translateY, isAnimated, duration);
        }

        //public async Task ExecuteZoom(RectangleF viewRect, bool isAnimated = false, int duration = 500, SolidMode solidMode = SolidMode.None, double actualWidth = 0, double actualHeight = 0)
        //{
        //    if (_presentor == null)
        //        return;

        //    var width = viewRect.Width + (_option.Inflate * 2);
        //    var height = viewRect.Width + (_option.Inflate * 2);

        //    var centerX = viewRect.X + viewRect.Width / 2;
        //    var centerY = viewRect.Y + viewRect.Height / 2;

        //    if (actualWidth == 0)
        //        actualWidth = _presentor.ActualWidth;
        //    if (actualHeight == 0)
        //        actualHeight = _presentor.ActualHeight;

        //    var scaleX = actualWidth / width;
        //    var scaleY = actualHeight / height;

        //    var scale = Math.Min(scaleX, scaleY);

        //    var translateX = -centerX * InvertScaleX * scale + actualWidth / 2;
        //    var translateY = -centerY * InvertScaleY * scale + actualHeight / 2;

        //    switch (solidMode)
        //    {
        //        case SolidMode.Min:
        //            _option.MinScale = Math.Min(scaleX, scaleY);
        //            _option.MaxScale = 0;
        //            break;
        //        case SolidMode.Max:
        //            _option.MaxScale = Math.Min(scaleX, scaleY);
        //            _option.MinScale = 0;
        //            break;
        //        case SolidMode.MinMax:
        //            _option.MinScale = Math.Min(scaleX, scaleY);
        //            _option.MaxScale = Math.Min(scaleX, scaleY);
        //            break;
        //    }

        //    if (solidMode != SolidMode.None)
        //    {
        //        _option.MinTranslateX = _option.MaxTranslateX = _option.InvertX ? translateX : translateX;
        //        _option.MinTranslateY = _option.MaxTranslateY = _option.InvertX ? translateY : translateY;
        //    }

        //    await UpdateValue(scale, translateX, translateY, isAnimated, duration);
        //}

        public void ZoomFit(double presentorWidth, double presentorHeight, double imageWidth, double imageHeight)
        {
            var scaleX = presentorWidth / imageWidth;
            var scaleY = presentorHeight / imageHeight;

            Scale = Math.Min(scaleX, scaleY);

            if (scaleX > scaleY)
            {
                TranslateX = presentorWidth / 2 - (imageWidth / 2 * Scale);
                TranslateY = 0;
            }
            else
            {
                TranslateX = 0;
                TranslateY = presentorHeight / 2 - (imageHeight / 2 * Scale);
            }

        }
    }
}
