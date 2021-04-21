using KT_Interface.Core;
using KT_Interface.Core.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.ViewModels
{
    class SettingLightViewModel :BindableBase
    {
        private int _responseTimeout;
        public int ResponseTimeout
        {
            get
            {
                return _responseTimeout;
            }
            set
            {
                SetProperty(ref _responseTimeout, value);
                CoreConfig.ReponseTimeout = _responseTimeout;
            }
        }


        private int _lightNum;
        public int LightNum
        {
            get
            {
                return _lightNum;
            }
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 4)
                    value = 4;

                SetProperty(ref _lightNum, value);
                CoreConfig.LightNum = _lightNum;
                CoreConfig.LightValues = new byte[_lightNum];

                var lightValues = new ObservableValue<byte>[_lightNum];
                for (int i = 0; i < lightValues.Length; i++)
                {
                    lightValues[i] = new ObservableValue<byte>(255);
                    var index = i;
                    lightValues[i].ValueChanged = (data =>
                    {
                        CoreConfig.LightValues[index] = data;
                    });
                }

                LightValues = lightValues;
            }
        }

        private ObservableValue<byte>[] _lightValues;
        public ObservableValue<byte>[] LightValues
        {
            get
            {
                return _lightValues;
            }
            set
            {
                SetProperty(ref _lightValues, value);
            }
        }

        public IEnumerable<int> BaudRates { get; set; }
        public IEnumerable<string> Ports { get; set; }
        public IEnumerable<Parity> Parities { get; set; }
        public IEnumerable<int> DataBits { get; set; }
        public IEnumerable<StopBits> StopBits { get; set; }

        public StateStore StateStore { get; private set; }
        public CoreConfig CoreConfig { get; private set; }

        public DelegateCommand RefreshCommand { get; set; }
        public DelegateCommand ConnectCommand { get; set; }
        public DelegateCommand DisconnectCommand { get; set; }

        public DelegateCommand OnCommand { get; set; }
        public DelegateCommand OffCommand { get; set; }

        public SettingLightViewModel(LightControlService lightControlService, StateStore stateStore, CoreConfig coreConfig)
        {
            StateStore = stateStore;
            CoreConfig = coreConfig;

            _responseTimeout = CoreConfig.ReponseTimeout;

            OnCommand = new DelegateCommand(() =>
            {
                lightControlService.SetValue(coreConfig.LightValues);
                lightControlService.LightOn();
            });

            OffCommand = new DelegateCommand(() =>
            {
                lightControlService.LightOff();
            });

            var temp = new byte[coreConfig.LightNum];
            if (coreConfig.LightValues != null)
                Array.Copy(coreConfig.LightValues, temp, coreConfig.LightNum);

            LightNum = coreConfig.LightNum;
            if (coreConfig.LightValues != null)
            {
                for (int i = 0; i < LightNum; i++)
                    _lightValues[i].Value = temp[i];
            }

            RefreshPorts();

            BaudRates = new int[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
            Parities = Enum.GetValues(typeof(Parity)).Cast<Parity>();
            DataBits = new int[] { 5, 6, 7, 8 };
            StopBits = Enum.GetValues(typeof(StopBits)).Cast<StopBits>();

            RefreshCommand = new DelegateCommand(() =>
            {
                RefreshPorts();
            });

            ConnectCommand = new DelegateCommand(() =>
            {
                if (CoreConfig.LightSerialInfo != null)
                {
                    lightControlService.Connect();
                }
            });

            DisconnectCommand = new DelegateCommand(() =>
            {
                lightControlService.Disconnect();
            });
        }

        private void RefreshPorts()
        {
            var ports = SerialPort.GetPortNames().ToArray();
            if (CoreConfig.LightSerialInfo != null)
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    if (ports[i] == CoreConfig.LightSerialInfo.PortName)
                    {
                        ports[i] = CoreConfig.LightSerialInfo.PortName;
                        break;
                    }
                }
            }

            Ports = ports;
        }
    }
}
