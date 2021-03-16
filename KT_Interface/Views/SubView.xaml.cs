﻿using KT_Interface.ViewModels;
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
using Unity;

namespace KT_Interface.Views
{
    /// <summary>
    /// SubView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SubView : UserControl
    {
        public SubView()
        {
            InitializeComponent();

            DataContext = ContainerRegistry.Container.Resolve<SubViewModel>();
        }
    }
}
