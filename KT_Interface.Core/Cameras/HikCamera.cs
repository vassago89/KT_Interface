using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KT_Interface.Core.Infos;
using MvCamCtrl.NET;
namespace KT_Interface.Core.Cameras
{
    public class HikCamera : ICamera
    {
        private MyCamera _device;
        private MyCamera.MV_IMAGE_BASIC_INFO _info;
        private GCHandle _handle;
        public GCHandle Handle 
        { 
            get
            {
                return _handle;
            }
        }

        private int _count;
        private int _grabCount;
        public Action<GrabInfo> ImageGrabbed { get; set; }
        public Action GrabStarted { get; set; }
        public Action GrabDone { get; set; }

        private static MyCamera.cbOutputExdelegate _imageCallback;
        public static MyCamera.cbOutputExdelegate ImageCallback 
        { 
            get
            {
                return _imageCallback;
            }
        } 

        public HikCamera(MyCamera device, MyCamera.MV_IMAGE_BASIC_INFO info)
        {
            _imageCallback = new MyCamera.cbOutputExdelegate(ImageCallbackFunc);
            _handle = GCHandle.Alloc(this);

            _device = device;
            _info = info;
            
            _count = 0;
            _grabCount = -1;
        }

        ~HikCamera()
        {
            Handle.Free();
        }

        public bool IsConnected()
        {
            if (_device != null)
                return _device.MV_CC_IsDeviceConnected_NET();

            return false;
        }

        public bool StartGrab(int grabCount = -1)
        {
            if (GrabStarted != null)
                GrabStarted();

            _count = 0;
            _grabCount = grabCount;

            if (MyCamera.MV_OK != _device.MV_CC_StartGrabbing_NET())
                return false;

            return true;
        }

        public bool Stop()
        {
            if (MyCamera.MV_OK != _device.MV_CC_StopGrabbing_NET())
                return false;

            return true;
        }

        public bool Disconnect()
        {
            Stop();
            if (MyCamera.MV_OK != _device.MV_CC_CloseDevice_NET())
                return false;

            if (MyCamera.MV_OK != _device.MV_CC_DestroyDevice_NET())
                return false;

            return true;
        }

        private static void ImageCallbackFunc(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            var camera = (HikCamera)((GCHandle)pUser).Target;

            if (camera._grabCount > 0)
            {
                camera._count++;
                if (camera._count > camera._grabCount)
                {
                    camera.Stop();
                    if (camera.GrabDone != null)
                        camera.GrabDone();
                }
                    
            }

            if (camera.ImageGrabbed != null)
                camera.ImageGrabbed(ConvertImage(pFrameInfo, pData, camera._device));
        }
        
        private static GrabInfo ConvertImage(MyCamera.MV_FRAME_OUT_INFO_EX frameInfo, IntPtr pData, MyCamera device)
        {
            uint channel = frameInfo.nFrameLen / frameInfo.nWidth / frameInfo.nHeight;

            if (channel == 1)
            {
                var data = new byte[frameInfo.nFrameLen];
                Marshal.Copy(pData, data, 0, (int)frameInfo.nFrameLen);
                return new GrabInfo(EGrabResult.Success, frameInfo.nWidth, frameInfo.nHeight, 1, data);
            }
            else
            {
                var data = new byte[frameInfo.nWidth * frameInfo.nHeight * 3];
                
                var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

                var stConverPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                stConverPixelParam.nWidth = frameInfo.nWidth;
                stConverPixelParam.nHeight = frameInfo.nHeight;
                stConverPixelParam.pSrcData = pData;
                stConverPixelParam.nSrcDataLen = frameInfo.nFrameLen;
                stConverPixelParam.enSrcPixelType = frameInfo.enPixelType;
                stConverPixelParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                stConverPixelParam.pDstBuffer = handle.AddrOfPinnedObject();
                stConverPixelParam.nDstBufferSize = (uint)(frameInfo.nWidth * frameInfo.nHeight * 3);
                
                if (MyCamera.MV_OK != device.MV_CC_ConvertPixelType_NET(ref stConverPixelParam))
                {
                    handle.Free();
                    return new GrabInfo(EGrabResult.Error);
                }

                handle.Free();

                return new GrabInfo(EGrabResult.Success, frameInfo.nWidth, frameInfo.nHeight, 3, data);
            }
        }

        public bool SetParameter(ECameraParameter parameter, double value)
        {
            int nRet = 0;
            switch (parameter)
            {
                case ECameraParameter.OffsetX:
                    nRet = _device.MV_CC_SetAOIoffsetX_NET((uint)Math.Round(value));
                    break;
                case ECameraParameter.OffsetY:
                    nRet = _device.MV_CC_SetAOIoffsetY_NET((uint)Math.Round(value));
                    break;
                case ECameraParameter.Width:
                    nRet = _device.MV_CC_SetWidth_NET((uint)Math.Round(value));
                    break;
                case ECameraParameter.Height:
                    nRet = _device.MV_CC_SetHeight_NET((uint)Math.Round(value));
                    break;
                case ECameraParameter.Exposure:
                    nRet = _device.MV_CC_SetExposureTime_NET((float)value);
                    break;
                case ECameraParameter.Gain:
                    nRet = _device.MV_CC_SetGain_NET((float)value);
                    break;
                case ECameraParameter.FrameRate:
                    nRet = _device.MV_CC_SetFrameRate_NET((float)value);
                    break;
                case ECameraParameter.TriggerDelay:
                    nRet = _device.MV_CC_SetTriggerDelay_NET((float)value);
                    break;
            }

            return MyCamera.MV_OK == nRet;
        }

        public bool SetTriggerMode(bool isTriggerMode)
        {
            if (isTriggerMode)
                return MyCamera.MV_OK == _device.MV_CC_SetTriggerMode_NET((uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            else
                return MyCamera.MV_OK == _device.MV_CC_SetTriggerMode_NET((uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
        }

        public bool SetAuto(ECameraAutoType type, ECameraAutoValue value)
        {
            int nRet = 0;
            switch (type)
            {
                case ECameraAutoType.Exposure:
                    switch (value)
                    {
                        case ECameraAutoValue.Off:
                            nRet = _device.MV_CC_SetExposureAutoMode_NET(
                                (uint)MyCamera.MV_CAM_EXPOSURE_AUTO_MODE.MV_EXPOSURE_AUTO_MODE_OFF);
                            break;
                        case ECameraAutoValue.Once:
                            nRet = _device.MV_CC_SetExposureAutoMode_NET(
                                (uint)MyCamera.MV_CAM_EXPOSURE_AUTO_MODE.MV_EXPOSURE_AUTO_MODE_ONCE);
                            break;
                        case ECameraAutoValue.Continuous:
                            nRet = _device.MV_CC_SetExposureAutoMode_NET(
                                (uint)MyCamera.MV_CAM_EXPOSURE_AUTO_MODE.MV_EXPOSURE_AUTO_MODE_CONTINUOUS);
                            break;
                    }
                    break;
                case ECameraAutoType.Gain:
                    switch (value)
                    {
                        case ECameraAutoValue.Off:
                            nRet = _device.MV_CC_SetGainMode_NET(
                                (uint)MyCamera.MV_CAM_GAIN_MODE.MV_GAIN_MODE_OFF);
                            break;
                        case ECameraAutoValue.Once:
                            nRet = _device.MV_CC_SetGainMode_NET(
                                (uint)MyCamera.MV_CAM_GAIN_MODE.MV_GAIN_MODE_ONCE);
                            break;
                        case ECameraAutoValue.Continuous:
                            nRet = _device.MV_CC_SetGainMode_NET(
                                (uint)MyCamera.MV_CAM_GAIN_MODE.MV_GAIN_MODE_CONTINUOUS);
                            break;
                    }
                    break;
                case ECameraAutoType.WhiteBalance:
                    switch (value)
                    {
                        case ECameraAutoValue.Off:
                            nRet = _device.MV_CC_SetBalanceWhiteAuto_NET(
                                (uint)MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_OFF);
                            break;
                        case ECameraAutoValue.Once:
                            nRet = _device.MV_CC_SetBalanceWhiteAuto_NET(
                                (uint)MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_ONCE);
                            break;
                        case ECameraAutoValue.Continuous:
                            nRet = _device.MV_CC_SetBalanceWhiteAuto_NET(
                                (uint)MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_CONTINUOUS);
                            break;
                    }
                    
                    break;
            }

            return MyCamera.MV_OK == nRet;
        }

        public bool SetROI(uint x, uint y, uint width, uint height)
        {
            if (MyCamera.MV_OK != _device.MV_CC_SetAOIoffsetX_NET(x))
                return false;

            if (MyCamera.MV_OK != _device.MV_CC_SetAOIoffsetY_NET(y))
                return false;

            if (MyCamera.MV_OK != _device.MV_CC_SetWidth_NET(width))
                return false;

            if (MyCamera.MV_OK != _device.MV_CC_SetHeight_NET(height))
                return false;

            return true;
        }

        public CameraParameterInfo GetParameterInfo()
        {
            var triggerMode = new MyCamera.MVCC_ENUMVALUE();
            _device.MV_CC_GetTriggerMode_NET(ref triggerMode);

            return new CameraParameterInfo(
                triggerMode.nCurValue == (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON,
                GetParameterDictionary(),
                GetAutoValueDictionary());
        }

        private IDictionary<ECameraParameter, CameraParameter> GetParameterDictionary()
        {
            var dictionary = new Dictionary<ECameraParameter, CameraParameter>();
            var width = new MyCamera.MVCC_INTVALUE();
            var height = new MyCamera.MVCC_INTVALUE();
            var offsetX = new MyCamera.MVCC_INTVALUE();
            var offsetY = new MyCamera.MVCC_INTVALUE();
            var exposure = new MyCamera.MVCC_FLOATVALUE();
            var gain = new MyCamera.MVCC_FLOATVALUE();
            var frameRate = new MyCamera.MVCC_FLOATVALUE();
            var triggerDelay = new MyCamera.MVCC_FLOATVALUE();

            _device.MV_CC_GetWidth_NET(ref width);
            _device.MV_CC_GetHeight_NET(ref height);
            _device.MV_CC_GetAOIoffsetX_NET(ref offsetX);
            _device.MV_CC_GetAOIoffsetY_NET(ref offsetY);
            _device.MV_CC_GetExposureTime_NET(ref exposure);
            _device.MV_CC_GetGain_NET(ref gain);
            _device.MV_CC_GetFrameRate_NET(ref frameRate);
            _device.MV_CC_GetTriggerDelay_NET(ref triggerDelay);

            dictionary[ECameraParameter.Width] = new CameraParameter(width.nCurValue, width.nMin, width.nMax);
            dictionary[ECameraParameter.Height] = new CameraParameter(height.nCurValue, height.nMin, height.nMax);
            dictionary[ECameraParameter.OffsetX] = new CameraParameter(offsetX.nCurValue, offsetX.nMin, offsetX.nMax);
            dictionary[ECameraParameter.OffsetY] = new CameraParameter(offsetY.nCurValue, offsetY.nMin, offsetY.nMax);
            dictionary[ECameraParameter.Exposure] = new CameraParameter(exposure.fCurValue, exposure.fMin, exposure.fMax);
            dictionary[ECameraParameter.Gain] = new CameraParameter(gain.fCurValue, gain.fMin, gain.fMax);
            dictionary[ECameraParameter.FrameRate] = new CameraParameter(frameRate.fCurValue, frameRate.fMin, frameRate.fMax);
            dictionary[ECameraParameter.TriggerDelay] = new CameraParameter(triggerDelay.fCurValue, triggerDelay.fMin, triggerDelay.fMax);
            
            return dictionary;
        }

        private IDictionary<ECameraAutoType, ECameraAutoValue> GetAutoValueDictionary()
        {
            var dictionary = new Dictionary<ECameraAutoType, ECameraAutoValue>();

            var autoExposure = new MyCamera.MVCC_ENUMVALUE();
            var autoGain = new MyCamera.MVCC_ENUMVALUE();
            var autoWhiteBalance = new MyCamera.MVCC_ENUMVALUE();
            _device.MV_CC_GetExposureAutoMode_NET(ref autoExposure);
            _device.MV_CC_GetGainMode_NET(ref autoGain);
            _device.MV_CC_GetBalanceWhiteAuto_NET(ref autoWhiteBalance);

            switch ((MyCamera.MV_CAM_EXPOSURE_AUTO_MODE)autoExposure.nCurValue)
            {
                case MyCamera.MV_CAM_EXPOSURE_AUTO_MODE.MV_EXPOSURE_AUTO_MODE_OFF:
                    dictionary[ECameraAutoType.Exposure] = ECameraAutoValue.Off;
                    break;
                case MyCamera.MV_CAM_EXPOSURE_AUTO_MODE.MV_EXPOSURE_AUTO_MODE_ONCE:
                    dictionary[ECameraAutoType.Exposure] = ECameraAutoValue.Once;
                    break;
                case MyCamera.MV_CAM_EXPOSURE_AUTO_MODE.MV_EXPOSURE_AUTO_MODE_CONTINUOUS:
                    dictionary[ECameraAutoType.Exposure] = ECameraAutoValue.Continuous;
                    break;
            }

            switch ((MyCamera.MV_CAM_GAIN_MODE)autoGain.nCurValue)
            {
                case MyCamera.MV_CAM_GAIN_MODE.MV_GAIN_MODE_OFF:
                    dictionary[ECameraAutoType.Gain] = ECameraAutoValue.Off;
                    break;
                case MyCamera.MV_CAM_GAIN_MODE.MV_GAIN_MODE_ONCE:
                    dictionary[ECameraAutoType.Gain] = ECameraAutoValue.Once;
                    break;
                case MyCamera.MV_CAM_GAIN_MODE.MV_GAIN_MODE_CONTINUOUS:
                    dictionary[ECameraAutoType.Gain] = ECameraAutoValue.Continuous;
                    break;
            }

            switch ((MyCamera.MV_CAM_BALANCEWHITE_AUTO)autoWhiteBalance.nCurValue)
            {
                case MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_OFF:
                    dictionary[ECameraAutoType.WhiteBalance] = ECameraAutoValue.Off;
                    break;
                case MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_ONCE:
                    dictionary[ECameraAutoType.WhiteBalance] = ECameraAutoValue.Once;
                    break;
                case MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_CONTINUOUS:
                    dictionary[ECameraAutoType.WhiteBalance] = ECameraAutoValue.Continuous;
                    break;
            }

            return dictionary;
        }
    }
}