using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Comm
{
    public class SerialComm
    {
        private SerialPort _port;
        public Action<string> DataReceived { get; set; }

        public bool Connect(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _port = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _port.Open();
            _port.DataReceived += PortDataReceived;
            return _port.IsOpen;
        }

        private void PortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            DataReceived?.Invoke(port.ReadExisting());
        }

        public bool Write(string str)
        {
            if (_port == null ||_port.IsOpen == false)
                return false;

            _port.Write(str);

            return true;
        }


    }
}
