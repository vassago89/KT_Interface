using KT_Interface.Core.Services;
using KT_Interface.Infos;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KT_Interface.ViewModels
{
    class CommunicationViewModel : BindableBase
    {
        private StateStore _stateStore;
        public StateStore StateStore
        {
            get
            {
                return _stateStore;
            }
        }

        public CommunicationViewModel(
            GrabService grabService,
            LightControlService lightControlService,
            HostCommService hostCommService,
            InspectService inspectService,
            StateStore stateStore,
            CancellationToken token)
        {
            _stateStore = stateStore;

            Task.Run(async () => 
            {
                while (token.IsCancellationRequested == false)
                {
                    _stateStore.HostInfo = GetConnectionInfo(hostCommService.IsConnected());
                    _stateStore.InspectorInfo = GetConnectionInfo(inspectService.IsConnected());
                    _stateStore.CameraInfo = GetConnectionInfo(grabService.IsConnected());
                    _stateStore.LightInfo = GetConnectionInfo(lightControlService.IsConnected());

                    await Task.Delay(1000).ConfigureAwait(false);
                }
            });
        }

        private ConnectionInfo GetConnectionInfo(bool isConnected)
        {
            if (isConnected)
            {
                return new ConnectionInfo(isConnected, "ONLINE", Colors.Green);
            }

            return new ConnectionInfo(isConnected, "OFFLINE", Colors.Red);
        }
    }
}
