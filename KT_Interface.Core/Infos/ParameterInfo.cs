using KT_Interface.Core.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Infos
{
    public struct CameraParameter
    {
        private double _current;
        public double Current 
        { 
            get
            {
                return _current;
            }
        }

        private double _min;
        public double Min 
        { 
            get
            {
                return _min;
            }
        }

        private double _max;
        public double Max 
        { 
            get
            {
                return _max;
            }
        }

        public CameraParameter(double current, double min, double max)
        {
            _current = current;
            _min = min;
            _max = max;
        }
    }

    public class CameraParameterInfo
    {
        private CameraParameter _width;
        public CameraParameter Width 
        { 
            get
            {
                return _width;
            }
        }

        private CameraParameter _height;
        public CameraParameter Height 
        { 
            get
            {
                return _height;
            }
        }

        private CameraParameter _exposure;
        public CameraParameter Exposure 
        { 
            get
            {
                return _exposure;
            }
        }

        private CameraParameter _gain;
        public CameraParameter Gain 
        { 
            get
            {
                return _gain;
            }
        }

        private CameraParameter _frameRate;
        public CameraParameter FrameRate 
        { 
            get
            {
                return _frameRate;
            }
        }

        private CameraParameter _triggerDelay;
        public CameraParameter TriggerDelay 
        { 
            get
            {
                return _triggerDelay;
            }
        }

        private bool _onTriggerMode;
        public bool OnTriggerMode 
        { 
            get
            {
                return _onTriggerMode;
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
            CameraParameter width,
            CameraParameter height,
            CameraParameter exposure,
            CameraParameter gain,
            CameraParameter frameRate,
            CameraParameter triggerDelay,
            bool onTriggerMode,
            IDictionary<ECameraAutoType, ECameraAutoValue> autoValues)
        {
            _width = width;
            _height = height;
            _exposure = exposure;
            _gain = gain;
            _frameRate = frameRate;
            _triggerDelay = triggerDelay;
            _onTriggerMode = onTriggerMode;
            _autoValues = autoValues;
        }
    }
}
