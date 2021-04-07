using KT_Interface.Core.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Infos
{
    public class CameraParameter
    {
        public double Current { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

        public CameraParameter(double current, double min, double max)
        {
            Current = current;
            Min = min;
            Max = max;
        }
    }

    public class CameraParameterInfo
    {
        public bool OnTriggerMode { get; set; }

        private IDictionary<ECameraParameter, CameraParameter> _parameters;
        public IDictionary<ECameraParameter, CameraParameter> Parameters
        { 
            get
            {
                return _parameters;
            }
        }

        private IDictionary<ECameraAutoType, ECameraAutoValue> _autoValues;
        public IDictionary<ECameraAutoType, ECameraAutoValue> AutoValues 
        { 
            get
            {
                return _autoValues;
            }
        }

        public CameraParameterInfo(
            bool onTriggerMode,
            IDictionary<ECameraParameter, CameraParameter> parameters,
            IDictionary<ECameraAutoType, ECameraAutoValue> autoValues)
        {
            OnTriggerMode = onTriggerMode;
            _parameters = parameters;
            _autoValues = autoValues;
        }
    }
}
