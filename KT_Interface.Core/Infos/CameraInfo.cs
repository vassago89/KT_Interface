using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Infos
{
    public enum ECameraType
    {
        USB, GIGE
    }

    public enum ECameraManufacturer
    {
        Hik, Basler
    }

    public class CameraInfo
    {
        public ECameraManufacturer Manufacturer { get; }
        public ECameraType CameraType { get; }
        public string ModelName { get; }
        public string SerialNo { get; }

        public CameraInfo(ECameraManufacturer manufacturer, ECameraType cameraType, string modelName, string serialNo)
        {
            Manufacturer = manufacturer;
            CameraType = cameraType;
            ModelName = modelName;
            SerialNo = serialNo;
        }
    }
}
