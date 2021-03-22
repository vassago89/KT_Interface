using KT_Interface.Core.Infos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core
{
    public class CoreConfig
    {
        public TcpInfo HostInfo { get; set; }
        public TcpInfo InspectorInfo { get; set; }
        public SerialInfo LightSerialInfo { get; set; }
    }
}
