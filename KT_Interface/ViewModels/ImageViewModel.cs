using KT_Interface.Core;
using KT_Interface.Core.Infos;
using KT_Interface.Core.Patterns;
using KT_Interface.Core.Services;
using KT_Interface.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KT_Interface.ViewModels
{
    class SubResultWrapper
    {
        public SubResult SubResult { get; }

        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get
            {
                if (_imageSource == null)
                    _imageSource = SubResult.GetGrabinfo().GetImage();

                return _imageSource;
            }
        }

        public SubResultWrapper(SubResult subResult)
        {
            SubResult = subResult;
        }
    }

    public static class GrabInfoExtensions
    {
        public static ImageSource GetImage(this GrabInfo grabInfo)
        {
            ImageSource source = null;
            switch (grabInfo.Channels)
            {
                case 1:
                    source = BitmapSource.Create(grabInfo.Width, grabInfo.Height, 96, 96, PixelFormats.Gray8, null, grabInfo.Data, grabInfo.Width);
                    break;
                case 3:
                    source = BitmapSource.Create(grabInfo.Width, grabInfo.Height, 96, 96, PixelFormats.Rgb24, null, grabInfo.Data, grabInfo.Width * 3);
                    break;
            }

            if (source != null)
                source.Freeze();

            return source;
        }
    }

    class ImageViewModel : BindableBase
    {
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get
            {
                return _imageSource;
            }
            private set
            {
                SetProperty(ref _imageSource, value);
            }
        }

        public FrameworkElement FrameworkElement { get; set; }
        public ZoomService ZoomService { get; set; }

        public DelegateCommand ZoomInCommand { get; set; }
        public DelegateCommand ZoomOutCommand { get; set; }
        public DelegateCommand ZoomFitCommand { get; set; }

        private IEnumerable<SubResultWrapper> _subResults;
        public IEnumerable<SubResultWrapper> SubResults 
        { 
            get
            {
                return _subResults;
            }
            private set
            {
                SetProperty(ref _subResults, value);
            }
        }

        private SubResultWrapper _selected;
        public SubResultWrapper Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                SetProperty(ref _selected, value);
            }
        }

        private PipeLine<GrabInfo> _pipeLine;
        private CoreConfig _coreConfig;

        public StateStore StateStore { get; }

        private bool _isCalcMode;
        public bool IsCalcMode
        {
            get
            {
                return _isCalcMode;
            }
            set
            {
                SetProperty(ref _isCalcMode, value);
            }
        }

        private Point _startPt;
        public Point StartPt
        {
            get
            {
                return _startPt;
            }
            set
            {
                SetProperty(ref _startPt, value);
                CalcLength = 0;
            }
        }

        private Point _endPt;
        public Point EndPt
        {
            get
            {
                return _endPt;
            }
            set
            {
                SetProperty(ref _endPt, value);

                if (ZoomService.Scale == 0)
                    return;

                CalcLength = Math.Sqrt(
                    Math.Pow(_startPt.X - _endPt.X, 2) * _coreConfig.ResolutionWidth 
                    + Math.Pow(_startPt.Y - _endPt.Y, 2) * _coreConfig.ResolutionHeight) / ZoomService.Scale;
            }
        }

        private double _calcLength;
        public double CalcLength
        {
            get
            {
                return _calcLength;
            }
            set
            {
                SetProperty(ref _calcLength, value);
            }
        }

        public ImageViewModel(
            GrabService grabService,
            ZoomService zoomService,
            InspectService inspectService,
            StateStore stateStore,
            CoreConfig coreConfig,
            CancellationToken token)
        {
            StateStore = stateStore;
            _coreConfig = coreConfig;

            SubResults = new ObservableCollection<SubResultWrapper>();
            BindingOperations.EnableCollectionSynchronization(SubResults, new object());

            grabService.ImageGrabbed += ImageGrabbed;

            ZoomService = zoomService;

            ZoomFitCommand = new DelegateCommand(ZoomFit);
            ZoomInCommand = new DelegateCommand(ZoomIn);
            ZoomOutCommand = new DelegateCommand(ZoomOut);

            inspectService.Inspected += Inspected;

            _pipeLine = new SinglePipeLine<GrabInfo>(info => DrawImage(info), -1, true);
            _pipeLine.Run(token);
        }

        private void DrawImage(GrabInfo grabInfo)
        {
            ImageSource source = grabInfo.GetImage();

            if (source != null)
            {
                bool zoomFitRequired = false;
                if (ImageSource == null)
                    zoomFitRequired = true;

                ImageSource = source;

                if (zoomFitRequired)
                    FrameworkElement.Dispatcher.Invoke(ZoomFit);
            }
        }

        private void ImageGrabbed(GrabInfo grabInfo)
        {
            _pipeLine.Enqueue(grabInfo);
        }

        public void ZoomFit()
        {
            if (ImageSource != null)
                ZoomService.ZoomFit(FrameworkElement.ActualWidth, FrameworkElement.ActualHeight, _imageSource.Width, _imageSource.Height);
        }

        public void ZoomIn()
        {
            ZoomService.ZoomIn(FrameworkElement.ActualWidth, FrameworkElement.ActualHeight);
        }

        public void ZoomOut()
        {
            ZoomService.ZoomOut(FrameworkElement.ActualWidth, FrameworkElement.ActualHeight);
        }

        private void Inspected(InspectResult result)
        {
            var wrappers = new List<SubResultWrapper>();
            foreach (var sub in result.SubResults)
                wrappers.Add(new SubResultWrapper(sub));

            SubResults = wrappers;
        }
    }
    
}