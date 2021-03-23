using KT_Interface.Core.CameraFactorys;
using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public GrabService(LogFactory factory)
        {
            _baslerFactory = CameraFactory.Instance.Create(ECameraManufacturer.Basler);
            _hikFactory = CameraFactory.Instance.Create(ECameraManufacturer.Hik);

            _logger = factory.GetCurrentClassLogger();

            _grabbing = false;
        }

        public IEnumerable<CameraInfo> GetDeviceInfos()
        {
            var Infos = new List<CameraInfo>();
            Infos.AddRange(_baslerFactory.GetDevices());
            Infos.AddRange(_hikFactory.GetDevices());

            _logger.Info("GetDeviceInfos");

            return Infos;
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
                return null;
            }
        }

        public bool StartGrab(int grabCount)
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
                return _camera.SetParameter(parameter, value);

            return false;
        }

        public bool SetTriggerMode(bool isTriggerMode)
        {
            if (_camera != null)
                return _camera.SetTriggerMode(isTriggerMode);

            return false;
        }

        public bool SetAuto(ECameraAutoType type, ECameraAutoValue value)
        {
            if (_camera != null)
                return _camera.SetAuto(type, value);

            return false;
        }

        public bool SetROI(uint x, uint y, uint width, uint height)
        {
            if (_camera != null)
                return _camera.SetROI(x, y, width, height);

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

            _camera.ImageGrabbed = Grabbed;
            _logger.Info("Connect");

            return true;
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

        public void Grabbed(GrabInfo grabInfo)
        {
            if (_grabbing)
            {
                _grabInfo = grabInfo;
                _grabbing = false;
            }
            
            ImageGrabbed?.Invoke(grabInfo);
        }
    }
}