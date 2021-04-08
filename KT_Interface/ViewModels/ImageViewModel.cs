using KT_Interface.Core.Infos;
using KT_Interface.Core.Services;
using KT_Interface.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KT_Interface.ViewModels
{
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

        public ImageViewModel(
            GrabService grabService,
            ZoomService zoomService)
        {
            grabService.ImageGrabbed += ImageGrabbed;

            ZoomService = zoomService;

            ZoomFitCommand = new DelegateCommand(ZoomFit);
        }

        private void ImageGrabbed(GrabInfo grabInfo)
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
            {
                bool zoomFitRequired = false;
                source.Freeze();
                if (ImageSource == null)
                    zoomFitRequired = true;

                ImageSource = source;

                if (zoomFitRequired)
                {
                    FrameworkElement.Dispatcher.Invoke(ZoomFit);
                }
            }
        }

        public void ZoomFit()
        {
            if (ImageSource != null)
                ZoomService.ZoomFit(FrameworkElement.ActualWidth, FrameworkElement.ActualHeight, _imageSource.Width, _imageSource.Height);
        }
    }
}
