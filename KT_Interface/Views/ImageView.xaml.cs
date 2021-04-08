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
        Point _initPos;
        bool _isDrag;

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

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var pos = e.GetPosition(sender as IInputElement);
            if (e.Delta > 0)
                _viewModel.ZoomService.ExecuteZoom(pos.X, pos.Y, 1.1f);
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(sender as IInputElement);
            _initPos = pos;
            _isDrag = true;
        }

        private void Border_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrag = false;
        }
        
        private void Border_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrag == false)
                return;

            var pos = e.GetPosition(sender as IInputElement);
            _viewModel.ZoomService.TranslateX = pos.X - _initPos.X;
            _viewModel.ZoomService.TranslateY = pos.Y - _initPos.Y;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            _isDrag = false;
        }
    }
}
