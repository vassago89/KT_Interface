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
        public EInspectionResult Result { get; }
        public string WaferID { get; }
        public string ImagePath { get; }

        public InspectionInfo(
            string waferID,
            string imagePath)
        {
            Result = EInspectionResult.Pass;
            WaferID = waferID;
            ImagePath = imagePath;
        }

        public override string ToString()
        {
            switch (Result)
            {
                case EInspectionResult.Pass:
                    return $"{Result},{WaferID},{ImagePath}";
                case EInspectionResult.NG:
                    throw new NotImplementedException();
            }

            return string.Empty;
        }
    }
}
