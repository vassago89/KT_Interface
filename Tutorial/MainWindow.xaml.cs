using KT_Interface.Core;
using KT_Interface.Core.CameraFactorys;
using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tutorial
{
    public class ImageLoader : BindableBase
    {
        ImageSource _image;
        public ImageSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
    }

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Test { get; } = "TEST";
        public DelegateCommand GrabCommand { get; }

        public ImageLoader Store { get; } = new ImageLoader();

        public MainWindow()
        {
            InitializeComponent();

            GrabCommand = new DelegateCommand(() =>
            {

                var factory = new BaslerCameraFactory();
                var devices = factory.GetDevices();
                var camera = factory.Connect(devices.First());
                camera.ImageGrabbed += ImageGrabbed;
                var grabInfo = camera.StartGrab();

            });

            this.DataContext = this;
            //textBlock.Text = "Test";
            //this.DataContext = this;
        }

        private void ImageGrabbed(GrabInfo grabInfo)
        {
            var image = BitmapSource.Create(grabInfo.Width, grabInfo.Height, 96, 96, PixelFormats.Rgb24, null, grabInfo.Data, grabInfo.Width * 3);
            image.Freeze();
            Store.Image = image;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            
        }
    }
}
