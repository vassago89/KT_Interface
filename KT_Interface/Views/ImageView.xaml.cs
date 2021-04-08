using KT_Interface.ViewModels;
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

namespace KT_Interface.Views
{
    /// <summary>
    /// ImaveView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageView : UserControl
    {
        private ImageViewModel _viewModel;

        public ImageView()
        {
            InitializeComponent();

            _viewModel = DataContext as ImageViewModel;
            if (_viewModel != null)
                _viewModel.FrameworkElement = Canvas;
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.ZoomFit();
        }

        private void Canvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void Canvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Canvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}
