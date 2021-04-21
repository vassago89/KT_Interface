using KT_Interface.Core;
using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using KT_Interface.Core.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.ViewModels
{
    class SettingCameraViewModel : BindableBase
    {
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

        public CoreConfig CoreConfig { get; private set; }
        public StateStore StateStore { get; private set; }
        public IEnumerable<ECameraAutoValue> AutoValues { get; set; }

        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand ConnectCommand { get; private set; }
        public DelegateCommand DisconnectCommand { get; private set; }
        public DelegateCommand<ECameraAutoType?> SetAutoCommand { get; set; }
        public DelegateCommand SetTriggerModeCommand { get; set; }

        public SettingCameraViewModel(GrabService grabService, StateStore stateStore, CoreConfig coreConfig)
        {
            CoreConfig = coreConfig;
            StateStore = stateStore;

            AutoValues = Enum.GetValues(typeof(ECameraAutoValue)).Cast<ECameraAutoValue>();

            CameraInfos = grabService.GetDeviceInfos();

            if (grabService.IsConnected())
                ParameterInfo = grabService.GetParameterInfo();

            grabService.ParameterChanged += (parameterInfo =>
            {
                ParameterInfo = parameterInfo;
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

            SetTriggerModeCommand = new DelegateCommand(() =>
            {
                grabService.SetTriggerMode(ParameterInfo.OnTriggerMode);
            });
        }
    }
}
