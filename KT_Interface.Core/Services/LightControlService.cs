using KT_Interface.Core.Comm;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Services
{
    public class LightControlService 
    {
        private SerialComm _serialComm;
        private CoreConfig _coreConfig;
        private ILogger _logger;

        public LightControlService(CoreConfig coreConfig, LogFactory factory)
        {
            _serialComm = new SerialComm();
            _coreConfig = coreConfig;

            _logger = factory.GetCurrentClassLogger();
        }

        public bool Connect()
        {
            var info = _coreConfig.LightSerialInfo;
            try
            {
                return _serialComm.Connect(info.PortName, info.BaudRate, info.Parity, info.DataBits, info.StopBits);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }

        public bool SetValue(int value)
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }

        public int GetValue()
        {
            try
            {
                return 0;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return -1;
            }
        }
    }
}
