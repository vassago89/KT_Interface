using KT_Interface.Core.Infos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Cameras
{
    public enum ECameraParameter
    {
        Exposure, Gain, FrameRate, TriggerDelay
    }

    public enum ECameraAutoType
    {
        Exposure, Gain, WhiteBalance
    }

    public enum ECameraAutoValue
    {
        Off, Once, Continuous
    }

    public interface ICamera
    {
        Action<GrabInfo> ImageGrabbed { get; set; }
        bool StartGrab(int grabCount = -1);
        bool Stop();
        bool Disconnect();
        
        bool SetParameter(ECameraParameter parameter, double value);
        bool SetTriggerMode(bool isTriggerMode);
        bool SetAuto(ECameraAutoType type, ECameraAutoValue value);
        bool SetROI(uint x, uint y, uint width, uint height);

        CameraParameterInfo GetParameterInfo();
    }
}
