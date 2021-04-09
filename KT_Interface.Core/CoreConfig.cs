using KT_Interface.Core.Infos;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
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
        private readonly string TempPath;

        public ESaveMode SaveMode { get; set; }

        public bool UseHost { get; set; }
        public bool UseInspector { get; set; }

        public int HostPort { get; set; }
        public int InspectorPort { get; set; }

        public CameraInfo CameraInfo { get; set; }
        public SerialInfo LightSerialInfo { get; set; }
        public int ReponseTimeout { get; set; }
        public int ResultTimeout { get; set; }

        public int CommCheckDelay { get; set; }

        public string ResultPath { get; set; }
        public string LogPath { get; set; }
        public ImageFormat ImageFormat { get; set; }

        public int ResultStoringDays { get; set; }
        public int LogStoringDays { get; set; }

        public int StoringCheckDelay { get; set; }

        public int LightNum { get; set; }
        public byte[] LightValues { get; set; }

        public int MaxLogCount { get; set; }

        public CoreConfig()
        {
            TempPath = "Temp";

            LightSerialInfo = new SerialInfo();

            HostPort = 5555;
            InspectorPort = 4444;
            ResultPath = "../Result";
            LogPath = "../Log";
            ImageFormat = ImageFormat.Bmp;
            ResultStoringDays = 90;
            LogStoringDays = 365;
            LightNum = 1;

            ReponseTimeout = 200;
            ResultTimeout = 2000;

            CommCheckDelay = 1000;

            StoringCheckDelay = 60;

            MaxLogCount = 1000;
        }

        public string GetTempPath()
        {
            return Path.Combine(ResultPath, TempPath);
        }
    }
}
