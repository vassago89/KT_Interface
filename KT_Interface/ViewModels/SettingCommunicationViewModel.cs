using KT_Interface.Core;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.ViewModels
{
    class SettingCommunicationViewModel : BindableBase
    {
        private int _resultTimeout;
        public int ResultTimeout
        {
            get
            {
                return _resultTimeout;
            }
            set
            {
                SetProperty(ref _resultTimeout, value);
                CoreConfig.ResultTimeout = _resultTimeout;
            }
        }

        public CoreConfig CoreConfig { get; private set; }

        public SettingCommunicationViewModel(CoreConfig coreConfig)
        {
            CoreConfig = coreConfig;

            _resultTimeout = CoreConfig.ResultTimeout;
        }
    }
}
