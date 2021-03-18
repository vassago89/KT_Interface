using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.CameraFactorys
{
    public class HikCameraFactory : ICameraFactory
    {
        public IEnumerable<CameraInfo> GetDevices()
        {
            var cameraInfos = new List<CameraInfo>();

            var stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();

            if (MyCamera.MV_OK != MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList))
                return cameraInfos;

            for (Int32 i = 0; i < stDevList.nDeviceNum; i++)
            {
                var stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                if (MyCamera.MV_GIGE_DEVICE == stDevInfo.nTLayerType)
                {
                    var stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    cameraInfos.Add(new CameraInfo(ECameraManufacturer.Hik, ECameraType.GIGE, stGigEDeviceInfo.chModelName, stGigEDeviceInfo.chSerialNumber));
                }
                else if (MyCamera.MV_USB_DEVICE == stDevInfo.nTLayerType)
                {
                    var stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    cameraInfos.Add(new CameraInfo(ECameraManufacturer.Hik, ECameraType.USB, stUsb3DeviceInfo.chModelName, stUsb3DeviceInfo.chSerialNumber));
                }
            }

            return cameraInfos;
        }


        public ICamera Connect(CameraInfo cameraInfo)
        {
            if (IsExist(cameraInfo) == false)
                return null;

            if (GetHikDeviceInfo(cameraInfo, out MyCamera.MV_CC_DEVICE_INFO stDevInfo) == false)
                return null;

            var device = new MyCamera();

            if (MyCamera.MV_OK != device.MV_CC_CreateDevice_NET(ref stDevInfo))
                return null;

            if (MyCamera.MV_OK != device.MV_CC_OpenDevice_NET())
                return null;

            if (cameraInfo.CameraType == ECameraType.GIGE)
            {
                int nPacketSize = device.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    if (MyCamera.MV_OK != device.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize))
                        return null;
                }
                else
                {
                    return null;
                }
            }

            var info = new MyCamera.MV_IMAGE_BASIC_INFO();

            if (MyCamera.MV_OK != device.MV_CC_GetImageInfo_NET(ref info))
                return null;
            
            var camera = new HikCamera(device, info);

            if (MyCamera.MV_OK != device.MV_CC_RegisterImageCallBackEx_NET(HikCamera.ImageCallback, GCHandle.ToIntPtr(camera.Handle)))
                return null;

            return camera;
        }

        public bool IsExist(CameraInfo cameraInfo)
        {
            if (cameraInfo.Manufacturer != ECameraManufacturer.Hik)
                return false;

            return GetHikDeviceInfo(cameraInfo, out MyCamera.MV_CC_DEVICE_INFO stDevInfo);
        }

        private static bool GetHikDeviceInfo(CameraInfo cameraInfo, out MyCamera.MV_CC_DEVICE_INFO stDevInfo)
        {
            stDevInfo = new MyCamera.MV_CC_DEVICE_INFO();

            var stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            var nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDevList);
            if (MyCamera.MV_OK != nRet)
                return false;

            for (Int32 i = 0; i < stDevList.nDeviceNum; i++)
            {
                stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                switch (cameraInfo.CameraType)
                {
                    case ECameraType.USB:
                        if (MyCamera.MV_USB_DEVICE == stDevInfo.nTLayerType)
                        {
                            var stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                            
                            if (cameraInfo.ModelName == stUsb3DeviceInfo.chModelName
                                && cameraInfo.SerialNo == stUsb3DeviceInfo.chSerialNumber)
                            {
                                return true;
                            }
                        }
                        break;
                    case ECameraType.GIGE:
                        if (MyCamera.MV_GIGE_DEVICE == stDevInfo.nTLayerType)
                        {
                            var stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                            
                            if (cameraInfo.ModelName == stGigEDeviceInfo.chModelName
                                && cameraInfo.SerialNo == stGigEDeviceInfo.chSerialNumber)
                            {
                                return true;
                            }
                        }
                        break;
                }
            }

            return false;
        }
    }
}
