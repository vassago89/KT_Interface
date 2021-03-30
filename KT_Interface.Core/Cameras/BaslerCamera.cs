using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KT_Interface.Core.Infos;
using Basler.Pylon;
using System.Runtime.InteropServices;

namespace KT_Interface.Core.Cameras
{
    public class BaslerCamera : KT_Interface.Core.Cameras.ICamera
    {
        private Basler.Pylon.ICamera _camera;
        private Basler.Pylon.PixelDataConverter _converter;
        private int _grabCount;
        private int _count;

        public Action<GrabInfo> ImageGrabbed { get; set; }
        
        public BaslerCamera(Basler.Pylon.ICameraInfo cameraInfo)
        {
            _grabCount = -1;
            _count = 0;

            _camera = new Basler.Pylon.Camera(cameraInfo);
            _camera.StreamGrabber.ImageGrabbed += StreamGrabber_ImageGrabbed;

            _camera.Open();

            _converter = new Basler.Pylon.PixelDataConverter();
            _converter.OutputPixelFormat = Basler.Pylon.PixelType.RGB8packed;
        }

        public bool Disconnect()
        {
            if (Stop() == false)
                return false;

            if (_camera.IsOpen)
                _camera.Close();
            else
                return false;

            if (_camera.IsConnected)
                _camera.Dispose();
            else
                return false;

            return true;
        }

        public CameraParameterInfo GetParameterInfo()
        {
            return new CameraParameterInfo(
                new CameraParameter(
                    _camera.Parameters[PLCamera.Width].GetValue(),
                    _camera.Parameters[PLCamera.Width].GetMinimum(),
                    _camera.Parameters[PLCamera.Width].GetMaximum()),
                new CameraParameter(
                    _camera.Parameters[PLCamera.Height].GetValue(),
                    _camera.Parameters[PLCamera.Height].GetMinimum(),
                    _camera.Parameters[PLCamera.Height].GetMaximum()),
                new CameraParameter(
                    _camera.Parameters[PLCamera.ExposureTime].GetValue(),
                    _camera.Parameters[PLCamera.ExposureTime].GetMinimum(),
                    _camera.Parameters[PLCamera.ExposureTime].GetMaximum()),
                new CameraParameter(
                    _camera.Parameters[PLCamera.Gain].GetValue(),
                    _camera.Parameters[PLCamera.Gain].GetMinimum(),
                    _camera.Parameters[PLCamera.Gain].GetMaximum()),
                new CameraParameter(
                    _camera.Parameters[PLCamera.AcquisitionFrameRate].GetValue(),
                    _camera.Parameters[PLCamera.AcquisitionFrameRate].GetMinimum(),
                    _camera.Parameters[PLCamera.AcquisitionFrameRate].GetMaximum()),
                new CameraParameter(
                    _camera.Parameters[PLCamera.TriggerDelay].GetValue(),
                    _camera.Parameters[PLCamera.TriggerDelay].GetMinimum(),
                    _camera.Parameters[PLCamera.TriggerDelay].GetMaximum()),
                _camera.Parameters[PLCamera.TriggerMode].GetValue() == PLCamera.TriggerMode.On,
                GetAutoValueDictionary());
        }

        private IDictionary<ECameraAutoType, ECameraAutoValue> GetAutoValueDictionary()
        {
            var dictionary = new Dictionary<ECameraAutoType, ECameraAutoValue>();

            var exposureAuto = _camera.Parameters[PLCamera.ExposureAuto].GetValue();
            if (exposureAuto == PLCamera.ExposureAuto.Once)
                dictionary[ECameraAutoType.Exposure] = ECameraAutoValue.Once;
            else if (exposureAuto == PLCamera.ExposureAuto.Continuous)
                dictionary[ECameraAutoType.Exposure] = ECameraAutoValue.Continuous;
            else
                dictionary[ECameraAutoType.Exposure] = ECameraAutoValue.Off;

            var gainAuto = _camera.Parameters[PLCamera.GainAuto].GetValue();
            if (gainAuto == PLCamera.GainAuto.Once)
                dictionary[ECameraAutoType.Gain] = ECameraAutoValue.Once;
            else if (gainAuto == PLCamera.GainAuto.Continuous)
                dictionary[ECameraAutoType.Gain] = ECameraAutoValue.Continuous;
            else
                dictionary[ECameraAutoType.Gain] = ECameraAutoValue.Off;

            var whiteBalanceAuto = _camera.Parameters[PLCamera.BalanceWhiteAuto].GetValue();
            if (gainAuto == PLCamera.BalanceWhiteAuto.Once)
                dictionary[ECameraAutoType.WhiteBalance] = ECameraAutoValue.Once;
            else if (gainAuto == PLCamera.BalanceWhiteAuto.Continuous)
                dictionary[ECameraAutoType.WhiteBalance] = ECameraAutoValue.Continuous;
            else
                dictionary[ECameraAutoType.WhiteBalance] = ECameraAutoValue.Off;

            return dictionary;
        }

        public bool SetAuto(ECameraAutoType type, ECameraAutoValue value)
        {
            switch (type)
            {
                case ECameraAutoType.Exposure:
                    switch (value)
                    {
                        case ECameraAutoValue.Off:
                            return _camera.Parameters[PLCamera.ExposureAuto].TrySetValue(PLCamera.ExposureAuto.Off);
                        case ECameraAutoValue.Once:
                            return _camera.Parameters[PLCamera.ExposureAuto].TrySetValue(PLCamera.ExposureAuto.Once);
                        case ECameraAutoValue.Continuous:
                            return _camera.Parameters[PLCamera.ExposureAuto].TrySetValue(PLCamera.ExposureAuto.Continuous);
                    }

                    break;
                case ECameraAutoType.Gain:
                    switch (value)
                    {
                        case ECameraAutoValue.Off:
                            return _camera.Parameters[PLCamera.GainAuto].TrySetValue(PLCamera.GainAuto.Off);
                        case ECameraAutoValue.Once:
                            return _camera.Parameters[PLCamera.GainAuto].TrySetValue(PLCamera.GainAuto.Once);
                        case ECameraAutoValue.Continuous:
                            return _camera.Parameters[PLCamera.GainAuto].TrySetValue(PLCamera.GainAuto.Continuous);
                    }
                    break;
                case ECameraAutoType.WhiteBalance:
                    switch (value)
                    {
                        case ECameraAutoValue.Off:
                            return _camera.Parameters[PLCamera.BalanceWhiteAuto].TrySetValue(PLCamera.BalanceWhiteAuto.Off);
                        case ECameraAutoValue.Once:
                            return _camera.Parameters[PLCamera.BalanceWhiteAuto].TrySetValue(PLCamera.BalanceWhiteAuto.Once);
                        case ECameraAutoValue.Continuous:
                            return _camera.Parameters[PLCamera.BalanceWhiteAuto].TrySetValue(PLCamera.BalanceWhiteAuto.Continuous);
                    }
                    break;
            }

            return false;
        }

        public bool SetParameter(ECameraParameter parameter, double value)
        {
            switch (parameter)
            {
                case ECameraParameter.Exposure:
                    return _camera.Parameters[PLCamera.ExposureTime].TrySetValue(value);
                case ECameraParameter.Gain:
                    return _camera.Parameters[PLCamera.Gain].TrySetValue(value);
                case ECameraParameter.FrameRate:
                    return _camera.Parameters[PLCamera.AcquisitionFrameRate].TrySetValue(value);
                case ECameraParameter.TriggerDelay:
                    return _camera.Parameters[PLCamera.TriggerDelay].TrySetValue(value);
            }
            
            return false;
        }

        public bool SetROI(uint x, uint y, uint width, uint height)
        {
            if (_camera.Parameters[PLCamera.OffsetX].TrySetValue(x) == false)
                return false;

            if (_camera.Parameters[PLCamera.OffsetY].TrySetValue(y) == false)
                return false;

            if (_camera.Parameters[PLCamera.Width].TrySetValue(width) == false)
                return false;

            if (_camera.Parameters[PLCamera.Height].TrySetValue(height) == false)
                return false;

            return true;
        }

        public bool SetTriggerMode(bool isTriggerMode)
        {
            if (isTriggerMode)
                return _camera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.On);
            else
                return _camera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
        }

        public bool StartGrab(int grabCount = -1)
        {
            _grabCount = grabCount;
            _count = 0;

            if (_camera.IsOpen == false
                || _camera.IsConnected == false
                || _camera.StreamGrabber.IsGrabbing == true)
                return false;

            _camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);

            return true;
        }

        private void StreamGrabber_ImageGrabbed(object sender, Basler.Pylon.ImageGrabbedEventArgs e)
        {
            if (_grabCount > 0)
            {
                _count++;

                if (_count >= _grabCount)
                    Stop();
            }
            
            Basler.Pylon.IGrabResult result = e.GrabResult;

            if (result.GrabSucceeded)
            {
                if (result.PixelTypeValue.IsMonoImage())
                {
                    var src = result.PixelData as byte[];
                    var data = new byte[src.Length];
                    Array.Copy(src, data, src.Length);
                    if (ImageGrabbed != null)
                    {
                        ImageGrabbed(
                        new GrabInfo(
                            EGrabResult.Success, result.Width, result.Height, 1, data));
                    }
                    
                    return;
                }
                else
                {
                    var data = new byte[result.Width * result.Height * 3];
                    _converter.Convert(data, result);

                    if (ImageGrabbed != null)
                    {
                        ImageGrabbed(
                        new GrabInfo(
                            EGrabResult.Success, result.Width, result.Height, 3, data));
                    }
                    
                    return;
                }
            }

            if (ImageGrabbed != null)
                ImageGrabbed(new GrabInfo(EGrabResult.Error));
        }

        public bool Stop()
        {
            if (_camera.StreamGrabber.IsGrabbing)
            {
                _camera.StreamGrabber.Stop();
                return true;
            }

            return false;
        }
    }
}