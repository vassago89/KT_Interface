using KT_Interface.Core;
using KT_Interface.Core.Services;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KT_Interface.ViewModels
{
    class ControlViewModel
    {
        private StateStore _stateStore;
        public StateStore StateStore
        {
            get
            {
                return _stateStore;
            }
        }

        public DelegateCommand GrabCommand { get; set; }
        public DelegateCommand LiveCommand { get; set; }
        public DelegateCommand StopCommand { get; set; }
        public DelegateCommand ExitCommand { get; set; }

        public ControlViewModel(
            GrabService grabService, 
            LightControlService lightControlService, 
            InspectService inspectService, 
            StateStore stateStore,
            CoreConfig coreConfig)
        {
            _stateStore = stateStore;

            GrabCommand = new DelegateCommand(async () =>
            {
                if (lightControlService.SetValue(coreConfig.LightValues) && lightControlService.LightOn())
                {
                    var grabInfo = await grabService.Grab();
                    lightControlService.LightOff();

                    if (grabInfo != null && coreConfig.UseInspector && stateStore.IsManualEnabled)
                        inspectService.Inspect(grabInfo.Value);
                }
            });

            LiveCommand = new DelegateCommand(() =>
            {
                if (lightControlService.SetValue(coreConfig.LightValues) && lightControlService.LightOn())
                    grabService.StartGrab();

                StateStore.IsLiveMode = true;
            });

            StopCommand = new DelegateCommand(() =>
            {
                grabService.Stop();

                lightControlService.LightOff();

                StateStore.IsLiveMode = false;
            });

            ExitCommand = new DelegateCommand(() =>
            {
                Application.Current.Shutdown();
            });
        }
    }
}
