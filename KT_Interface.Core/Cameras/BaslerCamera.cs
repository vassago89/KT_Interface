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
        public Action GrabStarted { get; set; }
        public Action GrabDone { get; set; }

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

        public bool IsConnected()
        {
            if (_camera != null)
                return _camera.IsConnected;

            return false;
        }

        public bool Disconnect()
        {
            Stop();

            if (_camera.IsOpen == false)
                return false;

            _camera.Close();
            _camera.Dispose();

            return true;
        }

        public CameraParameterInfo GetParameterInfo()
        {
            return new CameraParameterInfo(
                _camera.Parameters[PLCamera.TriggerMode].GetValue() == PLCamera.TriggerMode.On,
                GetParameterDictionary(),
                GetAutoValueDictionary());
        }

        private IDictionary<ECameraParameter, CameraParameter> GetParameterDictionary()
        {
            var dictionary = new Dictionary<ECameraParameter, CameraParameter>();
            dictionary[ECameraParameter.Width] = new CameraParameter(
                    _camera.Parameters[PLCamera.Width].GetValue(),
                    _camera.Parameters[PLCamera.Width].GetMinimum(),
                    _camera.Parameters[PLCamera.Width].GetMaximum());

            dictionary[ECameraParameter.Height] = new CameraParameter(
                    _camera.Parameters[PLCamera.Height].GetValue(),
                    _camera.Parameters[PLCamera.Height].GetMinimum(),
                    _camera.Parameters[PLCamera.Height].GetMaximum());

            dictionary[ECameraParameter.OffsetX] = new CameraParameter(
                    _camera.Parameters[PLCamera.OffsetX].GetValue(),
                    _camera.Parameters[PLCamera.OffsetX].GetMinimum(),
                    _camera.Parameters[PLCamera.OffsetX].GetMaximum());

            dictionary[ECameraParameter.OffsetY] = new CameraParameter(
                    _camera.Parameters[PLCamera.OffsetY].GetValue(),
                    _camera.Parameters[PLCamera.OffsetY].GetMinimum(),
                    _camera.Parameters[PLCamera.OffsetY].GetMaximum());

            dictionary[ECameraParameter.Exposure] = new CameraParameter(
                    _camera.Parameters[PLCamera.ExposureTime].GetValue(),
                    _camera.Parameters[PLCamera.ExposureTime].GetMinimum(),
                    _camera.Parameters[PLCamera.ExposureTime].GetMaximum());

            dictionary[ECameraParameter.Gain] = new CameraParameter(
                    _camera.Parameters[PLCamera.Gain].GetValue(),
                    _camera.Parameters[PLCamera.Gain].GetMinimum(),
                    _camera.Parameters[PLCamera.Gain].GetMaximum());

            dictionary[ECameraParameter.FrameRate] = new CameraParameter(
                    _camera.Parameters[PLCamera.AcquisitionFrameRate].GetValue(),
                    _camera.Parameters[PLCamera.AcquisitionFrameRate].GetMinimum(),
                    _camera.Parameters[PLCamera.AcquisitionFrameRate].GetMaximum());

            dictionary[ECameraParameter.TriggerDelay] = new CameraParameter(
                    _camera.Parameters[PLCamera.TriggerDelay].GetValue(),
                    _camera.Parameters[PLCamera.TriggerDelay].GetMinimum(),
                    _camera.Parameters[PLCamera.TriggerDelay].GetMaximum());

            return dictionary;
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
            //Modify.ach.20210528.Camera ModelName Substring.Start...
            string m_Name = _camera.Parameters[PLCamera.DeviceModelName].GetValue();
            if ("c" == m_Name.Substring(m_Name.Length - 1, 1))
            {
                var whiteBalanceAuto = _camera.Parameters[PLCamera.BalanceWhiteAuto].GetValue();
            }
            //Modify.ach.20210528.Camera ModelName Substring.end
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
                case ECameraParameter.OffsetX:
                    return _camera.Parameters[PLCamera.OffsetX].TrySetValue((long)Math.Round(value));
                case ECameraParameter.OffsetY:
                    return _camera.Parameters[PLCamera.OffsetY].TrySetValue((long)Math.Round(value));
                case ECameraParameter.Width:
                    return _camera.Parameters[PLCamera.Width].TrySetValue((long)Math.Round(value));
                case ECameraParameter.Height:
                    return _camera.Parameters[PLCamera.Height].TrySetValue((long)Math.Round(value));
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
            if (GrabStarted != null)
                GrabStarted();

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

                if (_count > _grabCount)
                {
                    //Modify.Choijh.2021.05.27.Insert Disable Stop Grab Code.Start...
                    //Stop();
                    //Modify.Choijh.2021.05.27.Insert Disable Stop Grab Code.End...
                    if (GrabDone != null)
                        GrabDone();
                }
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