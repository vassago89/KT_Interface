using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using static KT_Interface.Core.CameraFactorys.CameraFactory;

namespace KT_Interface.Core.CameraFactorys
{
    public class BaslerCameraFactory : ICameraFactory
    {
        public ICamera Connect(CameraInfo cameraInfo)
        {
            var baslerInfo = GetBaslerInfo(cameraInfo);

            if (baslerInfo == null)
                return null;
            
            return new BaslerCamera(baslerInfo);
        }

        private Basler.Pylon.ICameraInfo GetBaslerInfo(CameraInfo info)
        {
            if (info.Manufacturer != ECameraManufacturer.Basler)
                return null;

            List<Basler.Pylon.ICameraInfo> cameraInfos = Basler.Pylon.CameraFinder.Enumerate();
            foreach (var baslerInfo in cameraInfos)
            {
                var type = baslerInfo[Basler.Pylon.CameraInfoKey.DeviceType];
                switch (info.CameraType)
                {
                    case ECameraType.USB:
                        if (type == "BaslerUsb"
                            && baslerInfo[Basler.Pylon.CameraInfoKey.ModelName] == info.ModelName
                            && baslerInfo[Basler.Pylon.CameraInfoKey.SerialNumber] == info.SerialNo)
                            return baslerInfo;
                        break;
                    case ECameraType.GIGE:
                        if (type == "BaslerGigE"
                            && baslerInfo[Basler.Pylon.CameraInfoKey.ModelName] == info.ModelName
                            && baslerInfo[Basler.Pylon.CameraInfoKey.SerialNumber] == info.SerialNo)
                            return baslerInfo;
                        break;
                }
            }

            return null;
        }

        public IEnumerable<CameraInfo> GetDevices()
        {
            List<Basler.Pylon.ICameraInfo> baslerCameraInfos = Basler.Pylon.CameraFinder.Enumerate();
            List<CameraInfo> cameraInfos = new List<CameraInfo>();
            foreach (var info in baslerCameraInfos)
            {
                var type = info[Basler.Pylon.CameraInfoKey.DeviceType];
                if (type == "BaslerUsb")
                {
                    cameraInfos.Add(new CameraInfo(
                        ECameraManufacturer.Basler,
                        ECameraType.USB,
                        info[Basler.Pylon.CameraInfoKey.ModelName],
                        info[Basler.Pylon.CameraInfoKey.SerialNumber]));
                }
                else if (type == "BaslerGigE")
                {
                    cameraInfos.Add(new CameraInfo(
                        ECameraManufacturer.Basler,
                        ECameraType.GIGE,
                        info[Basler.Pylon.CameraInfoKey.ModelName],
                        info[Basler.Pylon.CameraInfoKey.SerialNumber]));
                }
            }

            return cameraInfos;
        }

        public bool IsExist(CameraInfo cameraInfo)
        {
            var infos = GetDevices();
            return infos.Any(i
                => i.Manufacturer == cameraInfo.Manufacturer
                && i.CameraType == cameraInfo.CameraType
                && i.ModelName == cameraInfo.ModelName
                && i.SerialNo == cameraInfo.SerialNo);
        }
    }
}
