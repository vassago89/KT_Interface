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
        private ECameraManufacturer _manufacturer;
        public ECameraManufacturer Manufacturer 
        { 
            get
            {
                return _manufacturer;
            }
        }

        private ECameraType _cameraType;
        public ECameraType CameraType
        {
            get
            {
                return _cameraType;
            }
        }

        private string _modelName;
        public string ModelName 
        { 
            get
            {
                return _modelName;
            }
        }

        private string _serialNo;
        public string SerialNo 
        { 
            get
            {
                return _serialNo;
            }
        }

        public CameraInfo(ECameraManufacturer manufacturer, ECameraType cameraType, string modelName, string serialNo)
        {
            _manufacturer = manufacturer;
            _cameraType = cameraType;
            _modelName = modelName;
            _serialNo = serialNo;
        }

        public override bool Equals(object obj)
        {
            var compare = obj as CameraInfo;
            if (compare == null)
                return false;

            if (Manufacturer == compare.Manufacturer
                && CameraType == compare.CameraType
                && ModelName == compare.ModelName
                && SerialNo == compare.SerialNo)
                return true;

            return false;
        }
    }
}
