using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KT_Interface.ViewModels
{
    class SubViewModel
    {
        public DelegateCommand CloseCommand { get; }

        public SubViewModel()
        {
            CloseCommand = new DelegateCommand(() =>
            {
                Application.Current.Shutdown();
            });

            CloseCommand = new DelegateCommand(Close);
        }

        private void Close()
        {
            Application.Current.Shutdown();
        }
    }
}
