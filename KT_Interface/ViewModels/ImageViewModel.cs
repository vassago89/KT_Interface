using KT_Interface.Core.Infos;
using KT_Interface.Core.Services;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ImageViewModel(GrabService grabService)
        {
            grabService.ImageGrabbed += ImageGrabbed;
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
                source.Freeze();
                ImageSource = source;
            }
        }
    }
}
