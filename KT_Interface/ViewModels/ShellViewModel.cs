using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.ViewModels
{
    class ShellViewModel
    {
        public AppConfig AppConfig { get; }

        public ShellViewModel(AppConfig appConfig)
        {
            AppConfig = appConfig;
        }
    }
}
