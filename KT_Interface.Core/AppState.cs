using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core
{
    public class AppState
    {
        public bool OnManual { get; set; }
        public bool IsAutoEnabled { get; set; }
        public bool IsManualEnabled { get; set; }
        public bool IsGrabEnabled { get; set; }
    }
}
