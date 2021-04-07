using KT_Interface.Core;
using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using KT_Interface.Core.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KT_Interface.ViewModels
{
    class ObservableValue<T> : BindableBase
    {
        public Action<T> ValueChanged;

        private T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                SetProperty(ref _value, value);
                if (ValueChanged != null)
                    ValueChanged(_value);
            }
        }

        public ObservableValue(T value)
        {
            _value = value;
        }
    }

    class SettingViewModel : BindableBase
    {
        private StateStore _stateStore;
        public StateStore StateStore
        {
            get
            {
                return _stateStore;
            }
        }

        private CoreConfig _coreConfig;
        public CoreConfig CoreConfig
        {
            get
            {
                return _coreConfig;
            }
        }

        public IEnumerable<ImageFormat> ImageFormats { get; }
        public DelegateCommand ResultPathCommand { get; }

        private string _resultPath;
        public string ResultPath
        {
            get
            {
                return _resultPath;
            }
            set
            {
                SetProperty(ref _resultPath, value);
            }
        }

        public IEnumerable<int> BaudRates { get; }
        public IEnumerable<string> Ports { get; }
        public IEnumerable<Parity> Parities { get; }
        public IEnumerable<int> DataBits { get; }
        public IEnumerable<StopBits> StopBits { get; }

        public IEnumerable<ESaveMode> SaveModes { get; }

        private IEnumerable<CameraInfo> _cameraInfos;
        public IEnumerable<CameraInfo> CameraInfos
        {
            get
            {
                return _cameraInfos;
            }
            set
            {
                SetProperty(ref _cameraInfos, value);
            }
        }

        private CameraParameterInfo _parameterInfo;
        public CameraParameterInfo ParameterInfo
        {
            get
            {
                return _parameterInfo;
            }
            set
            {
                SetProperty(ref _parameterInfo, value);

                IsEnableCameraSetting = ParameterInfo != null ? true : false;
            }
        }

        private bool _isEnableCameraSetting;
        public bool IsEnableCameraSetting
        {
            get
            {
                return _isEnableCameraSetting;
            }
            set
            {
                SetProperty(ref _isEnableCameraSetting, value);
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

        public IEnumerable<ECameraAutoValue> AutoValues { get; }

        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }
        public DelegateCommand<ECameraAutoType?> SetAutoCommand { get; }

        public DelegateCommand LightOnCommand { get; }
        public DelegateCommand LightOffCommand { get; }

        public DelegateCommand SaveCommand { get; }

        public SettingViewModel(GrabService grabService, LightControlService lightControlService, StateStore stateStore, CoreConfig coreConfig)
        {
            _stateStore = stateStore;
            _coreConfig = coreConfig;

            ResultPath = _coreConfig.ResultPath;

            LightOnCommand = new DelegateCommand(() =>
            {
                lightControlService.SetValue(coreConfig.LightValues);
                lightControlService.LightOn();
            });

            LightOffCommand = new DelegateCommand(() =>
            {
                lightControlService.LightOff();
            });

            grabService.ParameterChanged += (parameterInfo =>
            {
                ParameterInfo = parameterInfo;
            });

            var temp = new byte[coreConfig.LightNum];
            Array.Copy(coreConfig.LightValues, temp, coreConfig.LightNum);

            LightNum = coreConfig.LightNum;
            if (coreConfig.LightValues != null)
                for (int i = 0; i < LightNum; i++)
                    _lightValues[i].Value = temp[i];

            if (grabService.IsConnected())
                ParameterInfo = grabService.GetParameterInfo();

            CameraInfos = grabService.GetDeviceInfos();

            var ports = SerialPort.GetPortNames().ToArray();
            if (coreConfig.LightSerialInfo != null)
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    if (ports[i] == coreConfig.LightSerialInfo.PortName)
                    {
                        ports[i] = coreConfig.LightSerialInfo.PortName;
                        break;
                    }
                }
            }

            Ports = ports;
            BaudRates = new int[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
            Parities = Enum.GetValues(typeof(Parity)).Cast<Parity>();
            DataBits = new int[] { 5, 6, 7, 8 };
            StopBits = Enum.GetValues(typeof(StopBits)).Cast<StopBits>();

            SaveModes = Enum.GetValues(typeof(ESaveMode)).Cast<ESaveMode>();

            AutoValues = Enum.GetValues(typeof(ECameraAutoValue)).Cast<ECameraAutoValue>();

            ImageFormats = new ImageFormat[] { ImageFormat.Bmp, ImageFormat.Png, ImageFormat.Jpeg };
            ResultPathCommand = new DelegateCommand(() =>
            {
                var dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = _coreConfig.ResultPath;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    _coreConfig.ResultPath = dialog.FileName;
                    ResultPath = _coreConfig.ResultPath;
                }
            });

            RefreshCommand = new DelegateCommand(() =>
            {
                CameraInfos = grabService.GetDeviceInfos();
            });

            ConnectCommand = new DelegateCommand(() =>
            {
                if (CoreConfig.CameraInfo != null)
                {
                    if (grabService.Connect(CoreConfig.CameraInfo))
                        ParameterInfo = grabService.GetParameterInfo();
                }
            });

            DisconnectCommand = new DelegateCommand(() =>
            {
                grabService.Disconnect(); 
                ParameterInfo = null;
            });

            SetAutoCommand = new DelegateCommand<ECameraAutoType?>(type =>
            {
                if (type == null)
                    return;

                grabService.SetAuto(type.Value, ParameterInfo.AutoValues[type.Value]);
                ParameterInfo = grabService.GetParameterInfo();
            });

            SaveCommand = new DelegateCommand(() =>
            {
                File.WriteAllText("CoreConfig.json", JsonConvert.SerializeObject(coreConfig));
            });
        }
    }
}
