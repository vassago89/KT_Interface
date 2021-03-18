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
        public double Current { get; }
        public double Min { get; }
        public double Max { get; }

        public CameraParameter(double current, double min, double max)
        {
            Current = current;
            Min = min;
            Max = max;
        }
    }

    public class CameraParameterInfo
    {
        public CameraParameter Width { get; }
        public CameraParameter Height { get; }
        public CameraParameter Exposure { get; }
        public CameraParameter Gain { get; }
        public CameraParameter FrameRate { get; }
        public CameraParameter TriggerDelay { get; }

        public bool OnTriggerMode { get; }
        public IDictionary<ECameraAutoType, ECameraAutoValue> AutoValues { get; }

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
            Width = width;
            Height = height;
            Exposure = exposure;
            Gain = gain;
            FrameRate = frameRate;
            TriggerDelay = triggerDelay;
            OnTriggerMode = onTriggerMode;
            AutoValues = autoValues;
        }
    }
}
