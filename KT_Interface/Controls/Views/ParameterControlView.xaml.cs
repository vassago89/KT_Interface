using KT_Interface.Controls.ViewModels;
using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
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

namespace KT_Interface.Controls.Views
{
    /// <summary>
    /// ParameterControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ParameterControlView : UserControl
    {
        public static readonly DependencyProperty ParameterTypeProperty =
            DependencyProperty.Register(
                "ParameterType",
                typeof(ECameraParameter),
                typeof(ParameterControlView));

        public static readonly DependencyProperty ParameterInfoProperty =
            DependencyProperty.Register(
                "ParameterInfo",
                typeof(CameraParameterInfo),
                typeof(ParameterControlView), 
                new PropertyMetadata((d, e) =>
                {
                    var view = d as ParameterControlView;
                    if (view == null)
                        return;

                    var viewModel = view.DataContext as ParameterControlViewModel;
                    if (viewModel == null)
                        return;

                    var info = e.NewValue as CameraParameterInfo;
                    if (info == null)
                    {
                        viewModel.CameraParameter = null;
                        return;
                    }

                    viewModel.ParameterType = view.ParameterType;
                    viewModel.CameraParameter = info.Parameters[view.ParameterType];
                }));

        public ECameraParameter ParameterType
        {
            get { return (ECameraParameter)GetValue(ParameterTypeProperty); }
            set { SetValue(ParameterTypeProperty, value); }
        }

        public CameraParameterInfo ParameterInfo
        {
            get { return (CameraParameterInfo)GetValue(ParameterInfoProperty); }
            set { SetValue(ParameterInfoProperty, value); }
        }

        public ParameterControlView()
        {
            InitializeComponent();
        }
    }
}
