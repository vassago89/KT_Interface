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
        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand
        {
            get
            {
                return _closeCommand;
            }
        }

        public SubViewModel()
        {
            _closeCommand = new DelegateCommand(() =>
            {
                Application.Current.Shutdown();
            });

            _closeCommand = new DelegateCommand(Close);
        }

        private void Close()
        {
            Application.Current.Shutdown();
        }
    }
}
