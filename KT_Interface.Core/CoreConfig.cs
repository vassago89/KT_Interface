using KT_Interface.Core.Infos;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core
{
    public enum ESaveMode
    {
        All, None, OK, NG
    }

    public class CoreConfig
    {
        public ESaveMode SaveMode { get; set; }

        public bool UseHost { get; set; }
        public bool UseInspector { get; set; }

        public int HostPort { get; set; }
        public int InspectorPort { get; set; }

        public CameraInfo CameraInfo { get; set; }
        public SerialInfo LightSerialInfo { get; set; }

        public string ResultPath { get; set; }
        public string TempPath { get; set; }
        public ImageFormat ImageFormat { get; set; }

        public int StoringDays { get; set; }

        public int LightNum { get; set; }
        public byte[] LightValues { get; set; }

        public CoreConfig()
        {
            LightSerialInfo = new SerialInfo();

            HostPort = 5555;
            InspectorPort = 4444;
            TempPath = "Temp";
            ImageFormat = ImageFormat.Bmp;
            StoringDays = 90;
            LightNum = 1;
        }
    }
}
