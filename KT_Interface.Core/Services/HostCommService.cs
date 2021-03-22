using KT_Interface.Core.Comm;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Services
{
    public class HostCommService
    {
        private TcpComm _tcpComm;
        private CoreConfig _coreConfig;
        private ILogger _logger;

        public HostCommService(CoreConfig coreConfig, LogFactory factory)
        {
            _tcpComm = new TcpComm();
            _tcpComm.DataRecived += DataRecived;
            _coreConfig = coreConfig;

            _logger = factory.GetCurrentClassLogger();
        }

        public bool Connect()
        {
            try
            {
                var info = _coreConfig.HostInfo;
                var task = _tcpComm.Connect(info.IPAdress, info.ServerPort, info.ClientPort);

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }

        private void DataRecived(string data)
        {
            var index = data.IndexOf('\n');
            if (index < 0 || index > data.Length)
                return;

            var message = data.Take(index);

        }
    }
}
