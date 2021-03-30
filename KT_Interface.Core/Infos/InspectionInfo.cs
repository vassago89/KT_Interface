using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Infos
{
    public enum EInspectionResult
    {
        Pass, NG
    }

    public struct InspectionInfo
    {
        private EInspectionResult _result;
        public EInspectionResult Result 
        {
            get 
            {
                return _result;
            }
        }

        private string _waferID;
        public string WaferID 
        { 
            get
            {
                return _waferID;
            }
        }

        private string _imagePath;
        public string ImagePath 
        { 
            get
            {
                return _imagePath;
            }
        }

        public InspectionInfo(
            string waferID,
            string imagePath)
        {
            _result = EInspectionResult.Pass;
            _waferID = waferID;
            _imagePath = imagePath;
        }

        public override string ToString()
        {
            switch (Result)
            {
                case EInspectionResult.Pass:
                    return string.Format("{0},{1},{2}", Result, WaferID, ImagePath);
                case EInspectionResult.NG:
                    throw new NotImplementedException();
            }

            return string.Empty;
        }
    }
}
