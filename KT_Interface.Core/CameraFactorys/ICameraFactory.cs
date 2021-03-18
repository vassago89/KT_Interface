using KT_Interface.Core.Cameras;
using KT_Interface.Core.Infos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.CameraFactorys
{
    public interface ICameraFactory
    {
        IEnumerable<CameraInfo> GetDevices();
        ICamera Connect(CameraInfo cameraInfo);
        bool IsExist(CameraInfo cameraInfo);
    }
}
