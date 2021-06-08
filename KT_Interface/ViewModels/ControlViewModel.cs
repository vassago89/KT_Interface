using KT_Interface.Core;
using KT_Interface.Core.Services;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
                lightControlService.SetValue(coreConfig.LightValues);
                lightControlService.LightOn();

                var grabInfo = await grabService.Grab();
                lightControlService.LightOff();

                //if (grabInfo != null && coreConfig.UseInspector && stateStore.IsManualEnabled)
                //    inspectService.Inspect(grabInfo.Value);

                //if (lightControlService.SetValue(coreConfig.LightValues) && lightControlService.LightOn())
                //{
                //    var grabInfo = await grabService.Grab();
                //    lightControlService.LightOff();

                //    if (grabInfo != null && coreConfig.UseInspector && stateStore.IsManualEnabled)
                //        inspectService.Inspect(grabInfo.Value);
                //}
            });

            LiveCommand = new DelegateCommand(() =>
            {
                lightControlService.SetValue(coreConfig.LightValues);
                lightControlService.LightOn();
                grabService.StartGrab();

                //if (lightControlService.SetValue(coreConfig.LightValues) && lightControlService.LightOn())
                
                //if (inspectService.Inspected != null)
                //{
                //    List<SubResult> subResults = new List<SubResult>();
                //    subResults.Add(new SubResult(@"Pass, C:\Projects\KT_Interface_20210602\KT_Interface_20210602\KT_Interface\bin\Debug\Temp\1234_20210408120030.Bmp"));
                //    subResults.Add(new SubResult(@"Fail, C:\Projects\KT_Interface_20210602\KT_Interface_20210602\KT_Interface\bin\Debug\Temp\2345_20210408124853.Bmp"));
                //    inspectService.Inspected(new InspectResult(EJudgement.Pass, subResults));
                //}

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
