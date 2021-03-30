using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.ViewModels
{
    class ShellViewModel
    {
        private AppConfig _appConfig;
        public AppConfig AppConfig 
        { 
            get
            {
                return _appConfig;
            }
        }

        public ShellViewModel(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }
    }
}
