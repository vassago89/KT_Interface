using KT_Interface.Core.CameraFactorys;
using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KT_Interface.Core.Services
{
    public class GrabService
    {
        ILogger _logger;
        ICameraFactory _baslerFactory;
        ICameraFactory _hikFactory;
        ICamera _camera;

        bool _grabbing;
        GrabInfo _grabInfo;

        public Action<GrabInfo> ImageGrabbed { get; set; }
        public Action GrabDone { get; set; }
        public Action GrabStarted { get; set; }
        public Action<CameraParameterInfo> ParameterChanged { get; set; }

        public GrabService()
        {
            _baslerFactory = CameraFactory.Instance.Create(ECameraManufacturer.Basler);
            _hikFactory = CameraFactory.Instance.Create(ECameraManufacturer.Hik);

            _logger = LogManager.GetCurrentClassLogger();

            _grabbing = false;
        }

        public IEnumerable<CameraInfo> GetDeviceInfos()
        {
            var Infos = new List<CameraInfo>();

            // Basler SDK 필요
            try
            {
                Infos.AddRange(_baslerFactory.GetDevices());
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            // MVS SDK 필요
            try
            {
                Infos.AddRange(_hikFactory.GetDevices());
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return Infos;
        }

        public bool IsConnected()
        {
            if (_camera != null)
                return _camera.IsConnected();

            return false;
        }

        public async Task<GrabInfo?> Grab()
        {
            try
            {
                if (_grabbing)
                    return null;

                _grabbing = true;
                if (_camera.StartGrab(1) == false)
                {
                    Stop();
                    return null;
                }

                while (_grabbing)
                    await Task.Delay(1);

                return _grabInfo;
            }
            catch (Exception e)
            {
                _grabbing = false;
                _logger.Error(e);
                return null;
            }
        }

        public bool StartGrab(int grabCount = -1)
        {
            if (_camera != null)
                return _camera.StartGrab(grabCount);
            
            return false;
        }

        public bool Stop()
        {
            _grabbing = false;

            if (_camera != null)
                return _camera.Stop();

            return false;
        }

        public bool SetParameter(ECameraParameter parameter, double value)
        {
            if (_camera != null)
            {
                if(_camera.SetParameter(parameter, value))
                {
                    if (ParameterChanged != null)
                        ParameterChanged(GetParameterInfo());

                    return true;
                }
            }
                

            return false;
        }

        public bool SetTriggerMode(bool isTriggerMode)
        {
            if (_camera != null)
            {
                if (_camera.SetTriggerMode(isTriggerMode))
                {
                    if (ParameterChanged != null)
                        ParameterChanged(GetParameterInfo());

                    return true;
                }
            }
                

            return false;
        }

        public bool SetAuto(ECameraAutoType type, ECameraAutoValue value)
        {
            if (_camera != null)
            {
                if (_camera.SetAuto(type, value))
                {
                    if (ParameterChanged != null)
                        ParameterChanged(GetParameterInfo());

                    return true;
                }
            }

            return false;
        }

        public bool SetROI(uint x, uint y, uint width, uint height)
        {
            if (_camera != null)
            {
                if (_camera.SetROI(x, y, width, height))
                {
                    if (ParameterChanged != null)
                        ParameterChanged(GetParameterInfo());

                    return true;
                }
            }
                

            return false;
        }

        public bool Connect(CameraInfo info)
        {
            Disconnect();

            switch (info.Manufacturer)
            {
                case ECameraManufacturer.Basler:
                    if (_baslerFactory.IsExist(info) == false)
                        return false;
                    _camera = _baslerFactory.Connect(info);
                    break;
                case ECameraManufacturer.Hik:
                    if (_hikFactory.IsExist(info) == false)
                        return false;
                    _camera = _hikFactory.Connect(info);
                    break;
            }

            if (_camera == null)
                return false;

            _camera.ImageGrabbed = Grabbed;
            _camera.GrabDone = Done;
            _camera.GrabStarted = Started;
            _logger.Info("Connect");
            
            return true;
        }

        public CameraParameterInfo GetParameterInfo()
        {
            if (_camera != null && _camera.IsConnected())
                return _camera.GetParameterInfo();

            return null;
        }

        public void Disconnect()
        {
            if (_camera != null)
            {
                _camera.Disconnect();
                _camera = null;

                _logger.Info("Disconnect");
            }
        }

        private void Grabbed(GrabInfo grabInfo)
        {
            if (_grabbing)
            {
                _grabInfo = grabInfo;
                _grabbing = false;
            }
            
            if (ImageGrabbed != null)
                ImageGrabbed(grabInfo);
        }

        private void Done()
        {
            if (GrabDone != null)
                GrabDone();
        }

        private void Started()
        {
            if (GrabStarted != null)
                GrabStarted();
        }
    }
}