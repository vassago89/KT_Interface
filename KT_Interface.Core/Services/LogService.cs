using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Services
{
    public class LogService : TargetWithLayout
    {
        public Action<LogEventInfo> Recived { get; set; }

        public LogService(CoreConfig coreConfig)
        {
            if (Directory.Exists(coreConfig.LogPath) == false)
                Directory.CreateDirectory(coreConfig.LogPath);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (Recived != null)
                Recived(logEvent);
        }
    }
}
