using KT_Interface.Core;
using KT_Interface.Core.CameraFactorys;
using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using KT_Interface.Core.Services;
using NLog;
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

            CoreConfig config = new CoreConfig();
            LogFactory factory = new LogFactory();

            var grabService = new GrabService(factory);

            grabService.ImageGrabbed += ImageGrabbed;

            var infos = grabService.GetDeviceInfos();
            grabService.Connect(infos.First());
            HostCommService service =
                new HostCommService(
                    new InspectService(config, factory),
                    grabService,
                    new LightControlService(config, factory),
                    config, 
                    factory);

            service.Connect();

            GrabCommand = new DelegateCommand(() =>
            {
                grabService.Grab();
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
