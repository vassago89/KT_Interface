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

        private IEnumerable<SubResult> _subResults;
        public IEnumerable<SubResult> SubResults 
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

        private SubResult _selected;
        public SubResult Selected
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

            SubResults = new ObservableCollection<SubResult>();
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
            SubResults = result.SubResults;
        }
    }
    
}