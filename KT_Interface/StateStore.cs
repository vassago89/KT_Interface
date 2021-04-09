using KT_Interface.Core;
using KT_Interface.Core.Infos;
using KT_Interface.Infos;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface
{
    class StateStore : BindableBase
    {
        private bool _isLiveMode;
        public bool IsLiveMode
        {
            get
            {
                return _isLiveMode;
            }
            set
            {
                SetProperty(ref _isLiveMode, value);
                CheckState();
            }
        }

        private bool _onManual;
        public bool OnManual
        {
            get
            {
                return _onManual;
            }
            set
            {
                SetProperty(ref _onManual, value);
                CheckState();
                IsSettingMode = false;
                IsLogMode = false;
                Mode = _onManual ? "Manual" : "Auto";
            }
        }

        private string _mode = "Auto";
        public string Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                SetProperty(ref _mode, value);
            }
        }

        private ConnectionInfo _hostInfo;
        public ConnectionInfo HostInfo
        {
            get
            {
                return _hostInfo;
            }
            set
            {
                SetProperty(ref _hostInfo, value);
                CheckState();
            }
        }

        private ConnectionInfo _inspectorInfo;
        public ConnectionInfo InspectorInfo
        {
            get
            {
                return _inspectorInfo;
            }
            set
            {
                SetProperty(ref _inspectorInfo, value);
                CheckState();
            }
        }

        private ConnectionInfo _cameraInfo;
        public ConnectionInfo CameraInfo
        {
            get
            {
                return _cameraInfo;
            }
            set
            {
                SetProperty(ref _cameraInfo, value);
                CheckState();
            }
        }

        private ConnectionInfo _lightInfo;
        public ConnectionInfo LightInfo
        {
            get
            {
                return _lightInfo;
            }
            set
            {
                SetProperty(ref _lightInfo, value);
                CheckState();
            }
        }

        
        private bool _isAutoEnabled;
        public bool IsAutoEnabled
        {
            get
            {
                return _isAutoEnabled;
            }
            private set
            {
                SetProperty(ref _isAutoEnabled, value);
            }
        }

        private bool _isManualEnabled;
        public bool IsManualEnabled
        {
            get
            {
                return _isManualEnabled;
            }
            private set
            {
                SetProperty(ref _isManualEnabled, value);
            }
        }

        private bool _isGrabEnabled;
        public bool IsGrabEnabled
        {
            get
            {
                return _isGrabEnabled;
            }
            private set
            {
                SetProperty(ref _isGrabEnabled, value);
            }
        }

        private bool _isSettingMode;
        public bool IsSettingMode
        {
            get
            {
                return _isSettingMode;
            }
            set
            {
                SetProperty(ref _isSettingMode, value);
                if (_isSettingMode)
                    IsLogMode = false;
            }
        }

        private bool _isLogMode;
        public bool IsLogMode
        {
            get
            {
                return _isLogMode;
            }
            set
            {
                SetProperty(ref _isLogMode, value);
                if (_isLogMode)
                    IsSettingMode = false;
            }
        }

        private AppState _appState;
        
        public StateStore(AppState appState)
        {
            _appState = appState;
        }

        private void CheckState()
        {
            _appState.OnManual = _onManual;
            _appState.IsAutoEnabled = IsAutoEnabled =
                _cameraInfo.IsConnected
                && _lightInfo.IsConnected
                && _hostInfo.IsConnected
                && _inspectorInfo.IsConnected
                && _isLiveMode == false;

            _appState.IsManualEnabled = IsManualEnabled =
                _cameraInfo.IsConnected
                && _lightInfo.IsConnected
                && _inspectorInfo.IsConnected;

            _appState.IsGrabEnabled = IsGrabEnabled =
                _cameraInfo.IsConnected
                && _lightInfo.IsConnected;
        }
    }
}
