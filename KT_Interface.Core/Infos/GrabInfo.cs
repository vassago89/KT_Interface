using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Infos
{
    public enum EGrabResult
    {
        Success, Error, Timeout, NotConnected
    }

    public struct GrabInfo
    {
        public EGrabResult Result { get; }

        public int Width { get; }
        public int Height { get; }
        public int Channels { get; }

        public byte[] Data { get; }

        public GrabInfo(EGrabResult result)
        {
            Result = result;
            Width = -1;
            Height = -1;
            Channels = -1;
            Data = null;
        }

        public GrabInfo(EGrabResult result, int width, int height, int channels, byte[] data)
        {
            Result = result;
            Width = width;
            Height = height;
            Channels = channels;
            Data = data;
        }
    }
}
