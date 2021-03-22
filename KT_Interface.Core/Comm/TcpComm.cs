using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KT_Interface.Core.Comm
{
    public class TcpComm
    {
        private const int MAX_SIZE = 1024;
        
        private CancellationTokenSource _cts;

        private TcpListener _listener;
        private TcpClient _temp;

        private TcpClient _client;

        public Action<string> DataRecived { get; set; }

        public TcpComm()
        {

        }

        public async Task Connect(string ipAddress, int serverPort, int clientPort)
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Parse(ipAddress), serverPort);
            _client = new TcpClient(ipAddress, clientPort);

            _listener.Start();

            while (_cts.IsCancellationRequested == false)
            {
                _temp = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                Task.Factory.StartNew(AsyncTcpProcess, this);
            }
        }

        public void Disconnect()
        {
            _cts?.Cancel();
            _cts = null;
        }

        public bool Write(string message)
        {
            if (_cts == null || _cts.IsCancellationRequested)
                return false;

            byte[] buff = Encoding.ASCII.GetBytes(message);
            
            NetworkStream stream = _client.GetStream();

            stream.Write(buff, 0, buff.Length);
            stream.Close();

            return true;
        }

        private async static void AsyncTcpProcess(object o)
        {
            var tcpComm = (TcpComm)o;
            NetworkStream stream = tcpComm._temp.GetStream();

            // 비동기 수신            
            var buff = new byte[MAX_SIZE];
            var nbytes = await stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);
            if (nbytes > 0)
            {
                string message = Encoding.ASCII.GetString(buff, 0, nbytes);
                tcpComm.DataRecived?.Invoke(message);
            }

            stream.Close();
            tcpComm._temp.Close();
        }
    }
}
