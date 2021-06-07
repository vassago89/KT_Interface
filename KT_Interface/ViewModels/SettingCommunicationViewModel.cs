using KT_Interface.Core;
using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KT_Interface.Core.Services;
using System.Threading;

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
        public DelegateCommand TestCommand { get; private set;}

        public SettingCommunicationViewModel(InspectService inspectService, CoreConfig coreConfig)
        {
            CoreConfig = coreConfig;

            _resultTimeout = CoreConfig.ResultTimeout;

            TestCommand = new DelegateCommand(() =>
            {
                inspectService.Inspect(null, "test", new CancellationToken());
            });
        }
    }
}
