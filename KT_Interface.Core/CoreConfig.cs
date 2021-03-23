using KT_Interface.Core.Infos;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core
{
    public class CoreConfig
    {
        public int HostPort { get; set; }
        public int InspectorPort { get; set; }

        public SerialInfo LightSerialInfo { get; set; }

        public string ResultPath { get; set; }
        public string TempPath { get; set; }
        public ImageFormat ImageFormat { get; set; }

        public CoreConfig()
        {
            HostPort = 5555;
            InspectorPort = 4444;
            TempPath = "Temp";
            ImageFormat = ImageFormat.Bmp;
        }
    }
}
